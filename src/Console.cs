using System;

public class Console {
  public Cpu cpu;
  public CpuMemory cpuMemory;
  public Cartridge cartridge;

  public Console(Cartridge cartridge) {
    this.cartridge = cartridge;
    
    cpuMemory = new CpuMemory(this);
    cpu = new Cpu(this);
  }

  public void start() {
    cpu.start();
  }
}
