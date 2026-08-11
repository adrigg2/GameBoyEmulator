namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel4State(byte NR41, byte NR42, byte NR43, byte NR44, ushort LSFR, int LengthTimer, int Volume,
                            int EnvSweepPace, int EnvSweepCounter, int ChannelFrequency, int ChannelCycles, float Output,
                            bool Active, bool DacActive, bool EnvDir, bool EnvFinished);
