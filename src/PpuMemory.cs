using System;

public class PpuMemory : Memory
{
  Console _console;
  byte[] _vRam;
  byte[] _paletteRam;

  public PpuMemory (Console _console)
  {
    this._console = _console;
    _vRam = new byte[2048];
    _paletteRam = new byte[32];
  }

  public ushort GetVRamIndex(ushort address)
  {
    address = (ushort) ((address - 0x2000) % 0x1000);
    ushort index;
    if (_console.Cartridge.VerticalVramMirroring)
    {
      index = address >= 0x2800 ? (ushort) (address - 0x800) : address;
    }
    else
    { // Horizontal Mirroring  
      index = address < 0x2800 ? (ushort) (address - 0x2000) : (ushort) (address - 0x2800);
      index %= 0x0400;
    }
    return index;
  }

  public ushort GetPaletteRamIndex(ushort address)
  {
    ushort index = (ushort) ((address - 0x3F00) % 32);

    // Mirror $3F10, $3F14, $3F18, $3fF1C to $3F00
    if (index >= 16 && ((index - 16) % 4 == 0)) return 0;
    else return index;
  }

  public override byte Read(ushort address)
  {
    byte data;
    if (address < 0x2000) // CHR ROM pattern tables
    { 
      data = _console.Cartridge.ReadChrRom(address);
    }
    else if (address <= 0x2FFF) // Internal _vRam
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