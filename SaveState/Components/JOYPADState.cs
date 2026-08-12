using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record JOYPADState(byte JOYP)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(JOYP);
    }

    public static JOYPADState FromBinaryData(BinaryReader reader)
    {
        return new JOYPADState(reader.ReadByte());
    }
}
