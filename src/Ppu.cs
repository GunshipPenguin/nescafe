using System;

public class Ppu {
  private byte[] bitmapData;
  public byte[] BitmapData {
    get {
      return bitmapData;
    }
  }

  CpuMemory _cpuMemory;
  PpuMemory _memory;
  Console _console;

  // OAM
  byte[] oam;
  ushort oamAddr;

  int scanline;
  int cycle;

  // Current nametable, attribute table and pattern bytes
  byte currNameTableByte;
  byte currAttributeTableByte;
  byte[] currBgPattern; // 2 bytes

  // Base nametable and pattern table address
  ushort baseNameTableAddr;
  ushort bgPatternTableAddress;

  // Vram increment per write to PPUDATA
  int vRamIncrement;

  // Last value written to a PPU register
  byte lastRegisterWrite;

  // Sprite related flags
  byte flagSpriteOverflow;
  byte flagSprite0Hit;

  // PPUCTRL Register flags
  byte flagBaseNameTableAddr;
  byte flagVRamIncrement;
  byte flagPatternTableAddr;
  byte flagBgPatternTableAddr;
  byte flagSpriteSize;
  byte flagMasterSlaveSelect;
  byte nmiOutput;

  // NMI Occurred flag
  byte nmiOccurred;

  // PPUMASK Register flags
  byte flagGreyscale;
  byte flagShowBackgroundLeft;
  byte flagShowSpritesLeft;
  byte flagShowBackground;
  byte flagShowSprites;
  byte flagEmphasizeRed;
  byte flagEmphasizeGreen;
  byte flagEmphasizeBlue;

  // Internal PPU Registers
  ushort v; // Current VRAM address (15 bits)
  ushort t; // Temporary VRAM address (15 bits)
  byte x; // Fine X scroll (3 bits)
  byte w; // First or second write toggle (1 bit)

  public Ppu(Console console) {
    _cpuMemory = console.cpuMemory;
    _memory = console.ppuMemory;
    _console = console;
    bitmapData = new byte[256 * 240];

    currBgPattern = new byte[2];

    scanline = 0;
    cycle = 0;

    nmiOccurred = 0;
    nmiOutput = 0;

    w = 0;

    oam = new byte[256];
  }

  byte lookupBackgroundColor(int paletteNum, int colorNum) {
    // Special case for universal background color
    if (colorNum == 0) {
      return _memory.read(0x3F00);
    }

    ushort paletteAddress;
    switch (paletteNum) {
      case 0: paletteAddress = (ushort) 0x3F01;
        break;
      case 1: paletteAddress = (ushort) 0x3F05;
        break;
      case 2: paletteAddress = (ushort) 0x3F09;
        break;
      case 3: paletteAddress = (ushort) 0x3F0D;
        break;
      default:
        throw new Exception("Invalid background palette Number: " + paletteNum.ToString());
    }

    paletteAddress += (ushort) colorNum;
    return _memory.read(paletteAddress);
  }

  byte getCurrAttributeBits() {
    bool isLeft = (coarseX() % 4) < 2;
    bool isTop = (coarseY() % 4) < 2;

    byte bits;
    if (isTop) {
      if (isLeft) { // Top left
        bits = (byte) (currAttributeTableByte >> 0);
      } else { // Top right
        bits = (byte) (currAttributeTableByte >> 2);
      }
    } else {
      if (isLeft) { // Bottom left
        bits = (byte) (currAttributeTableByte >> 4);
      } else { // Bottom right
        bits = (byte) (currAttributeTableByte >> 6);
      }
    }

    return (byte) (bits & 0x3);
  }

  // Gets the CHR of the current background pixel as specified in nametablebyte
  private byte getBackgroundPixelColor() {
    if (flagShowBackground == 0) {
      return 0;
    }

    // Create color number from bitfields in current background pattern
    byte loBit = (byte) (currBgPattern[0] >> (fineX() - 1));
    byte hiBit = (byte) (currBgPattern[1] >> (fineX() - 1));
    byte colorNum = (byte) (((hiBit << 1) | loBit) & 0x03);

    // Lookup and return color
    byte color = lookupBackgroundColor(getCurrAttributeBits(), colorNum);
    return color;
  }

  void copyHorizPositionData() {
    // v: ....F.. ...EDCBA = t: ....F.. ...EDCBA
    v = (ushort) ((v & 0x7BE0) | t);
  }

  void copyVertPositionData() {
    // v: IHGF.ED CBA..... = t: IHGF.ED CBA.....
    v = (ushort) ((v & 0x041F) | t);
  }

  void renderPixel() {
    int pixelX = cycle - 1;
    int pixelY = scanline - 1;

    byte color = getBackgroundPixelColor();
    bitmapData[pixelY * 256 + pixelX] = color;
  }

  int coarseX() {
    return v & 0x1f;
  }

  int fineX() {
    return (cycle + x) % 8;
  }

  int coarseY() {
    return (v >> 5) & 0x1f;
  }

  int fineY() {
    return (v >> 12) & 0x7;
  }

  void updateNameTableByte() {
    ushort currNameTableAddr = (ushort) (baseNameTableAddr + (ushort) (coarseY() * 32 + coarseX()));
    currNameTableByte = _memory.read(currNameTableAddr);
  }

  void updateBgPattern() {
    ushort patternAddress = (ushort) (bgPatternTableAddress + currNameTableByte * 16);

    ushort patternYAddr = (ushort) (patternAddress + fineY());
    currBgPattern[0] = _memory.read(patternYAddr);
    currBgPattern[1] = _memory.read((ushort) (patternYAddr + 8));
  }

  void updateAttributeTableByte() {
    int x = cycle - 1;
    int y = scanline - 1;

    // Atribute tables on 32x32 blocks
    int blockX = x / 32;
    int blockY = y / 32;

    int attributeByteIndex = (blockY * 8) + blockX;
    byte currAttributeTableByte = _memory.read((ushort) (baseNameTableAddr + 960 + attributeByteIndex)); // Attribute tables are 960 bytes from start of nametable
  }

  void incrementX() {
    if ((v & 0x001F) == 31) {
      v = (ushort) (v & (~0x001F)); // Reset Coarse X
      v = (ushort) (v ^ 0x0400); // Switch horizontal nametable
    } else {
      v ++; // Increment Coarse X
    }
  }

  void incrementY() {
    if ((v & 0x7000) != 0x7000) { // if fine Y < 7
      v += 0x1000; // increment fine Y
    } else {
      v = (ushort) (v & ~0x7000u & 0xFFFF); // Set fine Y to 0
      int y = (v & 0x03E0) >> 5; // y = coarse Y
      if (y == 29) {
        y = 0; // coarse Y = 0
        v = (ushort) (v ^ 0x0800); // switch vertical nametable
      } else if (y == 31) {
        y = 0; // coarse Y = 0, nametable not switched
      } else {
        y += 1; // Increment coarse Y
        v = (ushort) ((v & ~0x03E0) | (y << 5)); // Put coarse Y back into v
      }
    }
  }

  void handleRenderCycle() {
    int pixelX = cycle - 1;
    int pixelY = scanline - 1;

    // if (pixelX == 24 && pixelY == 16) {
    //   System.Console.Write("");
    // }

    // Fetch new rendering information if needed
    if (pixelX % 8 == 0 && cycle < 240) {
      incrementX();
      updateNameTableByte();
      updateBgPattern();
    }
    if (pixelX % 32 == 0) {
      updateAttributeTableByte();
    }
    // Increment Y in v register if needed
    if (cycle == 256) {
      incrementY();
    }

    if (cycle == 0) { // Do nothing, idle cycle

    } else if (cycle >= 1 && cycle <= 256) {
      // System.Console.WriteLine("(" + coarseX().ToString() + "." + fineX().ToString() + "," + coarseY().ToString() + "." + fineY().ToString() + ")");
      renderPixel();
    } else { // Cycles that fetch data for next scanline
      // TODO Implement fetching of data for next scanline
    }
  }

  public void step() {
    // Trigger an NMI at the start of scanline 241 if VBLANK NMI's are enabled
    if (scanline == 241 && cycle == 1) {
      nmiOccurred = 1;
      if (nmiOccurred != 0 && nmiOutput != 0) {
      _console.cpu.triggerNmi();
      }
    }

    if (scanline == 0) {
      nmiOccurred = 0;
    }

    bool renderingEnabled = (flagShowBackground != 0) || (flagShowSprites != 0);

    if (renderingEnabled) {
      if (scanline == 0) {  // Do nothing, dummy scanline

      } else if (scanline >= 1 && scanline < 240) { // Rendering scanlines
        handleRenderCycle();
      } else if (scanline == 240) {
        // Idle scanline
      } else if (scanline > 240 && scanline < 260) { // Memory fetch scanlines
        // TODO Add memory fetch stuff here
      } else if (scanline == 260) { // Vblank, next frame
        // TODO Add vblank stuff here
      }

    }

    cycle ++;

    if (renderingEnabled) {
      // Copy horizontal position data from t to v on cycle 257 of each scanline if rendering enabled
      if (cycle == 257) {
        copyHorizPositionData();
      } 
      
      // Copy vertical position data from t to v repeatedly from cycle 280 to 304 (if rendering is enabled)
      if (cycle >= 280 && cycle <= 304 && scanline == 0) {
        copyVertPositionData();
      }
    }

    // Reset cycle (and scanline if scanline == 260)
    // Also set to next frame if at end of last scanline
    if (cycle == 340) {
      if (scanline == 260) { // Last scanline, reset to upper left corner
        scanline = 0;
        cycle = 0;
        _console.drawFrame();
      } else { // Not on last scanline
        cycle = 0;
        scanline ++;
      }
    }
  }

  public byte[] getScreen() {
    return bitmapData;
  }

  public byte readFromRegister(ushort address) {
    byte data;
    switch (address) {
      case 0x2002: data = readPpuStatus();
        break;
      case 0x2004: data = readOamData();
        break;
      case 0x2007: data = readPpuData();
        break;
      default:
        throw new Exception("Invalid PPU Register read from register: " + address.ToString("X4"));
    }

    return data;
  }

  public void writeToRegister(ushort address, byte data) {
    lastRegisterWrite = data;
    switch (address) {
      case 0x2000: writePpuCtrl(data);
        break;
      case 0x2001: writePpuMask(data);
        break;
      case 0x2003: writeOamAddr(data);
        break;
      case 0x2004: writeOamData(data);
        break;
      case 0x2005: writePpuScroll(data);
        break;
      case 0x2006: writePpuAddr(data);
        break;
      case 0x2007: writePpuData(data);
        break;
      default:
        throw new Exception("Invalid PPU Register write to register: " + address.ToString("X4"));
    }
  }
  
  // $2000
  void writePpuCtrl(byte data) {
    flagBaseNameTableAddr = (byte) (data & 0x3);
    flagVRamIncrement = (byte) ((data >> 2) & 1);
    flagPatternTableAddr = (byte) ((data >> 3) & 1);
    flagBgPatternTableAddr = (byte) ((data >> 4) & 1);
    flagSpriteSize = (byte) ((data >> 5) & 1);
    flagMasterSlaveSelect = (byte) ((data >> 6) & 1);
    nmiOutput = (byte) ((data >> 7) & 1);

    // Set values based off flags
    baseNameTableAddr = (ushort) (0x2000 + 0x4000*flagBaseNameTableAddr);
    vRamIncrement = (flagVRamIncrement == 0) ? 1 : 32;
    bgPatternTableAddress = (ushort) (flagBgPatternTableAddr == 0 ? 0x0000 : 0x1000);

    // t: ...BA.. ........ = d: ......BA
    t = (ushort) ((t & 0xF3FF) | ((data & 0x03) << 10));
  }

  // $2001
  void writePpuMask(byte data) {
    flagGreyscale = (byte) (data & 1);
    flagShowBackgroundLeft = (byte) ((data >> 1) & 1);
    flagShowSpritesLeft = (byte) ((data >> 2) & 1);
    flagShowBackground = (byte) ((data >> 3) & 1);
    flagShowSprites = (byte) ((data >> 4) & 1);
    flagEmphasizeRed = (byte) ((data >> 5) & 1);
    flagEmphasizeGreen = (byte) ((data >> 6) & 1);
    flagEmphasizeBlue = (byte) ((data >> 7) & 1);
  }

  // $4014
  void writeOamAddr(byte data) {
    oamAddr = data;
  }

  // $2004
  void writeOamData(byte data) {
    oam[oamAddr] = data;
    oamAddr ++;
  }

  // $2005
  void writePpuScroll(byte data) {
    if (w == 0) { // First write
      // t: ....... ...HGFED = d: HGFED...
      // x:              CBA = d: .....CBA
      // w:                  = 1
      t = (ushort) ((t & 0xFFE0) | (data << 3));
      x = (byte) (data & 0x07);
      w = 1;
    } else {
      // t: CBA..HG FED..... = d: HGFEDCBA
      // w:                  = 0
      t = (ushort) (t & 0xC1F);
      t |= (ushort) ((data & 0x07) << 13); // CBA
      t |= (ushort) ((data & 0xF8) << 2); // HG FED
      w = 0;
    }
  }

  // $2006
  void writePpuAddr(byte data) {
    if (w == 0) { // First write
      // t: .FEDCBA ........ = d: ..FEDCBA
      // t: X...... ........ = 0
      // w:                  = 1
      t = (ushort) ((t & 0x00FF) | (data << 8));
      w = 1;
    } else {
      // t: ....... HGFEDCBA = d: HGFEDCBA
      // v                   = t
      // w:                  = 0
      t = (ushort) ((t & 0xFF00) | data);
      v = t;
      w = 0;
    }
  }

  // $2007
  void writePpuData(byte data) {
    _memory.write(v, data);
    v += (ushort) (vRamIncrement);
  }

  // $4014
  void writeOamDma(byte data) {
    throw new NotImplementedException();
  }

  // $2002
  byte readPpuStatus() {
    byte retVal = 0;
    retVal |= (byte) (lastRegisterWrite & 0x1F); // Least signifigant 5 bits of last register write
    retVal |= (byte) (flagSpriteOverflow << 5);
    retVal |= (byte) (flagSprite0Hit << 6);
    retVal |= (byte) (nmiOccurred << 7);

    // Old status of nmiOccurred is returned then nmiOccurred is cleared
    nmiOccurred = 0;

    // w:                  = 0
    w = 0;

    return retVal;
  }

  // $2004
  byte readOamData() {
    return oam[oamAddr];
  }

  // $2007
  byte readPpuData() {
    return _memory.read(v);
    v += (ushort) (vRamIncrement);
  }
}