using System;

namespace Nescafe.Mappers
{
    public class Mmc1Mapper : Mapper
    {
        // Common shift register
        byte _shiftReg;

        // Internal registers
        byte _controlReg;
        byte _chr0Reg;
        byte _chr1Reg;
        byte _prgReg;

        // Control register
        byte _prgMode;
        byte _chrMode;

        // PRG register
        byte _prgRamEnable;

        // CHR offsets
        int _chrBank0Offset;
        int _chrBank1Offset;

        // PRG offsets
        int _prgBank0Offset;
        int _prgBank1Offset;

        // Current number of writes to internal shift register
        int _shiftCount;

        public Mmc1Mapper(Cartridge cartridge)
        {
            _cartridge = cartridge;
            _shiftReg = 0x0C;
            _controlReg = 0x00;
            _chr0Reg = 0x00;
            _chr1Reg = 0x00;
            _prgReg = 0x00;

            _shiftCount = 0;

            _prgBank1Offset = (cartridge.PrgRomBanks - 1) * 0x4000;

            _vramMirroringType = VramMirroring.Horizontal;
        }

        public override byte Read(ushort address)
        {
            byte data;
            if (address <= 0x1FFF) // CHR Banks 0 and 1 $0000-0x0FFF and $1000-$1FFF
            {
                int offset = (address / 0x1000) == 0 ? _chrBank0Offset : _chrBank1Offset;
                offset += address % 0x1000;
                data = _cartridge.ReadPrgRom(offset);
            }
            else if (address >= 0x6000 && address <= 0x7FFF) // 8 KB PRG RAM bank (CPU) $6000-$7FFF
            {
                // TODO: Implement This
                //    throw new NotImplementedException("PRG RAM not implemented");
                data = _cartridge.ReadPrgRam(address - 0x6000);
            }
            else if (address >= 0x8000 && address <= 0xFFFF) // 2 PRG ROM banks
            {
  
                address -= 0x8000;
                int offset = (address / 0x4000) == 0 ? _prgBank0Offset : _prgBank1Offset;
                offset += address % 0x4000;
                data = _cartridge.ReadPrgRom(offset);
            }
            else
            {
                throw new Exception("Invalid Mapper read at address " + address.ToString("X4"));
            }
            return data;
        }

        public override void Write(ushort address, byte data)
        {
            if (address < 0x2000)
            {
                if (!_cartridge.UsesChrRam) throw new Exception("Attempt to write to CHR ROM at " + address.ToString("X4"));

                int offset = (address / 0x1000) == 0 ? _chrBank0Offset : _chrBank1Offset;
                offset += address % 0x1000;
                _cartridge.WriteChr(offset, data);
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                _cartridge.WritePrgRam(address - 0x6000, data);
            }
            else if (address >= 0x8000) // Connected to common shift register
            {
                LoadRegister(address, data);
            }
            else
            {
                throw new Exception("Invalid mapper write at address " + address.ToString("X4"));
            }
        }

        void LoadRegister(ushort address, byte data)
        {
            if ((data & 0x80) != 0)
            {
                // If bit 7 set, clear internal shift register
                WriteRegister(address, (byte)(_shiftReg | 0x0C));
                _shiftReg = 0;
                _shiftCount = 0;
            }
            else
            {
                _shiftReg |= (byte)((data & 1) << _shiftCount);
                _shiftCount++;

                if (_shiftCount == 5)
                {
                    _shiftCount = 0;
                    WriteRegister(address, _shiftReg);
                    _shiftReg = 0;
                }
            }
        }

        void WriteRegister(ushort address, byte data)
        {
            if (address >= 0x8000 && address <= 0x9FFF)
            {
                WriteControlReg(data);
            }
            else if (address <= 0xBFFF)
            {
                WriteChr0Reg(data);
            }
            else if (address <= 0xDFFF)
            {
                WriteChr1Reg(data);
            }
            else if (address <= 0xFFFF)
            {
                WritePrgReg(data);
            }
            else
            {
                throw new Exception("Invalid MMC1 Register write at address " + address.ToString("X4"));
            }
        }

        void WriteControlReg(byte data)
        {
            _controlReg = data;
            _prgMode = (byte)((data >> 2) & 0x03);
            _chrMode = (byte)((data >> 4) & 0x01);
            switch(_controlReg & 0x03)
            {
                case 0:
                    _vramMirroringType = VramMirroring.SingleLower;
                    break;
                case 1:
                    _vramMirroringType = VramMirroring.SingleUpper;
                    break;
                case 2:
                    _vramMirroringType = VramMirroring.Vertical;
                    break;
                case 3:
                    _vramMirroringType = VramMirroring.Horizontal;
                    break;
            }
            UpdateBankOffsets();
        }

        void WriteChr0Reg(byte data)
        {
            _chr0Reg = data;
            UpdateBankOffsets();
        }

        void WriteChr1Reg(byte data)
        {
            _chr1Reg = data;
            UpdateBankOffsets();
        }

        void WritePrgReg(byte data)
        {
            _prgReg = data;
            _prgRamEnable = (byte) ((data >> 4) & 0x01);
            UpdateBankOffsets();
        }

        void UpdateBankOffsets()
        {
            switch (_chrMode)
            {
                case 0: // Switch 8 KB at a time
                    // Lowest bit of bank number ignored in 8 Kb mode
                    _chrBank0Offset = ((_chr0Reg & 0x1E) >> 1) * 0x1000;
                    _chrBank1Offset = _chrBank0Offset + 0x1000;
                    break;
                case 1: // Switch 4 KB at a time
                    _chrBank0Offset = _chr0Reg * 0x1000;
                    _chrBank1Offset = _chr1Reg * 0x1000;
                    break;
            }

            switch (_prgMode)
            {
                case 0:
                case 1:
                    // Lowest bit of bank number is ignored with mode 0 or 1
                    _prgBank0Offset = ((_prgReg & 0xE) >> 1) * 0x4000;
                    _prgBank1Offset = _prgBank0Offset + 0x4000;
                    break;
                case 2:
                    // Fix first bank at $8000
                    _prgBank0Offset = 0;
                    // Switch second bank at $C000
                    _prgBank1Offset = (_prgReg & 0xF) * 0x4000;
                    break;
                case 3:
                    // Switch 16 KB bank at $8000
                    _prgBank0Offset = (_prgReg & 0xF) * 0x4000;
                    // Fix last bank at $C000
                    _prgBank1Offset = (_cartridge.PrgRomBanks - 1) * 0x4000;
                    break;
            }
        }
    }
}
