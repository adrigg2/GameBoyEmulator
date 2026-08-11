namespace GameBoyEmulator.SaveState.Components;

public record DMAState(ushort Address, int Cycles, int Transfers, bool Active);
