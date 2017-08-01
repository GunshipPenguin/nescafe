using System;

public class CpuMemory : Memory {
  // First 2KB of internal ram
  byte[] internalRam = new byte[2048];

  Mapper mapper;

  public CpuMemory(Cartridge cartridge) {
    mapper = new Nrom128Mapper(cartridge);
  }

  public override void write(ushort address, byte data) {
    if (address > 0x1FFF) {
      throw new NotImplementedException();
    }

    ushort addressIndex = handleInternalRamMirror(address);
    internalRam[addressIndex] = data;
  }

  public override byte read(ushort address) {
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

  // Return the index in internalRam of the address (handle mirroring)
  private ushort handleInternalRamMirror(ushort address) {
    return (ushort) (address % 0x800);
  }
}