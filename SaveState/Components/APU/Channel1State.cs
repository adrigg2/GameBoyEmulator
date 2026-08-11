namespace GameBoyEmulator.SaveState.Components.APU;

public class Channel1State(byte nr10, byte nr11, byte nr12, byte nr13, byte nr14, int sweepCounter, int currentPace, int lengthTimer, int periodDiv, int sweepFrequency, int sampleIndex, int volume, int envSweepPace, int envSweepCounter, float output, bool active, bool dacActive, bool envDir, bool sweepEnabled, bool envFinished)
{
    public byte NR10 { get; } = nr10;
    public byte NR11 { get; } = nr11;
    public byte NR12 { get; } = nr12;
    public byte NR13 { get; } = nr13;
    public byte NR14 { get; } = nr14;

    public int SweepCounter { get; } = sweepCounter;
    public int CurrentPace { get; } = currentPace;
    public int LengthTimer { get; } = lengthTimer;
    public int PeriodDiv { get; } = periodDiv;
    public int SweepFrequency { get; } = sweepFrequency;
    public int SampleIndex { get; } = sampleIndex;
    public int Volume { get; } = volume;
    public int EnvSweepPace { get; } = envSweepPace;
    public int EnvSweepCounter { get; } = envSweepCounter;

    public float Output { get; } = output;

    public bool Active { get; } = active;
    public bool DacActive { get; } = dacActive;
    public bool EnvDir { get; } = envDir;
    public bool SweepEnabled { get; } = sweepEnabled;
    public bool EnvFinished { get; } = envFinished;
}
