using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record TIMERState(int TIMAResetCounter, int TIMAIgnoreWritesCounter, int DivAPU, ushort Counter, byte TIMA, byte TMA, byte TAC)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(TIMAResetCounter);
        writer.Write(TIMAIgnoreWritesCounter);
        writer.Write(DivAPU);
        writer.Write(Counter);
        writer.Write(TIMA);
        writer.Write(TMA);
        writer.Write(TAC);
    }

    public static TIMERState FromBinaryData(BinaryReader reader)
    {
        return new TIMERState(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadUInt16(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte()
            );
    }
}
