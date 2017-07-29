using System.IO;
using System;

public class Cartridge {
  const int HEADER_MAGIC = 0x1a53454e;

  // Flags
  const uint cartridgeContainsTrainerFlag = 1<<3;

  byte[] prgRom;
  byte[] chrRom;

  int prgRomSize;
  int chrRomSize;

  int prgRamSize;

  int flags6;
  int flags7;
  int flags9;

  public Cartridge (string path) {
    FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    BinaryReader reader = new BinaryReader(stream);
    parseHeader(reader);
    loadPrgRom(reader);
  }

  public byte readPrgRom(ushort address) {
    return prgRom[address];
  }

  private void loadPrgRom(BinaryReader reader) {
    // Add 512 byte trainer offset (if present as specified in flags6)
    int prgRomOffset = ((flags6 & cartridgeContainsTrainerFlag) == 0) ? 16 + 512 : 16;

    reader.BaseStream.Seek(prgRomOffset, SeekOrigin.Begin);

    prgRom = new byte[prgRomSize];
    reader.Read(prgRom, 0, prgRomSize);
  }

  private void parseHeader(BinaryReader reader) {
    // Verify magic number
    uint magicNum = reader.ReadUInt32();
    if (magicNum != HEADER_MAGIC) {
      throw new Exception("Magic number in header invalid");
    }

    // Size of PRG ROM
    prgRomSize = reader.ReadByte() * 16384; // x * 16 KiB

    // Size of CHR ROM
    chrRomSize = reader.ReadByte() * 8192; // x * 8 KiB

    // Flags 6
    flags6 = reader.ReadByte();

    // Flags 7
    flags7 = reader.ReadByte();

    // Size of PRG RAM
    prgRamSize = reader.ReadByte();

    // Flags 9
    flags9 = reader.ReadByte();
  }
}
