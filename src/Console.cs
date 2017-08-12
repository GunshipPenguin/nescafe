using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;

public class Console {
  public Cpu cpu;
  public CpuMemory cpuMemory;

  public Ppu ppu;
  public PpuMemory ppuMemory;

  public Cartridge cartridge;

  public Action<byte[]> drawAction;

  public Controller controller;

  bool frameEvenOdd;

  public Console(Cartridge cartridge) {
    this.cartridge = cartridge;

    cpuMemory = new CpuMemory(this);
    ppuMemory = new PpuMemory(this);

    controller = new Controller();

    frameEvenOdd = false;

    cpu = new Cpu(this);
    ppu = new Ppu(this);
  }

  public void drawFrame() {
    drawAction(ppu.getScreen());
    frameEvenOdd = !frameEvenOdd;
  }

  void goUntilFrame() {
    bool orig = frameEvenOdd;

    while (orig == frameEvenOdd) {
      int cpuCycles = cpu.step();

      // 3 PPU cycles for each CPU cycle
      for (int i=0;i<cpuCycles*3;i++) {
        ppu.step();
      }
    }
  }

  public void start() {
    byte[] bitmapData = ppu.BitmapData;
    drawAction(ppu.getScreen());

    
    while (true) {
      for (int i=0;i<60;i++) {
        Stopwatch frameWatch = Stopwatch.StartNew();
        goUntilFrame();
        frameWatch.Stop();

        long timeTaken = frameWatch.ElapsedMilliseconds;

        Thread.Sleep((int) ((1000.0/60) - timeTaken));
      }
    }
  }
}
