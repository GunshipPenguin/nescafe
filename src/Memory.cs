using System;

public abstract class Memory {
  public abstract byte read(ushort address);
  public abstract void write(ushort address, byte data);

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
}