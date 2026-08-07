namespace GameBoyEmulator.SaveState;

public class DMAState(ushort address, int cycles, int transfers, bool active)
{
    public ushort Address { get; } = address;

    public int Cycles { get; } = cycles;
    public int Transfers { get; } = transfers;

    public bool Active { get; } = active;
}
