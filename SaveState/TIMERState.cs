namespace GameBoyEmulator.SaveState;

public class TIMERState(int timaResetCounter, int timaIgnoreWritesCounter, int divApu, ushort counter, byte tima, byte tma, byte tac)
{
    public int TIMAResetCounter { get; } = timaResetCounter;
    public int TIMAIgnoreWritesCounter { get; } = timaIgnoreWritesCounter;
    public int DivAPU { get; } = divApu;

    public ushort Counter { get; } = counter;

    public byte TIMA { get; } = tima;
    public byte TMA { get; } = tma;
    public byte TAC { get; } = tac;
}
