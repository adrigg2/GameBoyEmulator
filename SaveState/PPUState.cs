namespace GameBoyEmulator.SaveState;

public class PPUState(int cycleCount, int windowY, byte lcdc, byte stat, byte scy, byte scx, byte ly, byte lyc, byte bgp, byte obp0, byte obp1, byte wy, byte wx, bool statInterruptRequest, bool screenOff, byte[] screenBuffer, byte[] bgColorIds, List<ushort> objectPool)
{
    public int CycleCount { get; } = cycleCount;
    public int WindowY { get; } = windowY;

    public byte LCDC { get; } = lcdc;
    public byte STAT { get; } = stat;
    public byte SCY { get; } = scy;
    public byte SCX { get; } = scx;
    public byte LY { get; } = ly;
    public byte LYC { get; } = lyc;
    public byte BGP { get; } = bgp;
    public byte OBP0 { get; } = obp0;
    public byte OBP1 { get; } = obp1;
    public byte WY { get; } = wy;
    public byte WX { get; } = wx;

    public bool STATInterruptRequest { get; } = statInterruptRequest;
    public bool ScreenOff { get; } = screenOff;

    public byte[] ScreenBuffer { get; } = screenBuffer;
    public byte[] BgColorIds { get; } = bgColorIds;

    public List<ushort> ObjectPool { get; } = objectPool;
}
