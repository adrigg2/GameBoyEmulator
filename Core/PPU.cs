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
    private byte[] _screenBuffer;

    private Dispatcher _windowDispatcher; // NOTE: Consider transfering logic to the main window

    public PPU(Dispatcher windowDispatcher)
    {
        _screenImage = new WriteableBitmap(ScreenWidth, ScreenHeigth, 96, 96, PixelFormats.Gray2, null); // TODO: Set pixel format and palette(?) (revise the constructor parameters)
        int stride = (ScreenWidth + 3) / 4;
        int totalBytes = ScreenHeigth * stride;
        byte[] pixels = Enumerable.Repeat((byte)0xFF, totalBytes).ToArray();
        _screenImage.WritePixels(new Int32Rect(0, 0, ScreenWidth, ScreenHeigth), pixels, stride, 0);
        _screenBuffer = new byte[ScreenWidth * ScreenHeigth / 4];
        _windowDispatcher = windowDispatcher;
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

                    RenderScanLine(mmu);
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
                        _windowDispatcher.Invoke(UpdateScreen); // NOTE: Consider transferring logic to the main window
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

    private void RenderScanLine(MMU mmu)
    {
        byte WX = (byte)(mmu.WX - 7);
        byte WY = mmu.WY;
        byte LY = mmu.LY;
        byte LCDC = mmu.LCDC;
        byte SCY = mmu.SCY;
        byte SCX = mmu.SCX;
        byte BGP = mmu.BGP;
        bool isWindow = (LCDC & 0x20) != 0 && LY >= WY;

        byte y = isWindow ? (byte)(LY - WY) : (byte)(LY + SCY);
        byte tileLine = (byte)((y & 7) * 2);

        ushort tileRow = (ushort)(y / 8 * 32);
        ushort tileMapAddress;
        if (isWindow)
        {
            tileMapAddress = (LCDC & 0x40) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
        }
        else
        {
            tileMapAddress = (LCDC & 0x08) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
        }

        byte tileDataLow = 0;
        byte tileDataHigh = 0;
        for (int i = 0; i < ScreenWidth; i++)
        {
            byte x = isWindow && i >= WX ? (byte)(i - WX) : (byte)(i + SCX);
            if ((i & 0x7) == 0 || ((i + SCX) & 0x7) == 0)
            {
                ushort tileCol = (ushort)(x / 8);
                ushort tileIndex = (ushort)(tileMapAddress + tileRow + tileCol);

                ushort tileDataAddress = (LCDC & 0x10) != 0 ? (ushort)0x8000 : (ushort)0x8800;
                ushort tileLoc;
                if ((LCDC & 0x10) != 0)
                {
                    tileLoc = (ushort)(tileDataAddress + (mmu.ReadByte(tileIndex) * 16));
                }
                else
                {
                    tileLoc = (ushort)(tileDataAddress + ((sbyte)mmu.ReadByte(tileIndex) + 128) * 16);
                }

                tileDataLow = mmu.ReadByte((ushort)(tileLoc + tileLine));
                tileDataHigh = mmu.ReadByte((ushort)(tileLoc + tileLine + 1));
            }

            int colorBit = 1 << (7 - (x & 7));
            int colorIdLow = (tileDataLow & colorBit) != 0 ? 1 : 0;
            int colorIdHigh = (tileDataHigh & colorBit) != 0 ? 2 : 0;
            int colorId = colorIdLow + colorIdHigh;
            int color = (BGP >> (colorId * 2)) & 0x3;
            color = ~color & 0x3; // Invert the color bits

            SetPixel(i, LY, color);
        }
    }
    
    private void SetPixel(int x, int y, int color)
    {
        _screenBuffer[(y * ScreenWidth + x) / 4] |= (byte)(color << ((~x & 3) * 2));
    }

    private void UpdateScreen()
    {
        int stride = (ScreenWidth + 3) / 4;
        _screenImage.WritePixels(new Int32Rect(0, 0, ScreenWidth, ScreenHeigth), _screenBuffer, stride, 0);
        Array.Clear(_screenBuffer);
    }
}
