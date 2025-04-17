using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GameBoyEmulator.Core;

// NOTE: Move mode to MMU?
public class PPU
{
    private const int OAMRead = 2;
    private const int VRAMRead = 3;
    private const int HBlank = 0;
    private const int VBlank = 1;
    private const int OAMReadCycles = 80;
    private const int VRAMReadCycles = 172;
    private const int HBlankCycles = 204;
    private const int ScanlineCycles = 456;
    private const int MaxLines = 153;
    private const int ScreenHeigth = 144;
    private const int ScreenWidth = 160;

    private int _cycleCount;

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
        byte mode = (byte)(mmu.STAT & 0x3);

        switch (mode)
        {
            case OAMRead:
                if (_cycleCount >= OAMReadCycles)
                {
                    _cycleCount -= OAMReadCycles;
                    ChangeMode(VRAMRead, mmu);
                }
                break;
            case VRAMRead:
                if (_cycleCount >= VRAMReadCycles)
                {
                    _cycleCount -= VRAMReadCycles;
                    ChangeMode(HBlank, mmu);

                    // TODO: RENDER
                    WindowDispatcher.Invoke(GeneratePixel); // DEBUG: Test method to generate a pixel
                }
                break;
            case HBlank:
                if (_cycleCount >= HBlankCycles)
                {
                    _cycleCount -= HBlankCycles;
                    mmu.LY++;

                    if (mmu.LY == ScreenHeigth)
                    {
                        ChangeMode(VBlank, mmu);
                        // TODO: RENDER
                        // TODO: VBlank Interrupt
                    }
                    else
                    {
                        ChangeMode(OAMRead, mmu);
                    }
                }
                break;
            case VBlank:
                if (_cycleCount >= ScanlineCycles)
                {
                    _cycleCount -= ScanlineCycles;
                    mmu.LY++;

                    if (mmu.LY > MaxLines)
                    {
                        ChangeMode(OAMRead, mmu);
                        mmu.LY = 0;
                    }
                }
                break;
        }
    }

    private void ChangeMode(int mode, MMU mmu)
    {
        int STAT = mmu.STAT & 0xFC; // Clear the mode bits
        mmu.STAT = (byte)(STAT | mode); // Set the new mode

        // TODO: Interrupts
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
