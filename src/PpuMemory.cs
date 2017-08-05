using System;

public class PpuMemory : Memory {
  Console console;
  byte[] vRam;

  public PpuMemory (Console console) {
    this.console = console;
    vRam = new byte[2048];
  }

  public override byte read(ushort address) {
    if (address < 0x2000) { // CHR ROM pattern tables
      return console.ppuMemory.read(address);
    } else if (address <= 0x2FFF) { // Internal vRam
      return vRam[address - 0x2000];
    } else {
      throw new Exception("Invalid PPU Memory read at address: " + address.ToString("x4"));
    }
  }

  public override void write(ushort address, byte data) {
    if (address < 0x2FFF) { // Internal vRam
      vRam[address] = data;
    } else {
      throw new Exception("Invalid PPU Memory read at address: " + address.ToString("x4"));
    }
  }
}