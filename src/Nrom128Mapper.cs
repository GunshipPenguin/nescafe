using System;

class Nrom128Mapper : Mapper {

  public Nrom128Mapper(Cartridge cartridge) {
    _cartridge = cartridge;
  }

  private ushort addressToPrgRomIndex(ushort address) {
    return (ushort) ((address - 0x8000) % 16384); // Rom maps start at address 0x8000, 0xC000-0xFFFF is a mirror of 0x8000-0xBFFF
  }

  public override byte readAddress(ushort address) {
    if (address < 0x2000) {
      return _cartridge.readChrRom(address);
    } else if (address > 0x8000) {
      return _cartridge.readPrgRom(addressToPrgRomIndex(address));
    } else {
      throw new Exception("Invalid mapper read");
    }
  }
}
