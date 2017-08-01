using System;

public class Ppu {
  CpuMemory _cpuMemory;
  PpuMemory _memory;

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

  // OAMADDR Register
  byte oamAddr;


  public Ppu(Console console) {
    _cpuMemory = console.cpuMemory;
    _memory = console.ppuMemory;
  }

  public void writePpuCtrl(byte data) {
    flagBaseNameTableAddr = (byte) (data & 0x3);
    flagVRamIncrement = (byte) ((data >> 2) & 1);
    flagPatternTableAddr = (byte) ((data >> 3) & 1);
    flagBgPatternTableAddr = (byte) ((data >> 4) & 1);
    flagSpriteSize = (byte) ((data >> 5) & 1);
    flagMasterSlaveSelect = (byte) ((data >> 6) & 1);
    flagVBlankNmi = (byte) ((data >> 7) & 1);
  }

  public void writePpuMask(byte data) {
    flagGreyscale = (byte) (data & 1);
    flagShowBackgroundLeft = (byte) ((data >> 1) & 1);
    flagShowSpritesLeft = (byte) ((data >> 2) & 1);
    flagShowBackground = (byte) ((data >> 3) & 1);
    flagShowSprites = (byte) ((data >> 4) & 1);
    flagEmphasizeRed = (byte) ((data >> 5) & 1);
    flagEmphasizeGreen = (byte) ((data >> 6) & 1);
    flagEmphasizeBlue = (byte) ((data >> 7) & 1);
  }

  public void writePpuStatus(byte data) {

  }

  public void writeOamAddr(byte data) {
    oamAddr = data;
  }

  public void writeOamData(byte data) {

  }

  public void writePpuScroll(byte data) {

  }

  public void writePpuAddr(byte data) {
    
  }

  public void writePpuData(byte data) {

  }

  public void writeOamDma(byte data) {

  }
}