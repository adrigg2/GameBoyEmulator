namespace GameBoyEmulator.SaveState.Components;

public record TIMERState(int TIMAResetCounter, int TIMAIgnoreWritesCounter, int DivAPU, ushort Counter, byte TIMA, byte TMA, byte TAC);
