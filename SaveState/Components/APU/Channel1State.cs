using System.IO;

namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel1State(byte NR10, byte NR11, byte NR12, byte NR13, byte NR14, int SweepCounter, int CurrentPace,
                            int LengthTimer, int PeriodDiv, int SweepFrequency, int SampleIndex, int Volume,
                            int EnvSweepPace, int EnvSweepCounter, float Output, bool Active, bool DacActive,
                            bool EnvDir, bool SweepEnabled, bool EnvFinished)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(NR10);
        writer.Write(NR11);
        writer.Write(NR12);
        writer.Write(NR13);
        writer.Write(NR14);
        writer.Write(SweepCounter);
        writer.Write(CurrentPace);
        writer.Write(LengthTimer);
        writer.Write(PeriodDiv);
        writer.Write(SweepFrequency);
        writer.Write(SampleIndex);
        writer.Write(Volume);
        writer.Write(EnvSweepPace);
        writer.Write(EnvSweepCounter);
        writer.Write(Output);
        writer.Write(Active);
        writer.Write(DacActive);
        writer.Write(EnvDir);
        writer.Write(SweepEnabled);
        writer.Write(EnvFinished);
    }

    public static Channel1State FromBinaryData(BinaryReader reader)
    {
        return new Channel1State(
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadSingle(),
            reader.ReadBoolean(),
            reader.ReadBoolean(),
            reader.ReadBoolean(),
            reader.ReadBoolean(),
            reader.ReadBoolean()
            );
    }
}
