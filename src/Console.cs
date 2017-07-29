using System;

public class Console {
  Cpu cpu;
  public Memory memory;

  public Console(Cartridge cartridge) {
    memory = new Memory(cartridge);
    cpu = new Cpu(memory);
  }

  public void start() {
    cpu.start();
  }
}
