using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record CPUState(int EiCounter, ushort AF, ushort BC, ushort DE, ushort HL, ushort PC, ushort SP, bool IME, bool Halted, bool HaltBug)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(EiCounter);
        writer.Write(AF);
        writer.Write(BC);
        writer.Write(DE);
        writer.Write(HL);
        writer.Write(PC);
        writer.Write(SP);
        writer.Write(IME);
        writer.Write(Halted);
        writer.Write(HaltBug);
    }

    public static CPUState FromBinaryData(BinaryReader reader)
    {
        return new CPUState(
            reader.ReadInt32(),
            reader.ReadUInt16(),
            reader.ReadUInt16(),
            reader.ReadUInt16(),
            reader.ReadUInt16(),
            reader.ReadUInt16(),
            reader.ReadUInt16(),
            reader.ReadBoolean(),
            reader.ReadBoolean(),
            reader.ReadBoolean()
            );
    }
}
