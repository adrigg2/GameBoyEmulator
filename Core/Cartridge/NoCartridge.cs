namespace GameBoyEmulator.Core.Cartridge;

public class NoCartridge : ICartridge
{
    public byte ReadRam(ushort address)
    {
        return 0xFF;
    }

    public byte ReadRom(ushort address)
    {
        return 0xFF;
    }

    public void SaveRam()
    {
        
    }

    public void WriteRam(ushort address, byte value)
    {
        
    }

    public void WriteRegister(ushort address, byte value)
    {
        
    }
}
