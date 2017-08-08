using System.IO;
using System;

public class Cartridge {
  const int HEADER_MAGIC = 0x1a53454e;

  // Flags
  const uint cartridgeContainsTrainerFlag = 1<<3;
  const uint verticalVramMirroringFlag = 1<<0;

  byte[] prgRom;
  byte[] chrRom;

  public int prgRomBanks;
  public int chrRomBanks;

  public bool verticalVramMirroring;

  int prgRamSize;

  int flags6;
  int flags7;
  int flags9;

  public Cartridge (string path) {
    FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    BinaryReader reader = new BinaryReader(stream);
    parseHeader(reader);
    loadPrgRom(reader);
    loadChrRom(reader);
  }

  public byte readPrgRom(ushort address) {
    return prgRom[address];
  }

  public byte readChrRom(ushort address) {
    return chrRom[address];
  }

  private void loadPrgRom(BinaryReader reader) {
    // Add 512 byte trainer offset (if present as specified in flags6)
    int prgRomOffset = ((flags6 & cartridgeContainsTrainerFlag) == 0) ? 16 : 16 + 512;

    reader.BaseStream.Seek(prgRomOffset, SeekOrigin.Begin);

    prgRom = new byte[prgRomBanks * 16384];
    reader.Read(prgRom, 0, prgRomBanks * 16384);
  }

  private void loadChrRom(BinaryReader reader) {
    chrRom = new byte[chrRomBanks * 8192];    
    reader.Read(chrRom, 0, chrRomBanks * 8192);
  }

  private void parseHeader(BinaryReader reader) {
    // Verify magic number
    uint magicNum = reader.ReadUInt32();
    if (magicNum != HEADER_MAGIC) {
      throw new Exception("Magic number in header invalid");
    }

    // Size of PRG ROM
    prgRomBanks = reader.ReadByte();

    // Size of CHR ROM
    chrRomBanks = reader.ReadByte();

    // Flags 6
    flags6 = reader.ReadByte();
    verticalVramMirroring = (flags6 & verticalVramMirroringFlag) != 0;

    // Flags 7
    flags7 = reader.ReadByte();

    // Size of PRG RAM
    prgRamSize = reader.ReadByte();

    // Flags 9
    flags9 = reader.ReadByte();
  }
}
