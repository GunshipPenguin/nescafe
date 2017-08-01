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

  public override void write(ushort address, byte data) {
    if (address < 0x2000) { // Internal CPU RAM 
      ushort addressIndex = handleInternalRamMirror(address);
      internalRam[addressIndex] = data;
    } else if (address < 0x1FF9) { // PPU Registers
      writePpuRegister(address, data);
    } else if (address == 0x4014) {
      console.ppu.writeOamDma(data);  
    } else {
      throw new NotImplementedException("Invalid CPU Memory Write to address: " + address.ToString("X4"));
    }
  }

  private void writePpuRegister(ushort address, byte data) {
    ushort registerAddress = (byte) (0x2000 + ((address - 0x2000) % 8));
    switch (registerAddress) {
      case 0x2000: console.ppu.writePpuCtrl(data);
        break;
      case 0x2001: console.ppu.writePpuMask(data);
        break;
      case 0x2003: console.ppu.writeOamAddr(data);
        break;
      case 0x2004: console.ppu.writeOamData(data);
        break;
      case 0x2005: console.ppu.writePpuScroll(data);
        break;
      case 0x2006: console.ppu.writePpuData(data);
        break;
      case 0x2007: console.ppu.writePpuData(data);
        break;
      default:
        throw new Exception("Invalid PPU Register write to register: " + registerAddress.ToString("X4"));
    }
  }

  private byte readPpuRegister(ushort address) {
    ushort registerAddress = (byte) (0x2000 + ((address - 0x2000) % 8));
    byte data;
    switch (registerAddress) {
      case 0x2002: data = console.ppu.readPpuStatus();
        break;
      case 0x2004: data = console.ppu.readOamData();
        break;
      case 0x2007: data = console.ppu.readPpuData();
        break;
      default:
        throw new Exception("Invalid PPU Register read from register: " + registerAddress.ToString("X4"));
    }

    return data;
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

  // Return the index in internalRam of the address (handle mirroring)
  private ushort handleInternalRamMirror(ushort address) {
    return (ushort) (address % 0x800);
  }
}