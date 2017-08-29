using System;
using System.Threading;
using System.Diagnostics;
using Nescafe.Mappers;

namespace Nescafe
{
    public class Console
    {
        public readonly Cpu Cpu;
        public readonly Ppu Ppu;

        public readonly CpuMemory CpuMemory;
        public readonly PpuMemory PpuMemory;

        public readonly Controller Controller;

        public Cartridge Cartridge { get; set; }
        public Mapper Mapper { get; set; }

        public Action<byte[]> DrawAction { get; set; }

        public bool Stop { get; set; }

        bool _frameEvenOdd;

        public Console()
        {
            Controller = new Controller();

            CpuMemory = new CpuMemory(this);
            PpuMemory = new PpuMemory(this);

            Cpu = new Cpu(this);
            Ppu = new Ppu(this);
        }

        public bool LoadCartridge(string path)
        {
            System.Console.WriteLine("Loading ROM " + path);

            Cartridge = new Cartridge(path);
            if (Cartridge.Invalid) return false;

            // Set mapper
            System.Console.Write("iNES Mapper Number: " + Cartridge.MapperNumber.ToString());
            switch (Cartridge.MapperNumber)
            {
                case 0:
                    System.Console.WriteLine(" (NROM) Supported!");
                    Mapper = new NromMapper(this);
                    break;
                case 1:
                    System.Console.WriteLine(" (MMC1) Supported!");
                    Mapper = new Mmc1Mapper(this);
                    break;
                case 2:
                    System.Console.WriteLine(" (UxROM) Supported!");
                    Mapper = new UxRomMapper(this);
                    break;
                case 4:
                    System.Console.WriteLine(" (MMC3) Supported!");
                    Mapper = new Mmc3Mapper(this);
                    break;
                default:
                    System.Console.WriteLine(" mapper is not supported");
                    return false;
            }

            Cpu.Reset();
            Ppu.Reset();

            CpuMemory.Reset();
            PpuMemory.Reset();

            _frameEvenOdd = false;
            return true;
        }

        public void DrawFrame()
        {
            DrawAction(Ppu.BitmapData);
            _frameEvenOdd = !_frameEvenOdd;
        }

        void GoUntilFrame()
        {
            bool orig = _frameEvenOdd;
            while (orig == _frameEvenOdd)
            {
                int cpuCycles = Cpu.Step();

                // 3 PPU cycles for each CPU cycle
                for (int i = 0; i < cpuCycles * 3; i++)
                {
                    Ppu.Step();
                    Mapper.Step();
                }
            }
        }

        public void Start()
        {
            Stop = false;
            byte[] bitmapData = Ppu.BitmapData;

            while (!Stop)
            {
                Stopwatch frameWatch = Stopwatch.StartNew();
                GoUntilFrame();
                frameWatch.Stop();

                long timeTaken = frameWatch.ElapsedMilliseconds;

                int sleepTime = (int)((1000.0 / 60) - timeTaken);
                Thread.Sleep(sleepTime);
            }
        }
    }    
}
