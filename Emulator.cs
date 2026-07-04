using GameBoyEmulator.Core;
using GameBoyEmulator.Debug;
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
    private TIMER _timer;

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
        _ppu = new(windowDispatcher);
        _joypad = new();
        _timer = new();
        _dma = new();
        _mmu = new(_dma, _joypad, _ppu, _timer);
        _mmu.LoadGame(romBytes);
        _mmu.LoadBootRom(bootRomBytes);
        _cpu = new(_mmu);
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
            _dma.Tick(cycles, _mmu);
            _joypad.Update(_mmu);
            _timer.Tick(cycles, _mmu);
            frameCycles += cycles;

            //if (!_mmu._bootRomMapped)
            //{
            //    logFile?.WriteLine($"{_cpu._lastInstructionPC:X2}: {OpcodeParser.ParseOpcode(_cpu._lastInstruction)}");
            //}

            //if (_cpu._lastInstructionPC == 0xFE)
            //{
            //    logFile?.WriteLine(_cpu);
            //}

            if (_cpu.interrupt)
            {
                logFile?.WriteLine(_cpu.interruptSource);
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
