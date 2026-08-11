namespace GameBoyEmulator.SaveState.Components;

public record CPUState(int EiCounter, ushort AF, ushort BC, ushort DE, ushort HL, ushort PC, ushort SP, bool IME, bool Halted, bool HaltBug);
