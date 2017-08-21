using System;

namespace Nescafe.Mappers
{
    class NromMapper : Mapper
    {
        public NromMapper(Cartridge cartridge)
        {
            _cartridge = cartridge;
        }

        ushort AddressToPrgRomIndex(ushort address)
        {
            ushort mappedAddress = (ushort)(address - 0x8000); // PRG banks start at 0x8000
            return _cartridge.PrgRomBanks == 1 ? (ushort)(mappedAddress % 16384) : mappedAddress; // Wrap if only 1 PRG bank
        }

        public override byte Read(ushort address)
        {
            if (address < 0x2000) // CHR rom stored from $0000 to $1FFF
            {
                return _cartridge.ReadChr(address);
            }
            else if (address >= 0x8000) // PRG ROM stored at $8000 and above
            {
                return _cartridge.ReadPrgRom(AddressToPrgRomIndex(address));
            }
            else
            {
                throw new Exception("Invalid mapper read");
            }
        }

        public override void Write(ushort address, byte data)
        {
            if (address < 0x2000) // CHR RAM
            {
                _cartridge.WriteChr(address, data);
            }
        }
    }    
}
