using System;
namespace Nescafe.Mappers
{
    public class UxRomMapper : Mapper
    {
        int _bank0Offset;
        int _bank1Offset;

        public UxRomMapper(Cartridge cartridge)
        {
            _cartridge = cartridge;

            // PRG Bank 0 is switchable
            _bank0Offset = 0;

            // PRG Bank 1 is always fixed to the last bank
            _bank1Offset = (cartridge.PrgRomBanks - 1) * 0x4000;

            _vramMirroringType = _cartridge.VerticalVramMirroring ? VramMirroring.Vertical : VramMirroring.Horizontal;
        }

        public override byte Read(ushort address)
        {
            byte data;
            if (address < 0x2000) // CHR ROM or RAM
            {
                data = _cartridge.ReadChr(address);
            }
            else if (address >= 0x6000 && address < 0x8000)
            {
                // Open Bus
                data = 0x00;
            }
            else if (address <= 0xC000) // PRG ROM bank 0
            {
                data = _cartridge.ReadPrgRom(_bank0Offset + (address - 0x8000));
            }
            else if (address <= 0xFFFF) // PRG ROM bank 1
            {
                data = _cartridge.ReadPrgRom(_bank1Offset + (address - 0xC000));
            }
            else
            {
                throw new Exception("Invalid mapper read at address: " + address.ToString("X4"));
            }
            return data;
        }

        public override void Write(ushort address, byte data)
        {
            if (address < 0x2000) // CHR ROM or RAM
            {
                _cartridge.WriteChr(address, data);
            }
            else if (address >= 0x6000 && address < 0x8000)
            {
                // Open Bus
            }
            else if (address >= 0x8000)
            {
                WriteBankSelect(data);
            }
            else
            {
                throw new Exception("Invalid mapper write at address: " + address.ToString("X4"));
            }
        }

        void WriteBankSelect(byte data)
        {
            _bank0Offset = (data & 0x0F) * 0x4000;
        }
    }
}
