namespace GameBoyEmulator.Core.Cartridge;

public class NoMBC(byte[] rom) : ICartridge
{
    private readonly byte[] _rom = rom;

    public byte ReadRam(ushort address)
    {
        return 0xFF;
    }

    public byte ReadRom(ushort address)
    {
        return _rom[address];
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
