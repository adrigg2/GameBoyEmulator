namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel3State(byte NR30, byte NR31, byte NR32, byte NR33, byte NR34, byte[] WaveRam, int LengthTimer,
                            int PeriodDiv, int WaveIndex, int WaveBuffer, float Output, bool Active, bool DacActive);
