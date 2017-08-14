using System;

public class Ppu
{
  byte[] _bitmapData;
  public byte[] BitmapData
  {
    get
    {
      return _bitmapData;
    }
    set
    {
      _bitmapData = value;
    }
  }

  PpuMemory _memory;
  Console _console;

  // OAM
  byte[] _oam;
  byte[] _secondaryOam;
  ushort _oamAddr;
  int _numSprites;

  int _scanline;
  int _cycle;

  // Current nametable, attribute table and background pattern address
  byte _currNametableByte;
  byte _currAttributeTableByte;
  ushort _currBgPatternAddress;

  // Base background nametable and pattern table address
  ushort _baseNametableAddresss;
  ushort _bgPatternTableAddress;

  // Base sprite pattern table address
  ushort _spritePatternTableAddress;

  // Vram increment per write to PPUDATA
  int _vRamIncrement;

  // Last value written to a PPU register
  byte _lastRegisterWrite;

  // Sprite related flags
  byte _flagSpriteOverflow;
  byte _flagSpriteZeroHit;

  // PPUCTRL Register flags
  byte _flagBaseNametableAddr;
  byte _flagVRamIncrement;
  byte _flagSpritePatternTableAddr;
  byte _flagBgPatternTableAddr;
  byte _flagSpriteSize;
  byte _flagMasterSlaveSelect;
  byte _nmiOutput;

  // NMI Occurred flag
  byte _nmiOccurred;

  // PPUMASK Register flags
  byte _flagGreyscale;
  byte _flagShowBackgroundLeft;
  byte _flagShowSpritesLeft;
  byte _flagShowBackground;
  byte _flagShowSprites;
  byte _flagEmphasizeRed;
  byte _flagEmphasizeGreen;
  byte _flagEmphasizeBlue;

  // Internal PPU Registers
  ushort v; // Current VRAM address (15 bits)
  ushort t; // Temporary VRAM address (15 bits)
  byte x; // Fine X scroll (3 bits)
  byte w; // First or second write toggle (1 bit)

  // PPUDATA buffer
  byte _ppuDataBuffer;

  public Ppu(Console console)
  {
    _memory = console.PpuMemory;
    _console = console;
    _bitmapData = new byte[256 * 240];

    _scanline = 0;
    _cycle = 0;

    _nmiOccurred = 0;
    _nmiOutput = 0;

    w = 0;

    _oam = new byte[256];
    _secondaryOam = new byte[32];
  }

  byte LookupBackgroundColor(int paletteNum, int colorNum)
  {
    // Special case for universal background color
    if (colorNum == 0) return _memory.Read(0x3F00);

    ushort paletteAddress;
    switch (paletteNum)
    {
      case 0: paletteAddress = (ushort) 0x3F01;
        break;
      case 1: paletteAddress = (ushort) 0x3F05;
        break;
      case 2: paletteAddress = (ushort) 0x3F09;
        break;
      case 3: paletteAddress = (ushort) 0x3F0D;
        break;
      default:
        throw new Exception("Invalid background palette Number: " + paletteNum.ToString());
    }

    paletteAddress += (ushort) (colorNum - 1);
    return _memory.Read(paletteAddress);
  }

  byte LookupSpriteColor(int paletteNum, int colorNum)
  {
    // Special case for universal background color
    if (colorNum == 0) return _memory.Read(0x3F00);

    ushort paletteAddress;
    switch (paletteNum)
    {
      case 0: paletteAddress = (ushort) 0x3F11;
        break;
      case 1: paletteAddress = (ushort) 0x3F15;
        break;
      case 2: paletteAddress = (ushort) 0x3F19;
        break;
      case 3: paletteAddress = (ushort) 0x3F1D;
        break;
      default:
        throw new Exception("Invalid background palette Number: " + paletteNum.ToString());
    }

    paletteAddress += (ushort) (colorNum - 1);
    return _memory.Read(paletteAddress);
  }

  byte GetCurrAttributeBits()
  {
    bool isLeft = (CoarseX() % 4) < 2;
    bool isTop = (CoarseY() % 4) < 2;

    byte bits;
    if (isTop)
    {
      if (isLeft) bits = (byte) (_currAttributeTableByte >> 0); // Top left
      else bits = (byte) (_currAttributeTableByte >> 2); // Top right
    } 
    else
    {
      if (isLeft) bits = (byte) (_currAttributeTableByte >> 4);  // Bottom left
      else bits = (byte) (_currAttributeTableByte >> 6); // Bottom right
    }

    return (byte) (bits & 0x3);
  }

  byte GetSpritePixelColor()
  {
    if (_flagShowSprites == 0) return 0;

    int xPos = _cycle - 1;
    int yPos = _scanline - 1;
    byte colorValue = 0;

    // Get sprite pattern bitfield
    for(int i=0;i<_numSprites*4;i+=4)
    {
      int spriteXLeft = _secondaryOam[i + 3];
      int offset = xPos - spriteXLeft;

      if (offset <= 7 && offset >= 0)
      {
        // Found intersecting sprite
        byte patternIndex = _secondaryOam[i + 1];
        int yOffset = yPos - _secondaryOam[i];

        ushort patternAddress = (ushort) (_spritePatternTableAddress + (patternIndex * 16));
      
        bool flipHoriz = (_secondaryOam[i + 2] & 0x40) != 0;
        bool flipVert = (_secondaryOam[i + 2] & 0x80) != 0;
        int colorNum = GetPatternPixel(patternAddress, offset, yOffset, flipHoriz, flipVert);

        // Handle transparent sprites
        if (colorNum == 0)
        {
          return 0;
        } 
        else
        {
          byte paletteNum = (byte) (_secondaryOam[i + 2] & 0x03);
          return LookupSpriteColor(paletteNum, colorNum);
        }
      }
    }

    return colorValue;
  }

  // Gets the CHR of the current background pixel as specified in nametablebyte
  byte GetBackgroundPixelColor()
  {
    if (_flagShowBackground == 0) return 0;

    int colorNum = GetPatternPixel(_currBgPatternAddress, FineX(), FineY());

    // Lookup and return color
    byte color = LookupBackgroundColor(GetCurrAttributeBits(), colorNum);
    return color;
  }

  void CopyHorizPositionData()
  {
    // v: ....F.. ...EDCBA = t: ....F.. ...EDCBA
    v = (ushort) ((v & 0x7BE0) | t);
  }

  void CopyVertPositionData()
  {
    // v: IHGF.ED CBA..... = t: IHGF.ED CBA.....
    v = (ushort) ((v & 0x041F) | t);
  }

  void RenderPixel()
  {
    int pixelX = _cycle - 1;
    int pixelY = _scanline - 1;

    byte backgroundPixel = GetBackgroundPixelColor();
    byte spritePixel = GetSpritePixelColor();

    _bitmapData[pixelY * 256 + pixelX] = spritePixel == 0 ? backgroundPixel :spritePixel;
  }

  int CoarseX()
  {
    return v & 0x1f;
  }

  int FineX()
  {
    return (_cycle + x) % 8;
  }

  int CoarseY()
  {
    return (v >> 5) & 0x1f;
  }

  int FineY()
  {
    return (v >> 12) & 0x7;
  }

  void UpdateNametableByte()
  {
    ushort currNameTableAddr = (ushort) (_baseNametableAddresss + (ushort) (CoarseY() * 32 + CoarseX()));
    _currNametableByte = _memory.Read(currNameTableAddr);
    _currBgPatternAddress = (ushort) (_bgPatternTableAddress + (_currNametableByte * 16));
  }

  int GetPatternPixel(ushort patternAddr, int x, int y, bool flipHoriz=false, bool flipVert=false)
  {
    // Flip x and y if needed
    x = flipHoriz ? 7 - x : x;
    y = flipVert ? 7 - y : y;
    
    // First byte in bitfield
    ushort yAddr = (ushort) (patternAddr + y);

    // Read the 2 bytes in the bitfield for the y coordinate
    byte[] pattern = new byte[2];
    pattern[0] = _memory.Read(yAddr);
    pattern[1] = _memory.Read((ushort) (yAddr + 8));

    // Extract correct bits based on x coordinate
    byte loBit = (byte) ((pattern[0] >> (7 - x)) & 1);
    byte hiBit = (byte) ((pattern[1] >> (7 - x)) & 1);

    return ((hiBit << 1) | loBit) & 0x03;
  }

  void UpdateAttributeTableByte()
  {
    // Atribute tables operate on 4x4 tile blocks
    int blockX = CoarseX() / 4;
    int blockY = CoarseY() / 4;

    int attributeByteIndex = (blockY * 8) + blockX;
    _currAttributeTableByte = _memory.Read((ushort) (_baseNametableAddresss + 960 + attributeByteIndex)); // Attribute tables are 960 bytes from start of nametable
  }

  void IncrementX()
  {
    if ((v & 0x001F) == 31)
    {
      v = (ushort) (v & (~0x001F)); // Reset Coarse X
      v = (ushort) (v ^ 0x0400); // Switch horizontal nametable
    }
    else
    {
      v ++; // Increment Coarse X
    }
  }

  void IncrementY()
  {
    if ((v & 0x7000) != 0x7000)
    { // if fine Y < 7
      v += 0x1000; // increment fine Y
    } 
    else
    {
      v = (ushort) (v & ~0x7000u & 0xFFFF); // Set fine Y to 0
      int y = (v & 0x03E0) >> 5; // y = coarse Y
      if (y == 29)
      {
        y = 0; // coarse Y = 0
        v = (ushort) (v ^ 0x0800); // switch vertical nametable
      }
      else if (y == 31)
      {
        y = 0; // coarse Y = 0, nametable not switched
      }
      else
      {
        y += 1; // Increment coarse Y
        v = (ushort) ((v & ~0x03E0) | (y << 5)); // Put coarse Y back into v
      }
    }
  }

  void EvalSprites()
  {
    _numSprites = 0;
    int yPos = _scanline;
    int _secondaryOamIndex = 0;

    for (int i=0;i<_oam.Length;i+=4)
    {
      if (_secondaryOamIndex == 32)
      {
        _flagSpriteOverflow = 1;
        break;
      }

      byte spriteYTop = _oam[i];

      // spriteYTop == 0 indicates that this is not a sprite (ie. no more sprites after)
      if (spriteYTop == 0)
      {
        break;
      }

      int offset = yPos - spriteYTop;

      // If this sprite is on the next _scanline, copy it to secondary _oam
      if (offset <= 7 && offset >= 0)
      {
        Array.Copy(_oam, i, _secondaryOam, _secondaryOamIndex, 4);
        _secondaryOamIndex += 4;
        _numSprites ++;
      }
    }
  }

  void HandleRenderScanline()
  {
    bool isRenderingCycle = _cycle > 0 && _cycle <= 256;

    // Fetch new rendering information if needed and if this is a rendering _cycle
    if (_cycle % 8 == 0 && isRenderingCycle)
    {
      IncrementX();
      UpdateNametableByte();
    }
    if (_cycle % 32 == 0 && isRenderingCycle)
    {
      UpdateAttributeTableByte();
    }
    
    // Increment Y in v register if needed
    if (_cycle == 256) IncrementY();

    // Evaluate sprites on _cycle 257
    // Actual sprite evaluation runs on multiple _cycles
    // but this works
    if (_cycle == 257 && _scanline != 0) EvalSprites();

    if (_cycle == 0)
    {
       // Do nothing, idle _cycle
    }
    else if (_cycle >= 1 && _cycle <= 256) // Rendering cycles
    {
      RenderPixel();
    }
    else
    {
      // Cycles that fetch data for next _scanline
      // TODO Implement fetching of data for next _scanline
    }
  }

  public void Step()
  {
    // Trigger an NMI at the start of _scanline 241 if VBLANK NMI's are enabled
    if (_scanline == 241 && _cycle == 1)
    {
      _nmiOccurred = 1;
      if (_nmiOccurred != 0 && _nmiOutput != 0) _console.Cpu.TriggerNmi();
    }

    if (_scanline == 0) _nmiOccurred = 0;

    bool renderingEnabled = (_flagShowBackground != 0) || (_flagShowSprites != 0);

    if (renderingEnabled) 
    {
      if (_scanline == 0) // Non rendering scanline
      {
        // Sprite overflow flag cleared at dot 1 of _scanline 0
        if (_cycle == 1) _flagSpriteOverflow = 0;
      }
      else if (_scanline >= 1 && _scanline < 240) // Rendering _scanlines
      {
        HandleRenderScanline();
      }
      else if (_scanline == 240) // Idle _scanline
      {

      } 
      else if (_scanline > 240 && _scanline < 260) // Memory fetch _scanlines
      { 
        // TODO Add memory fetch stuff here
      }
      else if (_scanline == 260) // Vblank, next frame
      {
        // TODO Add vblank stuff here
      }
    }

    _cycle ++;

    if (renderingEnabled)
    {
      // Copy horizontal position data from t to v on _cycle 257 of each _scanline if rendering enabled
      if (_cycle == 257) CopyHorizPositionData();
      
      // Copy vertical position data from t to v repeatedly from _cycle 280 to 304 (if rendering is enabled)
      if (_cycle >= 280 && _cycle <= 304 && _scanline == 0) CopyVertPositionData();
    }

    // Reset _cycle (and _scanline if _scanline == 260)
    // Also set to next frame if at end of last _scanline
    if (_cycle == 340)
    {
      if (_scanline == 260) // Last _scanline, reset to upper left corner
      {
        _scanline = 0;
        _cycle = 0;
        _console.drawFrame();
      }
      else // Not on last _scanline
      { 
        _cycle = 0;
        _scanline ++;
      }
    }
  }

  public byte ReadFromRegister(ushort address)
  {
    byte data;
    switch (address)
    {
      case 0x2002: data = ReadPpuStatus();
        break;
      case 0x2004: data = ReadOamData();
        break;
      case 0x2007: data = ReadPpuData();
        break;
      default:
        throw new Exception("Invalid PPU Register read from register: " + address.ToString("X4"));
    }

    return data;
  }

  public void WriteToRegister(ushort address, byte data)
  {
    _lastRegisterWrite = data;
    switch (address)
    {
      case 0x2000: WritePpuCtrl(data);
        break;
      case 0x2001: WritePpuMask(data);
        break;
      case 0x2003: WriteOamAddr(data);
        break;
      case 0x2004: WriteOamData(data);
        break;
      case 0x2005: WritePpuScroll(data);
        break;
      case 0x2006: WritePpuAddr(data);
        break;
      case 0x2007: WritePpuData(data);
        break;
      case 0x4014: WriteOamDma(data);
        break;
      default:
        throw new Exception("Invalid PPU Register write to register: " + address.ToString("X4"));
    }
  }
  
  // $2000
  void WritePpuCtrl(byte data)
  {
    _flagBaseNametableAddr = (byte) (data & 0x3);
    _flagVRamIncrement = (byte) ((data >> 2) & 1);
    _flagSpritePatternTableAddr = (byte) ((data >> 3) & 1);
    _flagBgPatternTableAddr = (byte) ((data >> 4) & 1);
    _flagSpriteSize = (byte) ((data >> 5) & 1);
    _flagMasterSlaveSelect = (byte) ((data >> 6) & 1);
    _nmiOutput = (byte) ((data >> 7) & 1);

    // Set values based off flags
    _baseNametableAddresss = (ushort) (0x2000 + 0x4000*_flagBaseNametableAddr);
    _vRamIncrement = (_flagVRamIncrement == 0) ? 1 : 32;
    _bgPatternTableAddress = (ushort) (_flagBgPatternTableAddr == 0 ? 0x0000 : 0x1000);
    _spritePatternTableAddress = (ushort) (0x1000 * _flagSpritePatternTableAddr);


    // t: ...BA.. ........ = d: ......BA
    t = (ushort) ((t & 0xF3FF) | ((data & 0x03) << 10));
  }

  // $2001
  void WritePpuMask(byte data)
  {
    _flagGreyscale = (byte) (data & 1);
    _flagShowBackgroundLeft = (byte) ((data >> 1) & 1);
    _flagShowSpritesLeft = (byte) ((data >> 2) & 1);
    _flagShowBackground = (byte) ((data >> 3) & 1);
    _flagShowSprites = (byte) ((data >> 4) & 1);
    _flagEmphasizeRed = (byte) ((data >> 5) & 1);
    _flagEmphasizeGreen = (byte) ((data >> 6) & 1);
    _flagEmphasizeBlue = (byte) ((data >> 7) & 1);
  }

  // $4014
  void WriteOamAddr(byte data)
  {
    _oamAddr = data;
  }

  // $2004
  void WriteOamData(byte data)
  {
    _oam[_oamAddr] = data;
    _oamAddr ++;
  }

  // $2005
  void WritePpuScroll(byte data)
  {
    if (w == 0) // First write
    {
      // t: ....... ...HGFED = d: HGFED...
      // x:              CBA = d: .....CBA
      // w:                  = 1
      t = (ushort) ((t & 0xFFE0) | (data << 3));
      x = (byte) (data & 0x07);
      w = 1;
    }
    else
    {
      // t: CBA..HG FED..... = d: HGFEDCBA
      // w:                  = 0
      t = (ushort) (t & 0xC1F);
      t |= (ushort) ((data & 0x07) << 13); // CBA
      t |= (ushort) ((data & 0xF8) << 2); // HG FED
      w = 0;
    }
  }

  // $2006
  void WritePpuAddr(byte data)
  {
    if (w == 0)  // First write
    {
      // t: .FEDCBA ........ = d: ..FEDCBA
      // t: X...... ........ = 0
      // w:                  = 1
      t = (ushort) ((t & 0x00FF) | (data << 8));
      w = 1;
    }
    else
    {
      // t: ....... HGFEDCBA = d: HGFEDCBA
      // v                   = t
      // w:                  = 0
      t = (ushort) ((t & 0xFF00) | data);
      v = t;
      w = 0;
    }
  }

  // $2007
  void WritePpuData(byte data)
  {
    _memory.Write(v, data);
    v += (ushort) (_vRamIncrement);
  }

  // $4014
  void WriteOamDma(byte data)
  {
    ushort startAddr = (ushort) (data << 8);
    _console.CpuMemory.ReadBuf(_oam, startAddr, 256);
  }

  // $2002
  byte ReadPpuStatus()
  {
    byte retVal = 0;
    retVal |= (byte) (_lastRegisterWrite & 0x1F); // Least signifigant 5 bits of last register write
    retVal |= (byte) (_flagSpriteOverflow << 5);
    retVal |= (byte) (_flagSpriteZeroHit << 6);
    retVal |= (byte) (_nmiOccurred << 7);

    // Old status of _nmiOccurred is returned then _nmiOccurred is cleared
    _nmiOccurred = 0;

    // w:                  = 0
    w = 0;

    return retVal;
  }

  // $2004
  byte ReadOamData()
  {
    return _oam[_oamAddr];
  }

  // $2007
  byte ReadPpuData()
  {
    byte data = _memory.Read(v);
    if (v < 0x3F00)
    {
      byte bufferedData = _ppuDataBuffer;
      _ppuDataBuffer = data;
      data = bufferedData;
    }

    v += (ushort) (_vRamIncrement);
    return data;
  }
}