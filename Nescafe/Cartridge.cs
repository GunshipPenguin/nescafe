using System;
using System.IO;
using Nescafe.Mappers;

namespace Nescafe
{
    public class Cartridge
    {
        const int HeaderMagic = 0x1A53454E;

        // Flags
        const uint TrainerFlag = 1 << 3;
        const uint VerticalVramMirrorFlag = 1 << 0;

        byte[] _prgRom;
        byte[] _chr;

        byte[] _prgRam;

        public int PrgRomBanks { get; private set; }
        public int ChrBanks { get; private set; }

        public bool VerticalVramMirroring { get; private set; }

        public bool BatteryBackedMemory { get; private set; }

        public bool ContainsTrainer { get; private set; }

        public bool UsesChrRam { get; private set; }

        int _mapperNumber;
        public Mapper Mapper { get; private set; }

        public bool Invalid { get; private set; }

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

            _prgRam = new byte[8192];
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
                case 1:
                    System.Console.WriteLine(" (MMC1) Supported!");
                    Mapper = new Mmc1Mapper(this);
                    break;
                default:
                    System.Console.WriteLine(" mapper is not supported");
                    Invalid = true;
                    break;
            }
        }

        public byte ReadPrgRom(int index)
        {
            return _prgRom[index];
        }

        public byte ReadPrgRam(int index)
        {
            return _prgRam[index];
        }

        public void WritePrgRam(int index, byte data)
        {
            _prgRam[index] = data;
        }

        public byte ReadChr(int index)
        {
            return _chr[index];
        }

        public void WriteChr(int index, byte data)
        {
            if (!UsesChrRam) throw new Exception("Attempted write to CHR ROM at index " + index.ToString("X4"));
            else _chr[index] = data;
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
                _chr = new byte[ChrBanks * 8192];
                reader.Read(_chr, 0, ChrBanks * 8192);
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
            ChrBanks = reader.ReadByte();
            if (ChrBanks == 0) {
                System.Console.WriteLine("Cartridge uses CHR RAM");
                ChrBanks = 2;
                UsesChrRam = true;
            }
            else 
            {
                System.Console.WriteLine((8 * ChrBanks).ToString() + "Kb of CHR ROM");
                UsesChrRam = false;
            }

            // Flags 6
            _flags6 = reader.ReadByte();
            VerticalVramMirroring = (_flags6 & VerticalVramMirrorFlag) != 0;
            System.Console.WriteLine("VRAM mirroring type: " + (VerticalVramMirroring ? "vertical" : "horizontal"));

            ContainsTrainer = (_flags6 & 0x04) != 0;
            if (ContainsTrainer) System.Console.WriteLine("Cartridge contains a 512 byte trainer");

            BatteryBackedMemory = (_flags6 & 0x02) != 0;
            if (BatteryBackedMemory) System.Console.WriteLine("Cartridge contains battery backed persistent memory");

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
