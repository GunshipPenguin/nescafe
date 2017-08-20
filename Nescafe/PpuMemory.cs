using System;

namespace Nescafe
{
    public class PpuMemory : Memory
    {
        Console _console;
        byte[] _vRam;
        byte[] _paletteRam;

        public PpuMemory(Console console)
        {
            _console = console;
            _vRam = new byte[2048];
            _paletteRam = new byte[32];
        }

        public void Reset()
        {
            Array.Clear(_vRam, 0, _vRam.Length);
            Array.Clear(_paletteRam, 0, _paletteRam.Length);
        }

        // Gets the index in the vram array of an address accounting for the type of mirroring
        public int GetVRamIndex(ushort address)
        {
            int index = (address - 0x2000) % 0x1000;
            if (_console.Cartridge.VerticalVramMirroring)
            {
                // If in one of the mirrored regions, subtract 0x800 to get index
                if (index > 0x800) index -= 0x800;
            }
            else
            {
                if (index > 0x800) index = ((index - 0x800) % 0x400) + 0x400; // In the 2 B regions
                else index %= 0x400; // In one of the 2 A regions
            }
            return index;
        }

        public ushort GetPaletteRamIndex(ushort address)
        {
            ushort index = (ushort)((address - 0x3F00) % 32);

            // Mirror $3F10, $3F14, $3F18, $3F1C to $3F00, $3F14, $3F08 $3F0C
            if (index >= 16 && ((index - 16) % 4 == 0)) return (ushort) (index - 16);
            else return index;
        }

        public override byte Read(ushort address)
        {
            byte data;
            if (address < 0x2000) // CHR ROM pattern tables
            {
                data = _console.Cartridge.ReadChrRom(address);
            }
            else if (address <= 0x3EFF) // Internal _vRam
            {
                data = _vRam[GetVRamIndex(address)];
            }
            else if (address >= 0x3F00 && address <= 0x3FFF) // Palette RAM
            {
                data = _paletteRam[GetPaletteRamIndex(address)];
            }
            else // Invalid Read
            {
                throw new Exception("Invalid PPU Memory Read at address: " + address.ToString("x4"));
            }
            return data;
        }

        public override void Write(ushort address, byte data)
        {
            if (address >= 0x2000 && address <= 0x3EFF) // Internal VRAM
            {
                _vRam[GetVRamIndex(address)] = data;
            }
            else if (address >= 0x3F00 && address <= 0x3FFF) // Palette RAM addresses
            {
                ushort addr = GetPaletteRamIndex(address);
                _paletteRam[addr] = data;
            }
            else // Invalid Write
            {
                throw new Exception("Invalid PPU Memory Write at address: " + address.ToString("x4"));
            }
        }
    }
}
