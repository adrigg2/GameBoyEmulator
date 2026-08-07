namespace GameBoyEmulator.SaveState;

public class CPUState(int eiCounter, ushort af, ushort bc, ushort de, ushort hl, ushort pc, ushort sp, bool ime, bool halted, bool haltBug)
{
    public ushort AF { get; } = af;
    public ushort BC { get; } = bc;
    public ushort DE { get; } = de;
    public ushort HL { get; } = hl;
    public ushort PC { get; } = pc;
    public ushort SP { get; } = sp;

    public int EiCounter { get; } = eiCounter;

    public bool IME { get; } = ime;
    public bool Halted { get; } = halted;
    public bool HaltBug { get; } = haltBug;
}
