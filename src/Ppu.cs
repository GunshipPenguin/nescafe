using System;

public class Ppu {
    CpuMemory _cpuMemory;
    PpuMemory _memory;

    public Ppu(Console console) {
        _cpuMemory = console.cpuMemory;
        _memory = console.ppuMemory;
    }
}