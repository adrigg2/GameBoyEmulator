namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel1State(byte NR10, byte NR11, byte NR12, byte NR13, byte NR14, int SweepCounter, int CurrentPace,
                            int LengthTimer, int PeriodDiv, int SweepFrequency, int SampleIndex, int Volume,
                            int EnvSweepPace, int EnvSweepCounter, float Output, bool Active, bool DacActive,
                            bool EnvDir, bool SweepEnabled, bool EnvFinished);
