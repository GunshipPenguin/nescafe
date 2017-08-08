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

  // Current nametable byte
  byte nameTableByte;

  ushort baseNameTableAddr;
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
  byte flagVBlankNmi;

  // PPUMASK Register flags
  byte flagGreyscale;
  byte flagShowBackgroundLeft;
  byte flagShowSpritesLeft;
  byte flagShowBackground;
  byte flagShowSprites;
  byte flagEmphasizeRed;
  byte flagEmphasizeGreen;
  byte flagEmphasizeBlue;

  // VRAM access by CPU
  ushort ppuAddr;
  bool expectingPpuAddrLo; // Set if the PPUADDR register is expecting the low byte

  public Ppu(Console console) {
    _cpuMemory = console.cpuMemory;
    _memory = console.ppuMemory;
    _console = console;
    bitmapData = new byte[256 * 240];

    expectingPpuAddrLo = false;

    scanline = 0;
    cycle = 0;

    oam = new byte[256];
  }

  byte getBackgroundColor(int paletteNum, int colorNum) {
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

  byte getAttributeBitsFromCoords(int x, int y, int attributeByte) {
    bool isLeft = (x % 16) < 8;
    bool isTop = (y % 16) < 8;

    byte bits;
    if (isTop) {
      if (isLeft) { // Top left
        bits = (byte) (attributeByte >> 0);
      } else { // Top right
        bits = (byte) (attributeByte >> 2);
      }
    } else {
      if (isLeft) { // Bottom left
        bits = (byte) (attributeByte >> 4);
      } else { // Bottom right
        bits = (byte) (attributeByte >> 6);
      }
    }

    return (byte) (bits & 0x3);
  }

  // Gets the CHR of the current background pixel as specified in nametablebyte
  private byte fetchBackgroundPattern(int x, int y) {
    // X and Y pixel coordinates relative to the pattern in the pattern table
    int patternX = x % 8;
    int patternY = y % 8;

    // Load base pattern table address, get pattern address
    ushort patternTableBaseAddress = (ushort) (flagBgPatternTableAddr == 0 ? 0x0000 : 0x1000);
    ushort patternAddress = (ushort) (patternTableBaseAddress + nameTableByte);

    // Fetch low and high byte of the pixel in the pattern
    byte loByte = _memory.read((ushort) (patternAddress + patternY));
    byte hiByte = _memory.read((ushort) (patternAddress + patternY + 8));

    // Create color number
    byte loBit = (byte) (loByte >> patternX);
    byte hiBit = (byte) (hiByte >> patternX);
    byte colorNum = (byte) (((hiBit << 1) | loBit) & 0x03);

    // Grab palette data from attribute table
    int blockX = x / 32;
    int blockY = y / 32;
    int attributeByteIndex = (blockY * 8) + blockX;
    ushort attributeTableEntry = _memory.read((ushort) (baseNameTableAddr + 960 + attributeByteIndex)); // Attribute tables are 960 bytes from start of nametable
    byte attributeBits = getAttributeBitsFromCoords(x, y, attributeTableEntry);

    // Lookup and return color
    byte color = getBackgroundColor(attributeBits, colorNum);
    return color;
  }

  private void renderPixel() {
    int pixelX = cycle;
    int pixelY = scanline;

    byte color = fetchBackgroundPattern(pixelX, pixelY);
    bitmapData[pixelY * 240 + pixelX] = color;
  }

  private void fetchNameTableByte() {
    // Set location to base location initially
    switch (flagBaseNameTableAddr) {
      case 0: baseNameTableAddr = 0x2000;
        break;
      case 1: baseNameTableAddr = 0x2400;
        break;
      case 2: baseNameTableAddr = 0x2800;
        break;
      case 3: baseNameTableAddr = 0x2C00;
        break;
      default:
        throw new Exception("Invalid base nametable address");
    }

    int pixelX = cycle;
    int pixelY = scanline;

    // Tiles are 8x8 pixels
    int tileX = pixelX / 8; 
    int tileY = pixelY / 8;

    ushort currNameTableAddr = (ushort) (baseNameTableAddr + (ushort) ((240 * tileY + tileX) / 8));
    nameTableByte = _memory.read(currNameTableAddr);
  }

  public void step() {
    // System.Console.Write("scanline:" + scanline.ToString() + " ");
    // System.Console.Write("cycle:" + cycle.ToString());
    // System.Console.Write("\n");
    if (scanline == 0) {  // Do nothing, dummy scanline

    } else if (scanline >= 1 && scanline <= 240) { // Rendering scanlines
      if (cycle == 0) { // Do nothing, idle cycle

      } else if (cycle >= 1 && cycle <= 256) {
        if (cycle % 8 == 0) { // Fetch new rendering information if needed
          fetchNameTableByte();
        }
        
        renderPixel();
      } else { // Cycles that fetch data for next scanline
        // TODO Implement fetching of data for next scanline
      }
    } else if (scanline > 240 && scanline < 260) { // Memory fetch scanlines
      if (scanline == 241 && cycle == 1) {
        if (flagVBlankNmi != 0) {
          _console.cpu.triggerNmi();
        }
      }
      // TODO Add memory fetch stuff here
    } else if (scanline == 260) { // Vblank, next frame
      // TODO Add vblank stuff here
    }

    // Increment cycle and scanline if cycle == 340
    // Also set to next frame if at end of last scanline
    cycle ++;
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
  
  void writePpuCtrl(byte data) {
    System.Console.WriteLine(data);
    flagBaseNameTableAddr = (byte) (data & 0x3);
    flagVRamIncrement = (byte) ((data >> 2) & 1);
    flagPatternTableAddr = (byte) ((data >> 3) & 1);
    flagBgPatternTableAddr = (byte) ((data >> 4) & 1);
    flagSpriteSize = (byte) ((data >> 5) & 1);
    flagMasterSlaveSelect = (byte) ((data >> 6) & 1);
    flagVBlankNmi = (byte) ((data >> 7) & 1);

    vRamIncrement = (flagVRamIncrement == 0) ? 1 : 32;
  }

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

  void writeOamAddr(byte data) {
    oamAddr = data;
  }

  void writeOamData(byte data) {
    oam[oamAddr] = data;
    oamAddr ++;
  }

  void writePpuScroll(byte data) {
    throw new NotImplementedException();
  }

  void writePpuAddr(byte data) {
    if (expectingPpuAddrLo) {
      ppuAddr |= data;
    } else {
      ppuAddr |= (ushort) (data << 8);
    }
    expectingPpuAddrLo = !expectingPpuAddrLo;
  }

  void writePpuData(byte data) {
    _memory.write(ppuAddr, data);
    ppuAddr += (ushort) (vRamIncrement);
  }

  void writeOamDma(byte data) {
    throw new NotImplementedException();
  }

  byte readPpuStatus() {
    byte retVal = 0;
    retVal |= (byte) (lastRegisterWrite & 0x1F); // Least signifigant 5 bits of last register write
    retVal |= (byte) (flagSpriteOverflow << 5);
    retVal |= (byte) (flagSprite0Hit << 6);
    retVal |= (byte) ((scanline > 240 ? 1 : 0) << 7);

    ppuAddr = 0;

    return retVal;
  }

  byte readOamData() {
    return oam[oamAddr];
  }

  byte readPpuData() {
    return _memory.read(ppuAddr);
  }
}