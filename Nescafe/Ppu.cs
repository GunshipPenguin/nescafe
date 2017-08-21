using System;

namespace Nescafe
{
    public class Ppu
    {
        public byte[] BitmapData { get; }

        PpuMemory _memory;
        Console _console;

        // OAM / Sprite rendering
        byte[] _oam;
        ushort _oamAddr;
        byte[] _sprites;
        int _numSprites;

        int _scanline;
        int _cycle;

        // Base background nametable address
        ushort _baseNametableAddress;

        // Address of pattern table used for background
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
        byte f; // Even odd flag (even = 0, odd = 1)

        // Tile shift register and variables (latches) that feed it every 8 cycles
        ulong _tileShiftReg;
        byte _nameTableByte;
        byte _attributeTableByte;
        byte _tileBitfieldLo;
        byte _tileBitfieldHi;

        // PPUDATA buffer
        byte _ppuDataBuffer;

        public Ppu(Console console)
        {
            _memory = console.PpuMemory;
            _console = console;

            BitmapData = new byte[256 * 240];

            _oam = new byte[256];
            _sprites = new byte[32];
        }

        public void Reset()
        {
            Array.Clear(BitmapData, 0, BitmapData.Length);

            _scanline = 240;
            _cycle = 340;

            _nmiOccurred = 0;
            _nmiOutput = 0;

            w = 0;
            f = 0;

            Array.Clear(_oam, 0, _oam.Length);
            Array.Clear(_sprites, 0, _sprites.Length);
        }

        byte LookupBackgroundColor(byte data)
        {
            int colorNum = data & 0x3;
            int paletteNum = (data >> 2) & 0x3;

            // Special case for universal background color
            if (colorNum == 0) return _memory.Read(0x3F00);

            ushort paletteAddress;
            switch (paletteNum)
            {
                case 0:
                    paletteAddress = (ushort)0x3F01;
                    break;
                case 1:
                    paletteAddress = (ushort)0x3F05;
                    break;
                case 2:
                    paletteAddress = (ushort)0x3F09;
                    break;
                case 3:
                    paletteAddress = (ushort)0x3F0D;
                    break;
                default:
                    throw new Exception("Invalid background palette Number: " + paletteNum.ToString());
            }

            paletteAddress += (ushort)(colorNum - 1);
            return _memory.Read(paletteAddress);
        }

        byte LookupSpriteColor(byte data)
        {
            int colorNum = data & 0x3;
            int paletteNum = (data >> 2) & 0x3;

            // Special case for universal background color
            if (colorNum == 0) return _memory.Read(0x3F00);

            ushort paletteAddress;
            switch (paletteNum)
            {
                case 0:
                    paletteAddress = (ushort)0x3F11;
                    break;
                case 1:
                    paletteAddress = (ushort)0x3F15;
                    break;
                case 2:
                    paletteAddress = (ushort)0x3F19;
                    break;
                case 3:
                    paletteAddress = (ushort)0x3F1D;
                    break;
                default:
                    throw new Exception("Invalid background palette Number: " + paletteNum.ToString());
            }

            paletteAddress += (ushort)(colorNum - 1);
            return _memory.Read(paletteAddress);
        }

        byte GetSpritePixelData(out int spriteIndex)
        {
            spriteIndex = 0;
            if (_flagShowSprites == 0) return 0;

            int xPos = _cycle - 1;
            int yPos = _scanline - 1;

            // Get sprite pattern bitfield
            for (int i = 0; i < _numSprites * 4; i += 4)
            {
                int spriteXLeft = _sprites[i + 3];
                int offset = xPos - spriteXLeft;

                if (offset <= 7 && offset >= 0)
                {
                    // Found intersecting sprite
                    byte patternIndex = _sprites[i + 1];
                    int yOffset = yPos - _sprites[i];

                    ushort patternAddress = (ushort)(_spritePatternTableAddress + (patternIndex * 16));

                    bool flipHoriz = (_sprites[i + 2] & 0x40) != 0;
                    bool flipVert = (_sprites[i + 2] & 0x80) != 0;
                    int colorNum = GetPatternPixel(patternAddress, offset, yOffset, flipHoriz, flipVert);

                    // Handle transparent sprites
                    if (colorNum == 0)
                    {
                        continue;
                    }
                    else // Non transparent sprite, return data
                    {
                        byte paletteNum = (byte)(_sprites[i + 2] & 0x03);
                        spriteIndex = i / 4;
                        return (byte)(((paletteNum << 2) | colorNum) & 0xF);
                    }
                }
            }

            return 0x00; // No sprite
        }

        void CopyHorizPositionData()
        {
            // v: ....F.. ...EDCBA = t: ....F.. ...EDCBA
            v = (ushort)((v & 0x7BE0) | t);
        }

        void CopyVertPositionData()
        {
            // v: IHGF.ED CBA..... = t: IHGF.ED CBA.....
            v = (ushort)((v & 0x041F) | t);
        }

        int CoarseX()
        {
            return v & 0x1f;
        }

        int CoarseY()
        {
            return (v >> 5) & 0x1f;
        }

        int FineY()
        {
            return (v >> 12) & 0x7;
        }

        int GetPatternPixel(ushort patternAddr, int x, int y, bool flipHoriz = false, bool flipVert = false)
        {
            // Flip x and y if needed
            x = flipHoriz ? 7 - x : x;
            y = flipVert ? 7 - y : y;

            // First byte in bitfield
            ushort yAddr = (ushort)(patternAddr + y);

            // Read the 2 bytes in the bitfield for the y coordinate
            byte[] pattern = new byte[2];
            pattern[0] = _memory.Read(yAddr);
            pattern[1] = _memory.Read((ushort)(yAddr + 8));

            // Extract correct bits based on x coordinate
            byte loBit = (byte)((pattern[0] >> (7 - x)) & 1);
            byte hiBit = (byte)((pattern[1] >> (7 - x)) & 1);

            return ((hiBit << 1) | loBit) & 0x03;
        }

        void IncrementX()
        {
            if ((v & 0x001F) == 31)
            {
                v = (ushort)(v & (~0x001F)); // Reset Coarse X
                v = (ushort)(v ^ 0x0400); // Switch horizontal nametable
            }
            else
            {
                v++; // Increment Coarse X
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
                v = (ushort)(v & ~0x7000u & 0xFFFF); // Set fine Y to 0
                int y = (v & 0x03E0) >> 5; // y = coarse Y
                if (y == 29)
                {
                    y = 0; // coarse Y = 0
                    v = (ushort)(v ^ 0x0800); // switch vertical nametable
                }
                else if (y == 31)
                {
                    y = 0; // coarse Y = 0, nametable not switched
                }
                else
                {
                    y += 1; // Increment coarse Y
                    v = (ushort)((v & ~0x03E0) | (y << 5)); // Put coarse Y back into v
                }
            }
        }

        void EvalSprites()
        {
            _numSprites = 0;
            int yPos = _scanline;

            for (int i = 0; i < _oam.Length; i += 4)
            {
                if (_numSprites == 8)
                {
                    _flagSpriteOverflow = 1;
                    break;
                }

                byte spriteYTop = _oam[i];

                // spriteYTop == 0 indicates that this is not a sprite (ie. no more sprites after)
                if (spriteYTop == 0) break;

                int offset = yPos - spriteYTop;

                // If this sprite is on the next scanline, copy it to the _sprites array for rendering
                if (offset <= 7 && offset >= 0)
                {
                    Array.Copy(_oam, i, _sprites, _numSprites * 4, 4);
                    _numSprites++;
                }
            }
        }

        void RenderPixel()
        {
            // Get pixel data (4 bits of tile shift register as specified by x)
            byte bgPixelData = (byte)((_tileShiftReg >> (x * 4)) & 0xF);

            int spriteIndex;
            byte spritePixelData = GetSpritePixelData(out spriteIndex);

            int bgColorNum = bgPixelData & 0x03;
            int spriteColorNum = spritePixelData & 0x03;

            byte color;

            if (bgColorNum == 0)
            {
                if (spriteColorNum == 0) color = LookupBackgroundColor(bgPixelData);
                else color = LookupSpriteColor(spritePixelData);
            }
            else
            {
                if (spriteColorNum == 0) color = LookupBackgroundColor(bgPixelData);
                else // Both pixels opaque, choose depending on sprite priority
                {
                    // Set sprite0hit if spriteIndex is 0
                    if (spriteIndex == 0) _flagSpriteZeroHit = 1;

                    // Get sprite priority
                    int priority = (_sprites[(spriteIndex * 4) + 2] >> 5) & 1;
                    if (priority == 1) color = LookupBackgroundColor(bgPixelData);
                    else color = LookupSpriteColor(spritePixelData);
                }
            }

            BitmapData[_scanline * 256 + (_cycle - 1)] = color;
        }

        void FetchNametableByte()
        {
            ushort address = (ushort)(0x2000 | (v & 0x0FFF));
            _nameTableByte = _memory.Read(address);
        }

        void FetchAttributeTableByte()
        {
            ushort address = (ushort)(0x23C0 | (v & 0x0C00) | ((v >> 4) & 0x38) | ((v >> 2) & 0x07));
            _attributeTableByte = _memory.Read(address);
        }

        void FetchTileBitfieldLo()
        {
            ushort address = (ushort)(_bgPatternTableAddress + (_nameTableByte * 16) + FineY());
            _tileBitfieldLo = _memory.Read(address);
        }

        void FetchTileBitfieldHi()
        {
            ushort address = (ushort)(_bgPatternTableAddress + (_nameTableByte * 16) + FineY() + 8);
            _tileBitfieldHi = _memory.Read(address);
        }

        // Stores data for the next 8 pixels in the upper 32 bits of _tileShiftReg
        void StoreTileData()
        {
            byte _palette = (byte)((_attributeTableByte >> ((CoarseX() & 0x2) | ((CoarseY() & 0x2) << 1))) & 0x3);

            // Upper 32 bits to add to _tileShiftReg
            ulong data = 0;

            for (int i = 0; i < 8; i++)
            {
                // Get color number
                byte loColorBit = (byte)((_tileBitfieldLo >> (7 - i)) & 1);
                byte hiColorBit = (byte)((_tileBitfieldHi >> (7 - i)) & 1);
                byte colorNum = (byte)((hiColorBit << 1) | (loColorBit) & 0x03);

                // Add palette number
                byte fullPixelData = (byte)(((_palette << 2) | colorNum) & 0xF);

                data |= (uint)(fullPixelData << (4 * i));
            }

            _tileShiftReg &= 0xFFFFFFFF;
            _tileShiftReg |= (data << 32);
        }

        void UpdateCounters()
        {
            // Trigger an NMI at the start of _scanline 241 if VBLANK NMI's are enabled
            if (_scanline == 241 && _cycle == 1)
            {
                _nmiOccurred = 1;
                if (_nmiOccurred != 0 && _nmiOutput != 0) _console.Cpu.TriggerNmi();
            }

            bool renderingEnabled = (_flagShowBackground != 0) || (_flagShowSprites != 0);

            // Skip last cycle of prerender scanline on odd frames
            if (renderingEnabled)
            {
                if (_scanline == 261 && f == 1 && _cycle == 339)
                {
                    f ^= 1;
                    _scanline = 0;
                    _cycle = -1;
                    _console.DrawFrame();
                    return;
                }
            }
            _cycle++;

            // Reset cycle (and scanline if scanline == 260)
            // Also set to next frame if at end of last _scanline
            if (_cycle > 340)
            {
                if (_scanline == 261) // Last scanline, reset to upper left corner
                {
                    f ^= 1;
                    _scanline = 0;
                    _cycle = -1;
                    _console.DrawFrame();
                }
                else // Not on last scanline
                {
                    _cycle = -1;
                    _scanline++;
                }
            }
        }

        public void Step()
        {
            UpdateCounters();

            // Cycle types
            bool renderingEnabled = (_flagShowBackground != 0) || (_flagShowSprites != 0);
            bool renderCycle = _cycle > 0 && _cycle <= 256;
            bool preFetchCycle = _cycle >= 321 && _cycle <= 336;
            bool fetchCycle = renderCycle || preFetchCycle;

            // Scanline types
            bool renderScanline = _scanline >= 0 && _scanline < 240;
            bool idleScanline = _scanline == 240;
            bool vBlankScanline = _scanline > 240;
            bool preRenderScanline = _scanline == 261;

            // nmiOccurred flag cleared on prerender scanline at cycle 1
            if (preRenderScanline && _cycle == 1)
            {
                _nmiOccurred = 0;
                _flagSpriteOverflow = 0;
                _flagSpriteZeroHit = 0;
            }

            // Evaluate sprites at cycle 257
            if (_cycle == 257 && renderScanline) EvalSprites();

            if (renderingEnabled)
            {
                if (renderCycle && renderScanline) RenderPixel();

                // Read rendering data into internal latches and update _tileShiftReg
                // with those latches every 8 cycles
                // https://wiki.nesdev.com/w/images/d/d1/Ntsc_timing.png
                if (fetchCycle && (renderScanline || preRenderScanline))
                {
                    _tileShiftReg >>= 4;
                    switch (_cycle % 8)
                    {
                        case 1:
                            FetchNametableByte();
                            break;
                        case 3:
                            FetchAttributeTableByte();
                            break;
                        case 5:
                            FetchTileBitfieldLo();
                            break;
                        case 7:
                            FetchTileBitfieldHi();
                            break;
                        case 0:
                            StoreTileData();
                            IncrementX();
                            if (_cycle == 256) IncrementY();
                            break;
                    }
                }
            }

            if (renderingEnabled)
            {
                // Copy horizontal position data from t to v on _cycle 257 of each scanline if rendering enabled
                if (_cycle == 257) CopyHorizPositionData();

                // Copy vertical position data from t to v repeatedly from cycle 280 to 304 (if rendering is enabled)
                if (_cycle >= 280 && _cycle <= 304 && _scanline == 261) CopyVertPositionData();
            }
        }

        public byte ReadFromRegister(ushort address)
        {
            byte data;
            switch (address)
            {
                case 0x2002:
                    data = ReadPpuStatus();
                    break;
                case 0x2004:
                    data = ReadOamData();
                    break;
                case 0x2007:
                    data = ReadPpuData();
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
                case 0x2000:
                    WritePpuCtrl(data);
                    break;
                case 0x2001:
                    WritePpuMask(data);
                    break;
                case 0x2003:
                    WriteOamAddr(data);
                    break;
                case 0x2004:
                    WriteOamData(data);
                    break;
                case 0x2005:
                    WritePpuScroll(data);
                    break;
                case 0x2006:
                    WritePpuAddr(data);
                    break;
                case 0x2007:
                    WritePpuData(data);
                    break;
                case 0x4014:
                    WriteOamDma(data);
                    break;
                default:
                    throw new Exception("Invalid PPU Register write to register: " + address.ToString("X4"));
            }
        }

        // $2000
        void WritePpuCtrl(byte data)
        {
            _flagBaseNametableAddr = (byte)(data & 0x3);
            _flagVRamIncrement = (byte)((data >> 2) & 1);
            _flagSpritePatternTableAddr = (byte)((data >> 3) & 1);
            _flagBgPatternTableAddr = (byte)((data >> 4) & 1);
            _flagSpriteSize = (byte)((data >> 5) & 1);
            _flagMasterSlaveSelect = (byte)((data >> 6) & 1);
            _nmiOutput = (byte)((data >> 7) & 1);

            // Set values based off flags
            _baseNametableAddress = (ushort)(0x2000 + 0x400 * _flagBaseNametableAddr);
            _vRamIncrement = (_flagVRamIncrement == 0) ? 1 : 32;
            _bgPatternTableAddress = (ushort)(_flagBgPatternTableAddr == 0 ? 0x0000 : 0x1000);
            _spritePatternTableAddress = (ushort)(0x1000 * _flagSpritePatternTableAddr);

            // t: ...BA.. ........ = d: ......BA
            t = (ushort)((t & 0xF3FF) | ((data & 0x03) << 10));
        }

        // $2001
        void WritePpuMask(byte data)
        {
            _flagGreyscale = (byte)(data & 1);
            _flagShowBackgroundLeft = (byte)((data >> 1) & 1);
            _flagShowSpritesLeft = (byte)((data >> 2) & 1);
            _flagShowBackground = (byte)((data >> 3) & 1);
            _flagShowSprites = (byte)((data >> 4) & 1);
            _flagEmphasizeRed = (byte)((data >> 5) & 1);
            _flagEmphasizeGreen = (byte)((data >> 6) & 1);
            _flagEmphasizeBlue = (byte)((data >> 7) & 1);
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
            _oamAddr++;
        }

        // $2005
        void WritePpuScroll(byte data)
        {
            if (w == 0) // First write
            {
                // t: ....... ...HGFED = d: HGFED...
                // x:              CBA = d: .....CBA
                // w:                  = 1
                t = (ushort)((t & 0xFFE0) | (data >> 3));
                x = (byte)(data & 0x07);
                w = 1;
            }
            else
            {
                // t: CBA..HG FED..... = d: HGFEDCBA
                // w:                  = 0
                t = (ushort)(t & 0xC1F);
                t |= (ushort)((data & 0x07) << 12); // CBA
                t |= (ushort)((data & 0xF8) << 2); // HG FED
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
                t = (ushort)((t & 0x00FF) | (data << 8));
                w = 1;
            }
            else
            {
                // t: ....... HGFEDCBA = d: HGFEDCBA
                // v                   = t
                // w:                  = 0
                t = (ushort)((t & 0xFF00) | data);
                v = t;
                w = 0;
            }
        }

        // $2007
        void WritePpuData(byte data)
        {
            _memory.Write(v, data);
            v += (ushort)(_vRamIncrement);
        }

        // $4014
        void WriteOamDma(byte data)
        {
            ushort startAddr = (ushort)(data << 8);
            _console.CpuMemory.ReadBuf(_oam, startAddr, 256);

            // OAM DMA always takes at least 513 CPU cycles
            _console.Cpu.AddIdleCycles(513);

            // OAM DMA takes an extra CPU cycle if executed on an odd CPU cycle
            if (_console.Cpu.Cycles % 2 == 1) _console.Cpu.AddIdleCycles(1);
        }

        // $2002
        byte ReadPpuStatus()
        {
            byte retVal = 0;
            retVal |= (byte)(_lastRegisterWrite & 0x1F); // Least signifigant 5 bits of last register write
            retVal |= (byte)(_flagSpriteOverflow << 5);
            retVal |= (byte)(_flagSpriteZeroHit << 6);
            retVal |= (byte)(_nmiOccurred << 7);

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

            v += (ushort)(_vRamIncrement);
            return data;
        }
    }
}
