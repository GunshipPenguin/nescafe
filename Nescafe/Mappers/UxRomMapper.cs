using System;
namespace Nescafe.Mappers
{
    /// <summary>
    /// Represents Nintendo's UNROM/UOROM and similar mappers.
    /// </summary>
    public class UxRomMapper : Mapper
    {
        int _bank0Offset;
        int _bank1Offset;

        /// <summary>
        /// Construct a new UxROM mapper.
        /// </summary>
        /// <param name="console">the console that this mapper is a part of</param>
        public UxRomMapper(Console console)
        {
            _console = console;

            // PRG Bank 0 is switchable
            _bank0Offset = 0;

            // PRG Bank 1 is always fixed to the last bank
            _bank1Offset = (_console.Cartridge.PrgRomBanks - 1) * 0x4000;

            _vramMirroringType = _console.Cartridge.VerticalVramMirroring ? VramMirroring.Vertical : VramMirroring.Horizontal;
        }

        /// <summary>
        /// Read a byte from the specified address.
        /// </summary>
        /// <returns>the byte read from the specified address</returns>
        /// <param name="address">the address to read a byte from</param>
        public override byte Read(ushort address)
        {
            byte data;
            if (address < 0x2000) // CHR ROM or RAM
            {
                data = _console.Cartridge.ReadChr(address);
            }
            else if (address >= 0x6000 && address < 0x8000)
            {
                // Open Bus
                data = 0x00;
            }
            else if (address <= 0xC000) // PRG ROM bank 0
            {
                data = _console.Cartridge.ReadPrgRom(_bank0Offset + (address - 0x8000));
            }
            else if (address <= 0xFFFF) // PRG ROM bank 1
            {
                data = _console.Cartridge.ReadPrgRom(_bank1Offset + (address - 0xC000));
            }
            else
            {
                throw new Exception("Invalid mapper read at address: " + address.ToString("X4"));
            }
            return data;
        }

        /// <summary>
        /// Writes a byte to the specified address.
        /// </summary>
        /// <param name="address">the address to write a byte to</param>
        /// <param name="data">the byte to write to the address</param>
        public override void Write(ushort address, byte data)
        {
            if (address < 0x2000) // CHR ROM or RAM
            {
                _console.Cartridge.WriteChr(address, data);
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
