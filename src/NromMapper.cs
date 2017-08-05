using System;

class NromMapper : Mapper {
  public NromMapper(Cartridge cartridge) {
    _cartridge = cartridge;
  }

  private ushort addressToPrgRomIndex(ushort address) {
    ushort mappedAddress = (ushort) (address - 0x8000); // PRG banks start at 0x8000
    return _cartridge.prgRomBanks == 1 ? (ushort) (mappedAddress % 16384) : mappedAddress; // Wrap if only 1 PRG bank
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
