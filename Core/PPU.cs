using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
    private const int ScreenWidth = 160;

    private int _cycleCount;
    private PPUMode _mode;

    private WriteableBitmap _screenImage;

    public Dispatcher WindowDispatcher { get; set; } // NOTE: Consider transferring logic to the main window

    public PPU()
    {
        _screenImage = new WriteableBitmap(ScreenWidth, ScreenHeigth, 96, 96, PixelFormats.Gray2, null); // TODO: Set pixel format and palette(?) (revise the constructor parameters)
    }

    public void SetWindowSource(MainWindow window)
    {
        window.Screen.Source = _screenImage;
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
                    WindowDispatcher.Invoke(GeneratePixel); // DEBUG: Test method to generate a pixel
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

    // DEBUG: Test method to generate a pixel
    // NOTE: Check if it would be better to use a safe method
    private void GeneratePixel()
    {
        Random random = new Random();
        int x = random.Next(0, 160);
        int y = random.Next(0, 144);

        try
        {
            _screenImage.Lock();

            unsafe
            {
                IntPtr pBackBuffer = _screenImage.BackBuffer;

                pBackBuffer += y * _screenImage.BackBufferStride;
                pBackBuffer += x * 4;

                int color_data = 255 << 16; // R
                color_data |= 128 << 8;     // G
                color_data |= 255 << 0;     // B

                *((int*) pBackBuffer) = color_data; // Set pixel color (ARGB format, 32 bits per pixel, 4 bytes per pixel)
            }

            _screenImage.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        }
        finally
        {
            _screenImage.Unlock();
        }
    }
}
