using GameBoyEmulator.Core;
using System.IO;

namespace GameBoyEmulator;
public class Emulator
{
    private string _rom;
    private string _bootRom;

    public Emulator(string rom, string bootRom)
    {
        _rom = rom;
        _bootRom = bootRom;
    }

    public void Start(MainWindow window)
    {
        byte[] rom = File.ReadAllBytes(_rom);
        byte[] bootRom = File.ReadAllBytes(_bootRom);
        MMU mmu = new();
        mmu.LoadGame(rom);
        mmu.LoadBootRom(bootRom);
        CPU cpu = new(mmu);
        PPU ppu = new(window);
        int calls = 0; // DEBUG: debug only

        while (mmu._inBios)
        {
            calls++;

            int cycles = cpu.Execute();
            ppu.Update(cycles, mmu);
            Console.WriteLine(cpu);
            Console.WriteLine($"LY = {mmu.LY}");
            Console.WriteLine($"LY(CPU) = {mmu.ReadByte(0xFF44)}");
            Console.WriteLine($"Calls = {calls}");
            //if (mmu.LY == 144 || (cpu._lastInstruction != 0x20 && cpu._lastInstruction != 0xFE && cpu._lastInstruction != 0xF0) )
            //Console.ReadKey();
        }
    }
}
