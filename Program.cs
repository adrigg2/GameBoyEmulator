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

while (true)
{
    int cycles = cpu.Execute();
    ppu.Update(cycles, mmu);
    Console.WriteLine(cpu);
    Console.ReadKey();
}
