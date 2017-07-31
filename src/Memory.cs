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

  // Reads 2 bytes, wrapping around to the start of the page if lower byte is at beginning
  // Eg reading from 0x0AFF reads 0x0AFF first and 0x0A00 second
  public ushort read16WrapPage(ushort address) {
    ushort data;
    if ((address & 0xFF) == 0xFF) {
      byte lo = read(address);
      byte hi = read((ushort) (address & (~0xFF))); // Wrap around to start of page eg. 0x02FF becomes 0x0200
      data = (ushort) ((hi << 8) | lo);
    } else {
      data = read16(address);
    }
    return data;
  }

  public void write(ushort address, byte data) {
    if (address > 0x1FFF) {
      throw new NotImplementedException();
    }

    ushort addressIndex = handleInternalRamMirror(address);
    internalRam[addressIndex] = data;
  }

  // Return the index in internalRam of the address (handle mirroring)
  private ushort handleInternalRamMirror(ushort address) {
    return (ushort) (address % 0x800);
  }
}
