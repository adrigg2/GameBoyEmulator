using GameBoyEmulator.Core;
using GameBoyEmulator.Core.Audio;
using System.IO;
using System.Windows.Threading;

namespace GameBoyEmulator;

public class Emulator
{
    private readonly MMU _mmu;
    private readonly CPU _cpu;
    private readonly PPU _ppu;
    private readonly DMA _dma;
    private readonly JOYPAD _joypad;
    private readonly TIMER _timer;
    private readonly APU _apu;

    private const int CPUFrequency = 4194304; // 4.194304 MHz
    private const int CyclesPerFrame = 70224; // ~60 FPS
    public const float FrameTime = (float)CyclesPerFrame / CPUFrequency;

    public PPU PPU { get => _ppu; }
    public JOYPAD JOYPAD { get => _joypad; }
    public APU APU { get => _apu; }
    public MMU MMU { get => _mmu; }

    public Emulator(string rom, string bootRom, Dispatcher windowDispatcher)
    {
        byte[] romBytes = File.ReadAllBytes(rom);
        byte[] bootRomBytes = File.ReadAllBytes(bootRom);

        string romName = Path.GetFileNameWithoutExtension(rom);

        _ppu = new(windowDispatcher);
        _joypad = new();
        _timer = new();
        _dma = new();
        _apu = new();
        _mmu = new(_dma, _joypad, _ppu, _timer, _apu);
        _mmu.LoadGame(romBytes, romName);
        _mmu.LoadBootRom(bootRomBytes);
        _cpu = new(_mmu);
    }

    public void ProcessFrame()
    {
        int frameCycles = 0;

        while (frameCycles < CyclesPerFrame)
        {
            //_calls++;
            int cycles = _cpu.Execute();
            _ppu.Update(cycles, _mmu);
            _dma.Tick(cycles, _mmu);
            _joypad.Update(_mmu);
            int divApuCounter = _timer.Tick(cycles, _mmu);
            _apu.Tick(cycles, divApuCounter);
            frameCycles += cycles;
        }
    }

    public SaveState.SaveState SaveState()
    {
        return new SaveState.SaveState(
            _cpu.SaveState(),
            _dma.SaveState(),
            _joypad.SaveState(),
            _mmu.SaveState(),
            _ppu.SaveState(),
            _timer.SaveState(),
            _apu.SaveState()
            );
    }

    public void LoadState(SaveState.SaveState state)
    {
        _cpu.LoadState(state.CPU);
        _dma.LoadState(state.DMA);
        _joypad.LoadState(state.JOYPAD);
        _mmu.LoadState(state.MMU);
        _ppu.LoadState(state.PPU);
        _timer.LoadState(state.TIMER);
        _apu.LoadState(state.APU);
    }
}
