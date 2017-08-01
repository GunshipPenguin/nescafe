using System;

public class Console {
  public Cpu cpu;
  public CpuMemory cpuMemory;

  public Console(Cartridge cartridge) {
    cpuMemory = new CpuMemory(cartridge);
    cpu = new Cpu(this);
  }

  public void start() {
    cpu.start();
  }
}
