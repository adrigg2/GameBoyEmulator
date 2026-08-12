using System.IO;

namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel4State(byte NR41, byte NR42, byte NR43, byte NR44, ushort LSFR, int LengthTimer, int Volume,
                            int EnvSweepPace, int EnvSweepCounter, int ChannelFrequency, int ChannelCycles, float Output,
                            bool Active, bool DacActive, bool EnvDir, bool EnvFinished)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(NR41);
        writer.Write(NR42);
        writer.Write(NR43);
        writer.Write(NR44);
        writer.Write(LSFR);
        writer.Write(LengthTimer);
        writer.Write(Volume);
        writer.Write(EnvSweepPace);
        writer.Write(EnvSweepCounter);
        writer.Write(ChannelFrequency);
        writer.Write(ChannelCycles);
        writer.Write(Output);
        writer.Write(Active);
        writer.Write(DacActive);
        writer.Write(EnvDir);
        writer.Write(EnvFinished);
    }

    public static Channel4State FromBinaryData(BinaryReader reader)
    {
        return new Channel4State(
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadUInt16(),
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
