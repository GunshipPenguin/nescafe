using System;
namespace Nescafe.Mappers
{
    public class Mmc3Mapper : Mapper
    {
        // Bank select register
        byte _bank;
        byte _prgRomMode;
        byte _chrRomMode;

        // PRG RAM protect register
        byte _prgRamEnable;
        byte _prgRamProtect;

        // Bank offsets
        int[] _chrOffsets;
        int[] _prgOffsets;

        // Bank registers
        byte[] _bankRegisters;

        // IRQ enable/disable registers
        bool _irqEnabled;

        // IRQ counter and reload value
        int _irqCounter;
        byte _irqCounterReload;

        public Mmc3Mapper(Console console)
        {
            _console = console;

            _bankRegisters = new byte[8];

            // 6 switchable CHR banks
            _chrOffsets = new int[8];

            // 2 switchable PRG ROM banks, 2 fixed to last and second to last banks
            _prgOffsets = new int[4];

            _prgOffsets[0] = 0;
            _prgOffsets[1] = 0x2000;
            _prgOffsets[2] = ((_console.Cartridge.PrgRomBanks * 2) - 2) * 0x2000;
            _prgOffsets[3] = _prgOffsets[2] + 0x2000;
        }

        public override byte Read(ushort address)
        {
            byte data;

            if (address < 0x2000)
            {
                int bank = address / 0x400;
                int offset = _chrOffsets[bank] + (address % 0x400);
                data = _console.Cartridge.ReadChr(offset);
            }
            else if (address >= 0x6000 && address < 0x8000) // 8 KB PRG RAM
            {
                data = (byte)(_prgRamEnable == 1 ? _console.Cartridge.ReadPrgRam(address - 0x6000) : 0);
            }
            else if (address <= 0xFFFF) // 8 KB PRG ROM banks
            {
                int bank = ((address - 0x8000) / 0x2000);
                int offset = _prgOffsets[bank] + (address % 0x2000);
                data = _console.Cartridge.ReadPrgRom(offset);
            }
            else
            {
                throw new Exception("Invalid mapper read at address " + address.ToString("X4"));
            }

            return data;
        }

        public override void Write(ushort address, byte data)
        {
            bool even = address % 2 == 0;
            if (address < 0x2000) // CHR
            {
                int bank = address / 0x400;
                int offset = _chrOffsets[bank] + (address % 0x400);
                _console.Cartridge.WriteChr(offset, data);
            }
            else if (address >= 0x6000 && address < 0x8000) // PRG RAM
            {
                if (_prgRamProtect == 0) _console.Cartridge.WritePrgRam(address - 0x6000, data);
            }
            else if (address < 0xA000) // $8000-$9FFFF
            {
                if (even) WriteBankSelectReg(data);
                else WriteBankDataReg(data);
            }
            else if (address < 0xC000) // $A000-$BFFF
            {
                if (even) WriteMirroringReg(data);
                else WritePrgRamProtectReg(data);
            }
            else if (address < 0xE000) // $C000-$DFFF
            {
                if (even) WriteIrqLatchReg(data);
                else WriteIrqReloadReg(data);
            }
            else if (address <= 0xFFFF) // $E000-$FFFF
            {
                if (even) WriteIrqDisableReg(data);
                else WriteIrqEnableReg(data);
            }
        }

        void WriteBankSelectReg(byte data)
        {
            _bank = (byte) (data & 0x07);
            _prgRomMode = (byte)((data >> 6) & 0x01);
            _chrRomMode = (byte)((data >> 7) & 0x01);
            UpdateBankOffsets();
        }

        void WriteBankDataReg(byte data)
        {
            _bankRegisters[_bank] = data;
            UpdateBankOffsets();
        }

        public override void Step()
        {
            int scanline = _console.Ppu.Scanline;
            int cycle = _console.Ppu.Cycle;
            bool renderingEnabled = _console.Ppu.RenderingEnabled;

            if (renderingEnabled && cycle == 260 && scanline >= 0 && scanline < 240) ClockA12();
        }

        void ClockA12()
        {
            if (_irqCounter == 0)
            {
                _irqCounter = _irqCounterReload;
            }
            else
            {
                _irqCounter--;
                if (_irqCounter == 0 && _irqEnabled) _console.Cpu.TriggerIrq();
            }
        }

        void WriteIrqEnableReg(byte data)
        {
            _irqEnabled = true;
        }

        void WriteIrqDisableReg(byte data)
        {
            _irqEnabled = false;
        }

        void WriteIrqReloadReg(byte data)
        {
            _irqCounter = 0;
        }

        void WriteIrqLatchReg(byte data)
        {
            _irqCounterReload = data;
        }

        void WritePrgRamProtectReg(byte data)
        {
            _prgRamEnable = (byte)((data >> 7) & 0x01);
            _prgRamProtect = (byte)((data >> 6) & 0x01);
        }

        void WriteMirroringReg(byte data)
        {
            switch(data & 0x01)
            {
                case 0:
                    _vramMirroringType = VramMirroring.Vertical;
                    break;
                case 1:
                    _vramMirroringType = VramMirroring.Horizontal;
                    break;
            }
        }

        void UpdateBankOffsets()
        {
            switch(_chrRomMode)
            {
                case 0:
                    _chrOffsets[0] = (_bankRegisters[0] & 0xFE) * 0x400;
                    _chrOffsets[1] = (_bankRegisters[0] | 0x01) * 0x400;
                    _chrOffsets[2] = (_bankRegisters[1] & 0xFE) * 0x400;
                    _chrOffsets[3] = (_bankRegisters[1] | 0x01) * 0x400;
                    _chrOffsets[4] = _bankRegisters[2] * 0x400;
                    _chrOffsets[5] = _bankRegisters[3] * 0x400;
                    _chrOffsets[6] = _bankRegisters[4] * 0x400;
                    _chrOffsets[7] = _bankRegisters[5] * 0x400;
                    break;
                case 1:
                    _chrOffsets[0] = _bankRegisters[2] * 0x400;
                    _chrOffsets[1] = _bankRegisters[3] * 0x400;
                    _chrOffsets[2] = _bankRegisters[4] * 0x400;
                    _chrOffsets[3] = _bankRegisters[5] * 0x400;
                    _chrOffsets[4] = (_bankRegisters[0] & 0xFE) * 0x400;
                    _chrOffsets[5] = (_bankRegisters[0] | 0x01) * 0x400;
                    _chrOffsets[6] = (_bankRegisters[1] & 0xFE) * 0x400;
                    _chrOffsets[7] = (_bankRegisters[1] | 0x01) * 0x400;
                    break;
            }

            int secondLastBankOffset = ((_console.Cartridge.PrgRomBanks * 2) - 2) * 0x2000;
            int lastBankOffset = secondLastBankOffset + 0x2000;
            switch(_prgRomMode)
            {
                case 0:
                    _prgOffsets[0] = _bankRegisters[6] * 0x2000;
                    _prgOffsets[1] = _bankRegisters[7] * 0x2000;
                    _prgOffsets[2] = secondLastBankOffset;
                    _prgOffsets[3] = lastBankOffset;
                    break;
                case 1:
                    _prgOffsets[0] = secondLastBankOffset;
                    _prgOffsets[1] = _bankRegisters[7] * 0x2000;
                    _prgOffsets[2] = _bankRegisters[6] * 0x2000;
                    _prgOffsets[3] = lastBankOffset;
                    break;
            }
        }
    }
}
