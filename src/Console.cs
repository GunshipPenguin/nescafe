using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;

public class Console
{
  Cpu _cpu;
  public Cpu Cpu 
  {
    get
    {
      return _cpu;
    }
    set 
    {
      _cpu = value;
    }
  }

  CpuMemory _cpuMemory;
  public CpuMemory CpuMemory
  {
    get
    {
      return _cpuMemory;
    }
    set
    {
      _cpuMemory = value;
    }
  }

  Ppu _ppu;
  public Ppu Ppu
  {
    get
    {
      return _ppu;
    }
    set
    {
      _ppu = value;
    }
  }

  PpuMemory _ppuMemory;
  public PpuMemory PpuMemory
  {
    get
    {
      return _ppuMemory;
    }
    set
    {
      _ppuMemory = value;
    }
  }

  Cartridge _cartridge;
  public Cartridge Cartridge
  {
    get
    {
      return _cartridge;
    }
    set
    {
      _cartridge = value;
    }
  }

  Action<byte[]> _drawAction;
  public Action<byte[]> DrawAction
  {
    get
    {
      return _drawAction;
    }
    set
    {
      _drawAction = value;
    }
  }

  Controller _controller;
  public Controller Controller
  {
    get
    {
      return _controller;
    }
    set
    {
      _controller = value;
    }
  }

  bool frameEvenOdd;

  public Console(Cartridge cartridge)
  {
    this.Cartridge = cartridge;

    CpuMemory = new CpuMemory(this);
    PpuMemory = new PpuMemory(this);

    Controller = new Controller();

    frameEvenOdd = false;

    Cpu = new Cpu(this);
    Ppu = new Ppu(this);
  }

  public void drawFrame()
  {
    DrawAction(Ppu.BitmapData);
    frameEvenOdd = !frameEvenOdd;
  }

  void goUntilFrame()
  {
    bool orig = frameEvenOdd;

    while (orig == frameEvenOdd)
    {
      int cpuCycles = Cpu.Step();

      // 3 PPU cycles for each CPU cycle
      for (int i=0;i<cpuCycles*3;i++)
      {
        Ppu.Step();
      }
    }
  }

  public void Start()
  {
    byte[] bitmapData = Ppu.BitmapData;
    DrawAction(Ppu.BitmapData);

    
    while (true)
    {
      for (int i=0;i<60;i++)
      {
        Stopwatch frameWatch = Stopwatch.StartNew();
        goUntilFrame();
        frameWatch.Stop();

        long timeTaken = frameWatch.ElapsedMilliseconds;

        Thread.Sleep((int) ((1000.0/60) - timeTaken));
      }
    }
  }
}
