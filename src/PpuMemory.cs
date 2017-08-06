using System;

public class PpuMemory : Memory {
  Console console;
  byte[] vRam;
  byte[] paletteRam;

  public PpuMemory (Console console) {
    this.console = console;
    vRam = new byte[2048];
    paletteRam = new byte[32];
  }

  public ushort getVramIndex(ushort address) {
    return (ushort) (address - 0x2000);
  }

  public ushort getPaletteRamIndex(ushort address) {
    return (ushort) (address - 0x3F00);
  }

  public override byte read(ushort address) {
    byte data;
    if (address < 0x2000) { // CHR ROM pattern tables
      data = console.cartridge.readChrRom(address);
    } else if (address <= 0x2FFF) { // Internal vRam
      data = vRam[getVramIndex(address)];
    } else if (address >= 0x3F00 && address <= 0x3F1F) {
      data = paletteRam[getPaletteRamIndex(address)];
    } else {
      throw new Exception("Invalid PPU Memory read at address: " + address.ToString("x4"));
    }
    return data;
  }

  public override void write(ushort address, byte data) {
    if (address >= 0x2000 && address <= 0x2FFF) { // Internal vRam
      vRam[getVramIndex(address)] = data;
    } else if (address >= 0x3F00 && address < 0x3F1F) {
      paletteRam[getPaletteRamIndex(address)] = data;
    } else {
      throw new Exception("Invalid PPU Memory read at address: " + address.ToString("x4"));
    }
  }
}