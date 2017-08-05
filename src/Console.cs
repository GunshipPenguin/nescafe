using System;
using System.Drawing;
using System.Drawing.Imaging;

public class Console {
  public Cpu cpu;
  public CpuMemory cpuMemory;

  public Ppu ppu;
  public PpuMemory ppuMemory;

  public Cartridge cartridge;

  public Action<byte[]> drawAction;

  public Console(Cartridge cartridge) {
    this.cartridge = cartridge;

    cpuMemory = new CpuMemory(this);
    ppuMemory = new PpuMemory(this);

    cpu = new Cpu(this);
    ppu = new Ppu(this);
  }

  public void drawFrame() {
    drawAction(ppu.getScreen());
  }

  public void start() {
    byte[] bitmapData = ppu.BitmapData;
    drawAction(ppu.getScreen());

    while (true) {
      int cycles = cpu.step();

      // 3 PPU cycles for each CPU cycle
      for (int i=0;i<cycles*3;i++) {
        ppu.step();
      }
    }
  }
}
