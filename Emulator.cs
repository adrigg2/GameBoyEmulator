using GameBoyEmulator.Core;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace GameBoyEmulator;
public class Emulator
{
    private MMU _mmu;
    private CPU _cpu;
    private PPU _ppu;
    int _calls;

    public PPU PPU { get => _ppu; }

    public Emulator(string rom, string bootRom)
    {
        byte[] romBytes = File.ReadAllBytes(rom);
        byte[] bootRomBytes = File.ReadAllBytes(bootRom);
        _mmu = new();
        _mmu.LoadGame(romBytes);
        _mmu.LoadBootRom(bootRomBytes);
        _cpu = new(_mmu);
        _ppu = new();
    }

    public void Tick()
    {
        _calls++;

        int cycles = _cpu.Execute();
        _ppu.Update(cycles, _mmu);
        Console.WriteLine(_cpu);
        Console.WriteLine($"LY = {_mmu.LY}");
        Console.WriteLine($"LY(CPU) = {_mmu.ReadByte(0xFF44)}");
        Console.WriteLine($"Calls = {_calls}");
        //if (mmu.LY == 144 || (cpu._lastInstruction != 0x20 && cpu._lastInstruction != 0xFE && cpu._lastInstruction != 0xF0) )
        //Console.ReadKey();
    }
}
