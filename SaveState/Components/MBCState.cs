namespace GameBoyEmulator.SaveState.Components;

public record MBCState(int ROMBank, int SRAMBank, bool RAMEnabled, byte[] HeaderCheck, byte[]? AdditionalRegisters, byte[]? SRAM);
