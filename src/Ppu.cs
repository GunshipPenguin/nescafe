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

  // Gets the CHR of the current background pixel as specified in nametablebyte
  private byte fetchBackgroundPattern(int x, int y) {
    int patternX = x % 8;
    int patternY = y % 8;

    // Load base pattern table address
    ushort patternAddress = (ushort) (flagBgPatternTableAddr == 0 ? 0x0000 : 0x1000);
    patternAddress += (ushort) (nameTableByte);

    // Fetch low and high byte
    byte loByte = _memory.read((ushort) (patternAddress + patternY));
    byte hiByte = _memory.read((ushort) (patternAddress + patternY + 8));

    // Get lo and hi bits
    byte loBit = (byte) (loByte >> patternX);
    byte hiBit = (byte) (loByte >> patternX);

    // Return lower 2 bits of color (TODO: Add attribute table lookup for full color)
    return (byte) (((hiBit << 1) | loBit) & 0x03);
  }

  private void renderPixel() {
    int pixelX = cycle;
    int pixelY = scanline;

    byte lowerTwoBits = fetchBackgroundPattern(pixelX, pixelY);
    bitmapData[pixelY * 240 + pixelX] = lowerTwoBits;
  }

  private void fetchNameTableByte() {
    ushort address;

    // Set location to base location initially
    switch (flagBaseNameTableAddr) {
      case 0: address = 0x2000;
        break;
      case 1: address = 0x2400;
        break;
      case 2: address = 0x2800;
        break;
      case 3: address = 0x2C00;
        break;
      default:
        throw new Exception("Invalid base nametable address");
    }

    int pixelX = cycle;
    int pixelY = scanline;

    // Tiles are 8x8 pixels
    int tileX = pixelX / 8; 
    int tileY = pixelY / 8;

    address += (ushort) ((240 * tileY + tileX) / 8);
    nameTableByte = _memory.read(address);
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
    flagBaseNameTableAddr = (byte) (data & 0x3);
    flagVRamIncrement = (byte) ((data >> 2) & 1);
    flagPatternTableAddr = (byte) ((data >> 3) & 1);
    flagBgPatternTableAddr = (byte) ((data >> 4) & 1);
    flagSpriteSize = (byte) ((data >> 5) & 1);
    flagMasterSlaveSelect = (byte) ((data >> 6) & 1);
    flagVBlankNmi = (byte) ((data >> 7) & 1);
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

    return retVal;
  }

  byte readOamData() {
    return oam[oamAddr];
  }

  byte readPpuData() {
    return _memory.read(ppuAddr);
  }
}