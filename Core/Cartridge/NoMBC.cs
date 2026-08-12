using GameBoyEmulator.SaveState.Components;

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

    public MBCState SaveState()
    {
        byte[] headerCheck = [.. _rom[0x0134..0x0144], .. _rom[0x014D..0x0150]];
        return new MBCState(0, 0, false, headerCheck, null, null);
    }

    public void LoadState(MBCState state)
    {
        
    }
}
