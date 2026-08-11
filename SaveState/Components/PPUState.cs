namespace GameBoyEmulator.SaveState.Components;

public record PPUState(int CycleCount, int WindowY, byte LCDC, byte STAT, byte SCY, byte SCX, byte LY, byte LYC,
                       byte BGP, byte OBP0, byte OBP1, byte WY, byte WX, bool STATInterruptRequest, bool ScreenOff,
                       byte[] ScreenBuffer, byte[] BgColorIds, List<ushort> ObjectPool);
