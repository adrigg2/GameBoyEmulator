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

while (true)
{
    cpu.Execute();
    Console.WriteLine(cpu);
    Console.ReadKey();
}
