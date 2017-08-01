using System;

public class Console {
  public Cpu cpu;
  public CpuMemory cpuMemory;

  public Ppu ppu;
  public PpuMemory ppuMemory;

  public Cartridge cartridge;

  public Console(Cartridge cartridge) {
    this.cartridge = cartridge;

    cpuMemory = new CpuMemory(this);
    ppuMemory = new PpuMemory(this);

    cpu = new Cpu(this);
    ppu = new Ppu(this);
  }

  public void start() {
    cpu.start();
  }
}
