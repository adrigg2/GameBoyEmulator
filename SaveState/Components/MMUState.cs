using GameBoyEmulator.Core.Cartridge;

namespace GameBoyEmulator.SaveState.Components;

public class MMUState(bool bootRomMapped, byte ie, byte IF, byte[] wram, byte[] vram, byte[] hram, byte[] oam)
{
    public bool BootRomMapped { get; } = bootRomMapped;

    public byte IE { get; } = ie;
    public byte IF { get; } = IF;

    public byte[] WRAM { get; } = wram;
    public byte[] VRAM { get; } = vram;
    public byte[] HRAM { get; } = hram;
    public byte[] OAM { get; } = oam;
}
