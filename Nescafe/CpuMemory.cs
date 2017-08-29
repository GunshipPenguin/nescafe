using System;

namespace Nescafe
{
    public class CpuMemory : Memory
    {
        // First 2KB of internal ram
        readonly byte[] _internalRam = new byte[2048];
        readonly Console _console;

        public CpuMemory(Console console)
        {
            _console = console;
        }

        public void Reset()
        {
            Array.Clear(_internalRam, 0, _internalRam.Length);
        }

        // Return the index in internalRam of the address (handle mirroring)
        ushort HandleInternalRamMirror(ushort address)
        {
            return (ushort)(address % 0x800);
        }

        // Handles mirroring of PPU register addresses
        ushort GetPpuRegisterFromAddress(ushort address)
        {
            // Special case for OAMDMA ($4014) which is not alongside the other registers
            if (address == 0x4014) return address;
            else return (ushort)(0x2000 + ((address - 0x2000) % 8));
        }

        void WritePpuRegister(ushort address, byte data)
        {
            _console.Ppu.WriteToRegister(GetPpuRegisterFromAddress(address), data);
        }

        byte ReadPpuRegister(ushort address)
        {
            return _console.Ppu.ReadFromRegister(GetPpuRegisterFromAddress(address));
        }

        byte ReadApuIoRegister(ushort address)
        {
            byte data;
            switch(address)
            {
                case 0x4016: // Controller 1
                    data = _console.Controller.ReadControllerOutput();
                    break;
                default: // Unimplemented register
                    data = 0;
                    break;
            }
            return data;
        }

        void WriteApuIoRegister(ushort address, byte data)
        {
            switch(address)
            {
                case 0x4016: // Controller 1
                    _console.Controller.WriteControllerInput(data);
                    break;
                default: // Unimplemented register
                    data = 0;
                    break;            
            }
        }

        public override byte Read(ushort address)
        {
            byte data;
            if (address < 0x2000) // Internal CPU RAM 
            {
                ushort addressIndex = HandleInternalRamMirror(address);
                data = _internalRam[addressIndex];
            }
            else if (address <= 0x3FFF) // PPU Registers
            {
                data = ReadPpuRegister(address);
            }
            else if (address <= 0x4017) // Apu and IO Registers
            {
                data = ReadApuIoRegister(address);
            }
            else if (address <= 0x401F) // Disabled on a retail NES
            {
                data = 0;
            }
            else if (address >= 0x4020) // Handled by mapper (PRG rom, CHR rom/ram etc.)
            {
                data = _console.Mapper.Read(address);
            }
            else // Invalid Read
            {
                throw new Exception("Invalid CPU read at address " + address.ToString("X4"));
            }

            return data;
        }

        public override void Write(ushort address, byte data)
        {
            if (address < 0x2000) // Internal CPU RAM
            {
                ushort addressIndex = HandleInternalRamMirror(address);
                _internalRam[addressIndex] = data;
            }
            else if (address <= 0x3FFF || address == 0x4014) // PPU Registers
            {
                WritePpuRegister(address, data);
            }
            else if (address <= 0x4017) // APU / IO 
            {
                WriteApuIoRegister(address, data);
            }
            else if (address <= 0x401F) // Disabled on a retail NES
            {

            }
            else if (address >= 0x4020)
            {
                _console.Mapper.Write(address, data);
            }
            else // Invalid Write
            {
                throw new Exception("Invalid CPU write to address " + address.ToString("X4"));
            }
        }
    }
}
