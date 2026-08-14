using GameBoyEmulator.SaveState.Components;

namespace GameBoyEmulator.Core.Cartridge;

public interface ICartridge
{
    byte[] HeaderCheck { get; }

    byte ReadRom(ushort address);
    void WriteRegister(ushort address, byte value);
    byte ReadRam(ushort address);
    void WriteRam(ushort address, byte value);
    void SaveRam();
    MBCState SaveState();
    void LoadState(MBCState state);
}
