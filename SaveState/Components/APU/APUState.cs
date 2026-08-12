using System.IO;

namespace GameBoyEmulator.SaveState.Components.APU;

public record APUState(byte NR50, byte NR51, byte NR52, int OldDivApu, float SampleCycles, float CapacitorL,
                       float CapacitorR, bool Active, Channel1State Channel1, Channel2State Channel2,
                       Channel3State Channel3, Channel4State Channel4)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(NR50);
        writer.Write(NR51);
        writer.Write(NR52);
        writer.Write(OldDivApu);
        writer.Write(SampleCycles);
        writer.Write(CapacitorL);
        writer.Write(CapacitorR);
        writer.Write(Active);
        Channel1.Write(writer);
        Channel2.Write(writer);
        Channel3.Write(writer);
        Channel4.Write(writer);
    }

    public static APUState FromBinaryData(BinaryReader reader)
    {
        return new APUState(
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadInt32(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadBoolean(),
            Channel1State.FromBinaryData(reader),
            Channel2State.FromBinaryData(reader),
            Channel3State.FromBinaryData(reader),
            Channel4State.FromBinaryData(reader)
            );
    }
}
