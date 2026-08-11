namespace GameBoyEmulator.SaveState.Components;

public record MMUState(bool BootRomMapped, byte IE, byte IF, byte[] WRAM, byte[] VRAM, byte[] HRAM, byte[] OAM, MBCState Cartridge);
