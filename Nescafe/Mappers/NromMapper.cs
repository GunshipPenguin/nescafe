using System;

namespace Nescafe.Mappers
{
    /// <summary>
    /// Represents Nintendo's NROM Mapper.
    /// </summary>
    class NromMapper : Mapper
    {
        /// <summary>
        /// Construct a new NROM mapper.
        /// </summary>
        /// <param name="console">Console.</param>
        public NromMapper(Console console)
        {
            _console = console;
            _vramMirroringType = _console.Cartridge.VerticalVramMirroring ? VramMirroring.Vertical : VramMirroring.Horizontal;
        }

        int AddressToPrgRomIndex(ushort address)
        {
            ushort mappedAddress = (ushort)(address - 0x8000); // PRG banks start at 0x8000
            return _console.Cartridge.PrgRomBanks == 1 ? (ushort)(mappedAddress % 16384) : mappedAddress; // Wrap if only 1 PRG bank
        }

        /// <summary>
        /// Read a byte from the specified address.
        /// </summary>
        /// <returns>the byte read from the specified address</returns>
        /// <param name="address">the address to read a byte from</param>
        public override byte Read(ushort address)
        {
            byte data;
            if (address < 0x2000) // CHR rom stored from $0000 to $1FFF
            {
                data = _console.Cartridge.ReadChr(address);
            }
            else if (address >= 0x8000) // PRG ROM stored at $8000 and above
            {
                data = _console.Cartridge.ReadPrgRom(AddressToPrgRomIndex(address));
            }
            else
            {
                throw new Exception("Invalid mapper read");
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
            if (address < 0x2000) // CHR RAM
            {
                _console.Cartridge.WriteChr(address, data);
            }
        }
    }    
}
