using GameBoyEmulator.SaveState.Components;

namespace GameBoyEmulator.Core.Cartridge;

public class NoCartridge : ICartridge
{
    public byte[] HeaderCheck => [];

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

    public MBCState SaveState()
    {
        return new MBCState(0, 0, false, [], null, null);
    }

    public void LoadState(MBCState state)
    {
        
    }
}
