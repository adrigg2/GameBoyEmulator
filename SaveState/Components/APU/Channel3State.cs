using System.IO;

namespace GameBoyEmulator.SaveState.Components.APU;

public record Channel3State(byte NR30, byte NR31, byte NR32, byte NR33, byte NR34, byte[] WaveRam, int LengthTimer,
                            int PeriodDiv, int WaveIndex, int WaveBuffer, float Output, bool Active, bool DacActive)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(NR30);
        writer.Write(NR31);
        writer.Write(NR32);
        writer.Write(NR33);
        writer.Write(NR34);
        writer.Write(WaveRam);
        writer.Write(LengthTimer);
        writer.Write(PeriodDiv);
        writer.Write(WaveIndex);
        writer.Write(WaveBuffer);
        writer.Write(Output);
        writer.Write(Active);
        writer.Write(DacActive);
    }

    public static Channel3State FromBinaryData(BinaryReader reader)
    {
        return new Channel3State(
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadBytes(0x10),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadSingle(),
            reader.ReadBoolean(),
            reader.ReadBoolean()
            );
    }
}
