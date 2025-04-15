using GameBoyEmulator.Core;

if (args.Length < 1)
{
    Console.WriteLine("A ROM filepath should be given as a parameter");
    return;
}

byte[] rom = File.ReadAllBytes(args[0]);
MMU mmu = new();
mmu.LoadGame(rom);
CPU cpu = new(mmu);
PPU ppu = new();
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
