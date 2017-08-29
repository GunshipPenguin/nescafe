using System;
using System.IO;
using Nescafe.Mappers;

namespace Nescafe
{
    /// <summary>
    /// Represents a NES Cartridge.
    /// </summary>
    public class Cartridge
    {
        const int HeaderMagic = 0x1A53454E;

        byte[] _prgRom;
        byte[] _chr;

        byte[] _prgRam;

        /// <summary>
        /// Gets or sets the console that this cartridge is loaded into.
        /// </summary>
        /// <value>The console that this this cartridge is loaded into</value>
        public Console Console { get; set; }

        /// <summary>
        /// Gets the number of 16KB PRG ROM banks present in this cartridge.
        /// </summary>
        /// <value>The number of 16 KB PRG ROM banks present in this cartridge.</value>
        public int PrgRomBanks { get; private set; }

        /// <summary>
        /// Gets the number of 8KB CHR banks present in this cartridge.
        /// </summary>
        /// <value>The number of 8KB CHR banks present in this cartridge.</value>
        public int ChrBanks { get; private set; }

        /// <summary>
        /// Is <c>true</c> if the Vertical VRAM mirroring iNES flag is set on
        /// on this cartridge.
        /// </summary>
        /// <value><c>true</c> if the vertical VRAM mirroring flag is set; otherwise, <c>false</c>.</value>
        public bool VerticalVramMirroring { get; private set; }

        /// <summary>
        /// Is <c>true</c> if this cartridge contains battery backed persistent memory.
        /// </summary>
        /// <value><c>true</c> if battery backed memory present; otherwise, <c>false</c>.</value>
        public bool BatteryBackedMemory { get; private set; }

        /// <summary>
        /// Is <c>true</c> if this cartridge contains a 512 byte trainer.
        /// </summary>
        /// <value><c>true</c> if this cartridge contains a trainer; otherwise, <c>false</c>.</value>
        public bool ContainsTrainer { get; private set; }

        /// <summary>
        /// Is true if this cartridge uses CHR RAM as opposed to CHR ROM.
        /// </summary>
        /// <value><c>true</c> if this cartridge uses CHR RAM; otherwise, <c>false</c>.</value>
        public bool UsesChrRam { get; private set; }

        /// <summary>
        /// Gets the iNES mapper number set on this cartridge.
        /// </summary>
        /// <value>This cartridge's iNES mapper number.</value>
        public int MapperNumber { get; private set; }

        /// <summary>
        /// Is true if this cartridge is for some reason invalid, fase otherwise.
        /// </summary>
        /// <value><c>true</c> if cartridge is invalid; otherwise, <c>false</c>.</value>
        public bool Invalid { get; private set; }

        int _flags6;
        int _flags7;

        /// <summary>
        /// Constructs a new Cartridge from the iNES cartridge file at the
        /// specified path.
        /// </summary>
        /// <param name="path">The path to the .nes file to load</param>
        public Cartridge(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            Invalid = false;
            ParseHeader(reader);
            LoadPrgRom(reader);
            LoadChr(reader);

            _prgRam = new byte[8192];
        }

        /// <summary>
        /// Reads and returns a byte of this cartridge's PRG ROM at the specified index.
        /// </summary>
        /// <returns>This cartridge's PRG ROM at the specified index</returns>
        /// <param name="index">The index to read the PRG ROM at</param>
        public byte ReadPrgRom(int index)
        {
            return _prgRom[index];
        }

        /// <summary>
        /// Reads and returns a byte of this cartridge's PRG RAM at the specified index.
        /// </summary>
        /// <returns>A byte of this cartridge's PRG ROM at the specified index</returns>
        /// <param name="index">The index to read the PRG RAM at</param>
        public byte ReadPrgRam(int index)
        {
            return _prgRam[index];
        }

        /// <summary>
        /// Write a byte to this cartridge's PRG RAM at the specified index.
        /// </summary>
        /// <param name="index">The PRG RAM index to write to</param>
        /// <param name="data">The byte to write to PRG RAm</param>
        public void WritePrgRam(int index, byte data)
        {
            _prgRam[index] = data;
        }

        /// <summary>
        /// Reads and returns a byte of this cartridge's CHR data at the specified index.
        /// </summary>
        /// <returns>A byte of this cartridge's CHR at the specified index</returns>
        /// <param name="index">The index to read the CHR data at</param>
        public byte ReadChr(int index)
        {
            return _chr[index];
        }

        /// <summary>
        /// Write a byte to this cartridge's CHR at the specified index.
        /// </summary>
        /// <param name="index">The index to write the data to in CHR</param>
        /// <param name="data">The byte to write to CHR</param>
        public void WriteChr(int index, byte data)
        {
            if (!UsesChrRam) throw new Exception("Attempted write to CHR ROM at index " + index.ToString("X4"));
            _chr[index] = data;
        }

        void LoadPrgRom(BinaryReader reader)
        {
            // Add 512 byte trainer offset (if present as specified in _flags6)
            int _prgRomOffset = ContainsTrainer ? 16 + 512 : 16;

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
            // 76543210
            // ||||||||
            // |||||||+-Mirroring: 0: horizontal(vertical arrangement)(CIRAM A10 = PPU A11)
            // |||||||             1: vertical(horizontal arrangement)(CIRAM A10 = PPU A10)
            // ||||||+--1: Cartridge contains battery - backed PRG RAM($6000 - 7FFF) or other persistent memory
            // |||||+---1: 512 - byte trainer at $7000 -$71FF(stored before PRG data)
            // ||||+----1: Ignore mirroring control or above mirroring bit; instead provide four - screen VRAM
            // ++++---- - Lower nybble of mapper number
            _flags6 = reader.ReadByte();
            VerticalVramMirroring = (_flags6 & 0x01) != 0;
            System.Console.WriteLine("VRAM mirroring type: " + (VerticalVramMirroring ? "vertical" : "horizontal"));

            BatteryBackedMemory = (_flags6 & 0x02) != 0;
            if (BatteryBackedMemory) System.Console.WriteLine("Cartridge contains battery backed persistent memory");

            ContainsTrainer = (_flags6 & 0x04) != 0;
            if (ContainsTrainer) System.Console.WriteLine("Cartridge contains a 512 byte trainer");

            // Flags 7
            // 76543210
            // ||||||||
            // |||||||+-VS Unisystem
            // ||||||+--PlayChoice - 10(8KB of Hint Screen data stored after CHR data)
            // ||||++---If equal to 2, flags 8 - 15 are in NES 2.0 format
            // ++++---- - Upper nybble of mapper number
            _flags7 = reader.ReadByte();

            // Mapper Number
            MapperNumber = _flags7 & 0xF0 | (_flags6 >> 4 & 0xF);
        }
    }   
}
