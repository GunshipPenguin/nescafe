using System.IO;
using System;

public class Cartridge
{
  const int HeaderMagic = 0x1a53454e;

  // Flags
  const uint TrainerFlag = 1<<3;
  const uint VerticalVramMirrorFlag = 1<<0;

  byte[] _prgRom;
  byte[] _chrRom;

  int _prgRomBanks;
  public int PrgRomBanks
  {
    get
    {
      return _prgRomBanks;
    }
  }
  int _chrRomBanks;
  public int ChrRomBanks
  {
    get
    {
      return _chrRomBanks;
    }
  }

  bool _verticalVramMirroring;
  public bool VerticalVramMirroring
  {
    get
    {
      return _verticalVramMirroring;
    }
  }

  int _prgRamSize;

  int _flags6;
  int _flags7;
  int _flags9;

  public Cartridge (string path)
  {
    FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    BinaryReader reader = new BinaryReader(stream);
    ParseHeader(reader);
    LoadPrgRom(reader);
    LoadChrRom(reader);
  }

  public byte ReadPrgRom(ushort address)
  {
    return _prgRom[address];
  }

  public byte ReadChrRom(ushort address)
  {
    return _chrRom[address];
  }

  void LoadPrgRom(BinaryReader reader)
  {
    // Add 512 byte trainer offset (if present as specified in _flags6)
    int _prgRomOffset = ((_flags6 & TrainerFlag) == 0) ? 16 : 16 + 512;

    reader.BaseStream.Seek(_prgRomOffset, SeekOrigin.Begin);

    _prgRom = new byte[_prgRomBanks * 16384];
    reader.Read(_prgRom, 0, _prgRomBanks * 16384);
  }

  void LoadChrRom(BinaryReader reader)
  {
    _chrRom = new byte[_chrRomBanks * 8192];    
    reader.Read(_chrRom, 0, _chrRomBanks * 8192);
  }

  void ParseHeader(BinaryReader reader)
  {
    // Verify magic number
    uint magicNum = reader.ReadUInt32();
    if (magicNum != HeaderMagic) throw new Exception("Magic number in header invalid");

    // Size of PRG ROM
    _prgRomBanks = reader.ReadByte();

    // Size of CHR ROM
    _chrRomBanks = reader.ReadByte();

    // Flags 6
    _flags6 = reader.ReadByte();
    _verticalVramMirroring = (_flags6 & VerticalVramMirrorFlag) != 0;

    // Flags 7
    _flags7 = reader.ReadByte();

    // Size of PRG RAM
    _prgRamSize = reader.ReadByte();

    // Flags 9
    _flags9 = reader.ReadByte();
  }
}
