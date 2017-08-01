using System;

public class Ppu {
  CpuMemory _cpuMemory;
  PpuMemory _memory;

  public Ppu(Console console) {
    _cpuMemory = console.cpuMemory;
    _memory = console.ppuMemory;
  }

  public void writePpuCtrl(byte data) {

  }

  public void writePpuMask(byte data) {

  }

  public void writePpuStatus(byte data) {

  }

  public void writeOamAddr(byte data) {

  }

  public void writeOamData(byte data) {

  }

  public void writePpuScroll(byte data) {

  }

  public void writePpuData(byte data) {

  }

  public void writeOamDma(byte data) {

  }
}