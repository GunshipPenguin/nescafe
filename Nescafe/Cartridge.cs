using System;
using System.IO;
using Nescafe.Mappers;

namespace Nescafe
{
    public class Cartridge
    {
        const int HeaderMagic = 0x1a53454e;

        // Flags
        const uint TrainerFlag = 1 << 3;
        const uint VerticalVramMirrorFlag = 1 << 0;

        byte[] _prgRom;
        byte[] _chr;

        public int PrgRomBanks { get; set; }
        public int ChrRomBanks { get; set; }

        public bool VerticalVramMirroring { get; set; }
        public bool UsesChrRam { get; set; }

        int _mapperNumber;
        public Mapper Mapper { get; set; }

        public bool Invalid { get; set; }

        int _prgRamSize;

        int _flags6;
        int _flags7;
        int _flags9;

        public Cartridge(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            Invalid = false;
            ParseHeader(reader);
            LoadPrgRom(reader);
            LoadChr(reader);
            SetMapper();
        }

        void SetMapper()
        {
            System.Console.Write("iNES Mapper Number: " + _mapperNumber.ToString());
            switch (_mapperNumber)
            {
                case 0:
                    System.Console.WriteLine(" (NROM) Supported!");
                    Mapper = new NromMapper(this);
                    break;
                default:
                    System.Console.WriteLine(" Mapper is not supported");
                    Invalid = true;
                    break;
            }
        }

        public byte ReadPrgRom(ushort address)
        {
            return _prgRom[address];
        }

        public byte ReadChr(ushort address)
        {
            return _chr[address];
        }

        public void WriteChr(ushort address, byte data)
        {
            if (!UsesChrRam) throw new Exception("Attempted write to CHR ROM at address " + address.ToString("X4"));
            else _chr[address] = data;
        }

        void LoadPrgRom(BinaryReader reader)
        {
            // Add 512 byte trainer offset (if present as specified in _flags6)
            int _prgRomOffset = ((_flags6 & TrainerFlag) == 0) ? 16 : 16 + 512;

            reader.BaseStream.Seek(_prgRomOffset, SeekOrigin.Begin);

            _prgRom = new byte[PrgRomBanks * 16384];
            reader.Read(_prgRom, 0, PrgRomBanks * 16384);
        }

        void LoadChr(BinaryReader reader)
        {
            if (UsesChrRam)
            {
                _chr = new byte[8192];
            }
            else
            {
                _chr = new byte[ChrRomBanks * 8192];
                reader.Read(_chr, 0, ChrRomBanks * 8192);
            }
        }

        void ParseHeader(BinaryReader reader)
        {
            // Verify magic number
            uint magicNum = reader.ReadUInt32();
            if (magicNum != HeaderMagic)
            {
                System.Console.WriteLine("Magic header value (" + magicNum.ToString("X4") + ") is incorrect");
                Invalid = true;
                return;
            }

            // Size of PRG ROM
            PrgRomBanks = reader.ReadByte();
            System.Console.WriteLine((16 * PrgRomBanks).ToString() + "Kb of PRG ROM");

            // Size of CHR ROM (Or set CHR RAM if using it)
            ChrRomBanks = reader.ReadByte();
            if (ChrRomBanks == 0) {
                System.Console.WriteLine("Cartridge uses CHR RAM");
                UsesChrRam = true;
            }
            else 
            {
                System.Console.WriteLine((8 * ChrRomBanks).ToString() + "Kb of CHR ROM");
                UsesChrRam = false;
            }

            // Flags 6
            _flags6 = reader.ReadByte();
            VerticalVramMirroring = (_flags6 & VerticalVramMirrorFlag) != 0;
            System.Console.WriteLine("VRAM mirroring type: " + (VerticalVramMirroring ? "vertical" : "horizontal"));

            // Flags 7
            _flags7 = reader.ReadByte();

            // Mapper Number
            _mapperNumber = (int)(_flags7 & 0xF0 | (_flags6 >> 4 & 0xF));

            // Size of PRG RAM
            _prgRamSize = reader.ReadByte();

            // Flags 9
            _flags9 = reader.ReadByte();
        }
    }   
}
