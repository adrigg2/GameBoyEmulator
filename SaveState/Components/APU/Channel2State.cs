namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel2State(byte NR21, byte NR22, byte NR23, byte NR24, int LengthTimer, int PeriodDiv, int SampleIndex,
                            int Volume, int EnvSweepPace, int EnvSweepCounter, float Output, bool Active, bool DacActive,
                            bool EnvDir, bool EnvFinished);