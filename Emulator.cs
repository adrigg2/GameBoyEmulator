using GameBoyEmulator.Core;
using System.IO;
using System.Windows.Threading;

namespace GameBoyEmulator;
public class Emulator
{
    private MMU _mmu;
    private CPU _cpu;
    private PPU _ppu;
    private DMA _dma;
    private JOYPAD _joypad;

    private const int CPUFrequency = 4194304; // 4.194304 MHz
    private const int CyclesPerFrame = 70224; // ~60 FPS
    public const float FrameTime = (float)CyclesPerFrame / CPUFrequency;
    private int _frames;

    public PPU PPU { get => _ppu; }
    public JOYPAD JOYPAD { get => _joypad; }

    public Emulator(string rom, string bootRom, Dispatcher windowDispatcher)
    {
        byte[] romBytes = File.ReadAllBytes(rom);
        byte[] bootRomBytes = File.ReadAllBytes(bootRom);
        _mmu = new();
        _mmu.LoadGame(romBytes);
        _mmu.LoadBootRom(bootRomBytes);
        _dma = _mmu.DMA;
        _cpu = new(_mmu);
        _ppu = new(windowDispatcher);
        _joypad = new();
        _frames = 0;
    }

    public void ProcessFrame(StreamWriter? logFile = null)
    {
        int frameCycles = 0;

        while (frameCycles < CyclesPerFrame)
        {
            //_calls++;
            int cycles = _cpu.Execute();
            _ppu.Update(cycles, _mmu);
            _dma.Tick(cycles);
            _joypad.Update(_mmu);
            frameCycles += cycles;

            //logFile?.WriteLine($"{_cpu._lastInstructionPC:X2}: {_cpu._lastInstruction:X2}");

            if (_cpu._lastInstructionPC == 0xFE)
            {
                logFile?.WriteLine(_cpu);
            }

            //Console.WriteLine(_cpu);
            //Console.WriteLine($"LY = {_mmu.LY}");
            //Console.WriteLine($"LY(CPU) = {_mmu.ReadByte(0xFF44)}");
            //Console.WriteLine($"Calls = {_calls}");
            //if (mmu.LY == 144 || (cpu._lastInstruction != 0x20 && cpu._lastInstruction != 0xFE && cpu._lastInstruction != 0xF0) )
            //Console.ReadKey();
            //using (StreamWriter log = new StreamWriter(".\\logs\\log.txt", true))
            //{
            //    log.WriteLine($"{_cpu._lastInstruction:X2}");
            //}
        }

        _frames++;
    }
}
