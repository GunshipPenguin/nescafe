using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;

public class Console
{
  public readonly Cpu Cpu;
  public readonly CpuMemory CpuMemory;
  public readonly Ppu Ppu;
  public readonly PpuMemory PpuMemory;
  public readonly Cartridge Cartridge;
  public readonly Controller Controller;

  public Action<byte[]> DrawAction { get; set; }

  bool _frameEvenOdd;

  public Console(Cartridge cartridge)
  {
    this.Cartridge = cartridge;

    CpuMemory = new CpuMemory(this);
    PpuMemory = new PpuMemory(this);

    Cpu = new Cpu(this);
    Ppu = new Ppu(this);

    Controller = new Controller();

    _frameEvenOdd = false;
  }

  public void drawFrame()
  {
    DrawAction(Ppu.BitmapData);
    _frameEvenOdd = !_frameEvenOdd;
  }

  void goUntilFrame()
  {
    bool orig = _frameEvenOdd;
    while (orig == _frameEvenOdd)
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
