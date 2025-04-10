namespace GameBoyEmulator.Core;
public class MMU
{
    public byte IE { get; set; } // TODO: Implement IE
    public byte IF { get; set; } // TODO: Implement IF

    public byte ReadByte(ushort address)
    {
        throw new NotImplementedException("MMU is not implemented yet");
    }

    public ushort ReadWord(ushort address)
    {
        throw new NotImplementedException("MMU is not implemented yet");
    }

    public void WriteByte(ushort address, byte value)
    {
        throw new NotImplementedException("MMU is not implemented yet");
    }

    public void WriteWord(ushort address, ushort value)
    {
        throw new NotImplementedException("MMU is not implemented yet");
    }
}
