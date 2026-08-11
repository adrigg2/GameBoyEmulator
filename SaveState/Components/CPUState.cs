using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record CPUState(int EiCounter, ushort AF, ushort BC, ushort DE, ushort HL, ushort PC, ushort SP, bool IME, bool Halted, bool HaltBug)
{
    public byte[] Serialize()
    {
        return [
            .. BitConverter.GetBytes(EiCounter),
            .. BitConverter.GetBytes(AF),
            .. BitConverter.GetBytes(BC),
            .. BitConverter.GetBytes(HL),
            .. BitConverter.GetBytes(PC),
            .. BitConverter.GetBytes(SP),
            .. BitConverter.GetBytes(IME),
            .. BitConverter.GetBytes(Halted),
            .. BitConverter.GetBytes(HaltBug),
            ];
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
