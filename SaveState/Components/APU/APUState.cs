namespace GameBoyEmulator.SaveState.Components.APU;

public record APUState(byte NR50, byte NR51, byte NR52, int OldDivApu, float SampleCycles, float CapacitorL,
                       float CapacitorR, bool Active, Channel1State Channel1, Channel2State Channel2,
                       Channel3State Channel3, Channel4State Channel4);
