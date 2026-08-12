using System.IO;

namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel2State(byte NR21, byte NR22, byte NR23, byte NR24, int LengthTimer, int PeriodDiv, int SampleIndex,
                            int Volume, int EnvSweepPace, int EnvSweepCounter, float Output, bool Active, bool DacActive,
                            bool EnvDir, bool EnvFinished)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(NR21);
        writer.Write(NR22);
        writer.Write(NR23);
        writer.Write(NR24);
        writer.Write(LengthTimer);
        writer.Write(PeriodDiv);
        writer.Write(SampleIndex);
        writer.Write(Volume);
        writer.Write(EnvSweepPace);
        writer.Write(EnvSweepCounter);
        writer.Write(Output);
        writer.Write(Active);
        writer.Write(DacActive);
        writer.Write(EnvDir);
        writer.Write(EnvFinished);
    }

    public static Channel2State FromBinaryData(BinaryReader reader)
    {
        return new Channel2State(
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
            reader.ReadSingle(),
            reader.ReadBoolean(),
            reader.ReadBoolean(),
            reader.ReadBoolean(),
            reader.ReadBoolean()
            );
    }
}
