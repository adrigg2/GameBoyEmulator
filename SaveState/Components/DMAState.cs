using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record DMAState(ushort Address, int Cycles, int Transfers, bool Active)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(Address);
        writer.Write(Cycles);
        writer.Write(Transfers);
        writer.Write(Active);
    }

    public static DMAState FromBinaryData(BinaryReader reader)
    {
        return new DMAState(
            reader.ReadUInt16(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadBoolean()
            );
    }
}
