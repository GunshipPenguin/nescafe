using System;

public class Memory {
  // First 2KB of internal ram
  byte[] internalRam = new byte[2048];

  Mapper mapper;

  public Memory(Cartridge cartridge) {
    mapper = new Nrom128Mapper(cartridge);
  }

  public byte read(ushort address) {
    if (address > 0x1FFF && address < 0x4020) {
      throw new NotImplementedException();
    }

    byte data;

    if (address < 0x1FFF) {
      ushort addressIndex = handleInternalRamMirror(address);
      data = internalRam[addressIndex];
    } else {
      data = mapper.readAddress(address);
    }

    return data;
  }

  public void readBuf(byte[] buffer, ushort address, ushort size) {
    for (int bytesRead=0;bytesRead<size;bytesRead++) {
      ushort readAddr = (ushort) (address + bytesRead);
      buffer[bytesRead] = read(readAddr);
    }
  }

  public ushort read16(ushort address) {
    byte lo = read(address);
    byte hi = read((ushort) (address + 1));
    return (ushort) ((hi << 8) | lo);
  }

  public void write(ushort address, byte data) {
    if (address > 0x1FFF) {
      throw new NotImplementedException();
    }

    ushort addressIndex = handleInternalRamMirror(address);
    internalRam[addressIndex] = data;
  }

  public void write16(ushort address, ushort data) {
    byte hi = (byte) (data >> 8);
    byte lo = (byte) (data & 0xFF);
    write(address, hi);
    write(address, lo);
  }

  // Return the index in internalRam of the address (handle mirroring)
  private ushort handleInternalRamMirror(ushort address) {
    return (ushort) (address & 0x800);
  }
}
