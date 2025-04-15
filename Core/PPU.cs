namespace GameBoyEmulator.Core;

// NOTE: Move mode to MMU?
public class PPU
{
    private enum PPUMode
    {
        OAMRead = 2,
        VRAMRead = 3,
        HBlank = 0,
        VBlank = 1,
    }

    private const int OAMReadCycles = 80;
    private const int VRAMReadCycles = 172;
    private const int HBlankCycles = 204;
    private const int ScanlineCycles = 456;
    private const int MaxLines = 153;
    private const int ScreenHeigth = 144;

    private int _cycleCount;
    private PPUMode _mode;

    private MainWindow _window;

    public PPU(MainWindow window)
    {
        _window = window;
    }

    public void Update(int cycles, MMU mmu)
    {
        _cycleCount += cycles;

        switch (_mode)
        {
            case PPUMode.OAMRead:
                if (_cycleCount >= OAMReadCycles)
                {
                    _cycleCount -= OAMReadCycles;
                    _mode = PPUMode.VRAMRead;
                }
                break;
            case PPUMode.VRAMRead:
                if (_cycleCount >= VRAMReadCycles)
                {
                    _cycleCount -= VRAMReadCycles;
                    _mode = PPUMode.HBlank;

                    // TODO: RENDER
                }
                break;
            case PPUMode.HBlank:
                if (_cycleCount >= HBlankCycles)
                {
                    _cycleCount -= HBlankCycles;
                    mmu.LY++;

                    if (mmu.LY == ScreenHeigth)
                    {
                        _mode = PPUMode.VBlank;
                        // TODO: RENDER
                        // TODO: VBlank Interrupt
                    }
                    else
                    {
                        _mode = PPUMode.OAMRead;
                    }
                }
                break;
            case PPUMode.VBlank:
                if (_cycleCount >= ScanlineCycles)
                {
                    _cycleCount -= ScanlineCycles;
                    mmu.LY++;

                    if (mmu.LY > MaxLines)
                    {
                        _mode = PPUMode.OAMRead;
                        mmu.LY = 0;
                    }
                }
                break;
        }
    }
}
