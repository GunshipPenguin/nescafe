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
    address = (ushort) ((address - 0x2000) % 0x1000);
    ushort index;
    if (console.cartridge.verticalVramMirroring) {
      index = address >= 0x2800 ? (ushort) (address - 0x800) : address;
    } else { // Horizontal Mirroring  
      index = address < 0x2800 ? (ushort) (address - 0x2000) : (ushort) (address - 0x2800);
      index %= 0x0400;
    }
    return index;
  }

  public ushort getPaletteRamIndex(ushort address) {
    return (ushort) ((address - 0x3F00) % 32);
  }

  public override byte read(ushort address) {
    byte data;
    if (address < 0x2000) { // CHR ROM pattern tables
      data = console.cartridge.readChrRom(address);
    } else if (address <= 0x2FFF) { // Internal vRam
      data = vRam[getVramIndex(address)];
    } else if (address >= 0x3F00 && address <= 0x3FFF) {
      data = paletteRam[getPaletteRamIndex(address)];
    } else {
      throw new Exception("Invalid PPU Memory read at address: " + address.ToString("x4"));
    }
    return data;
  }

  public override void write(ushort address, byte data) {
    if (address >= 0x2000 && address <= 0x3EFF) { // Internal vRam
      vRam[getVramIndex(address)] = data;
    } else if (address >= 0x3F00 && address <= 0x3FFF) {
      ushort addr = getPaletteRamIndex(address);
      paletteRam[addr] = data;
    } else {
      throw new Exception("Invalid PPU Memory write at address: " + address.ToString("x4"));
    }
  }
}