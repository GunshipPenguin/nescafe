using System;

public class CpuMemory : Memory {
  // First 2KB of internal ram
  byte[] internalRam = new byte[2048];

  Mapper mapper;
  Console console;

  public CpuMemory(Console console) {
    mapper = new Nrom128Mapper(console.cartridge);
    this.console = console;
  }

  // Return the index in internalRam of the address (handle mirroring)
  private ushort handleInternalRamMirror(ushort address) {
    return (ushort) (address % 0x800);
  }

  private ushort getPpuRegisterFromAddress(ushort address) {
    return (ushort) (0x2000 + ((address - 0x2000) % 8));
  }

  private void writePpuRegister(ushort address, byte data) {
    console.ppu.writeToRegister(getPpuRegisterFromAddress(address), data);
  }

  private byte readPpuRegister(ushort address) {
    return console.ppu.readFromRegister(getPpuRegisterFromAddress(address));
  }

  public override byte read(ushort address) {
    byte data;
    if (address < 0x2000) { // Internal CPU RAM 
      ushort addressIndex = handleInternalRamMirror(address);
      data = internalRam[addressIndex];
    } else if (address < 0x1FF9) { // PPU Registers
      data = readPpuRegister(address);
    } else if (address >= 0x4020) { // Program ROM
      data = mapper.readAddress(address);
    } else {
      throw new NotImplementedException("Invalid CPU Memory Read from address: " + address.ToString("X4"));
    }

    return data;
  }

    public override void write(ushort address, byte data) {
    if (address < 0x2000) { // Internal CPU RAM 
      ushort addressIndex = handleInternalRamMirror(address);
      internalRam[addressIndex] = data;
    } else if (address < 0x1FF9 || address == 0x4014) { // PPU Registers
      writePpuRegister(address, data);
    } else {
      throw new NotImplementedException("Invalid CPU Memory Write to address: " + address.ToString("X4"));
    }
  }
}