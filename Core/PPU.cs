using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xaml;

namespace GameBoyEmulator.Core;

// NOTE: Change pixel format to Bgra32, use a palette to translate the pixels and a buffer array where each value is a pixel OR use Indexed2 and a color palette
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

    private bool _STATInterruptRequest;
    private bool _screenOff;

    private WriteableBitmap _screenImage;
    private byte[] _screenBuffer;
    private byte[] _bgColorIds;

    private List<ushort> _objectPool;

    private Dispatcher _windowDispatcher; // NOTE: Consider transfering logic to the main window

    public PPU(Dispatcher windowDispatcher)
    {
        _screenImage = new WriteableBitmap(ScreenWidth, ScreenHeigth, 96, 96, PixelFormats.Gray2, null);
        int stride = (ScreenWidth + 3) / 4;
        int totalBytes = ScreenHeigth * stride;
        byte[] pixels = Enumerable.Repeat((byte)0xFF, totalBytes).ToArray();
        _screenImage.WritePixels(new Int32Rect(0, 0, ScreenWidth, ScreenHeigth), pixels, stride, 0);
        _screenBuffer = new byte[ScreenWidth * ScreenHeigth / 4];
        _bgColorIds = new byte[ScreenWidth];
        _windowDispatcher = windowDispatcher;
        _objectPool = [];
    }

    public void SetWindowSource(MainWindow window)
    {
        window.Screen.Source = _screenImage;
    }

    public void Update(int cycles, MMU mmu)
    {
        if ((mmu.LCDC & 0x80) == 0)
        {
            mmu.LY = 0;
            mmu.STAT = (byte)(mmu.STAT & ~0x3);
            _cycleCount = 0;
            
            if (!_screenOff)
            {
                _screenOff = true;
                Array.Fill<byte>(_screenBuffer, 0xFF);
            }

            return;
        }

        _screenOff = false;

        _cycleCount += cycles;
        byte mode = (byte)(mmu.STAT & 0x3);

        switch (mode)
        {
            case OAMRead:
                if (_cycleCount >= OAMReadCycles)
                {
                    _objectPool.Clear();
                    for (ushort i = 0xFE00; i < 0xFEA0 && _objectPool.Count < 10; i += 4)
                    {
                        int y = mmu.ReadByte(i) - 16;
                        int size = (mmu.LCDC & 0x4) != 0 ? 16 : 8;
                        if (y <= mmu.LY && y + size > mmu.LY)
                        {
                            _objectPool.Add(i);
                        }
                    }

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
                        mmu.IF |= 0x1;
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

        if (mmu.LY == mmu.LYC)
        {
            mmu.STAT |= 0x4;
        }
        else
        {
            mmu.STAT = (byte)(mmu.STAT & ~0x4);
        }

        STATInterrupt(mmu);
    }

    private void ChangeMode(int mode, MMU mmu)
    {
        int STAT = mmu.STAT & 0xFC; // Clear the mode bits
        mmu.STAT = (byte)(STAT | mode); // Set the new mode

        // TODO: Interrupts
    }

    private void STATInterrupt(MMU mmu)
    {
        bool previousSTATInterruptRequest = _STATInterruptRequest;
        byte STAT = mmu.STAT;

        if ((STAT & 0x40) != 0 && (STAT & 0x4) != 0)
        {
            _STATInterruptRequest = true;
        }
        else if ((STAT & 0x20) != 0 && (STAT & 0x3) == OAMRead)
        {
            _STATInterruptRequest = true;
        }
        else if ((STAT & 0x10) != 0 && (STAT & 0x3) == VBlank)
        {
            _STATInterruptRequest = true;
        }
        else if ((STAT & 0x8) != 0 && (STAT & 0x3) == HBlank)
        {
            _STATInterruptRequest = true;
        }
        else
        {
            _STATInterruptRequest = false;
        }

        if (!previousSTATInterruptRequest && _STATInterruptRequest)
        {
            mmu.IF |= 0x2;
        }
    }

    private void RenderScanLine(MMU mmu)
    {
        byte LCDC = mmu.LCDC;
        if ((LCDC & 0x1) != 0)
        {
            RenderBG(mmu);
        }

        if ((LCDC & 0x2) != 0)
        {
            RenderObjects(mmu);
        }
    }

    private void RenderBG(MMU mmu)
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
            _bgColorIds[i] = (byte)colorId;
        }
    }

    private void RenderObjects(MMU mmu)
    {
        byte LY = mmu.LY;
        bool doubleSize = (mmu.LCDC & 0x4) != 0;
        for (int i = 0; i < ScreenWidth; i++)
        {
            ushort objectAddress = 0;
            int x = 0;
            foreach (ushort address in _objectPool)
            {
                x = mmu.ReadByte((ushort)(address + 1)) - 8;
                if (i >= x && i < x + 8)
                {
                    objectAddress = address;
                    break;
                }
            }

            if (objectAddress == 0)
            {
                continue;
            }

            int y = mmu.ReadByte(objectAddress) - 16;

            byte tile = mmu.ReadByte((ushort)(objectAddress + 2));
            if (doubleSize)
            {
                tile = LY < y + 8 ? (byte)(tile & 0xFE) : (byte)(tile | 0x1);
            }
            byte attributes = mmu.ReadByte((ushort)(objectAddress + 3));
            int priority = attributes & 0x80;
            if (priority != 0 && _bgColorIds[i] != 0)
            {
                continue;
            }

            int yFlip = attributes & 0x40;
            int xFlip = attributes & 0x20;
            int palette = attributes & 0x10;

            ushort tileAddress = (ushort)(tile * 16 + 0x8000);

            int addressShift = yFlip > 0 ? (~(LY - y)) & 0x7 : (LY - y);
            ushort tileRowAddress = (ushort)(tileAddress + addressShift * 2);

            byte tileLow = mmu.ReadByte(tileRowAddress);
            byte tileHigh = mmu.ReadByte((ushort)(tileRowAddress + 1));

            int colorBit = xFlip > 0 ? 1 << (7 - (~(i - x) & 7)) : 1 << (7 - ((i - x) & 7));
            int colorIdLow = (tileLow & colorBit) != 0 ? 1 : 0;
            int colorIdHigh = (tileHigh & colorBit) != 0 ? 2 : 0;
            int colorId = colorIdLow + colorIdHigh;

            if (colorId != 0)
            {
                ushort paletteAddress = palette > 0 ? (ushort)0xFF49 : (ushort)0xFF48;
                byte OBP = mmu.ReadByte(paletteAddress);
                int color = (OBP >> (colorId * 2)) & 0x3;
                color = ~color & 0x3; // Invert the color bits

                SetObjectPixel(i, LY, color);
            }
        }
    }
    
    private void SetPixel(int x, int y, int color)
    {
        _screenBuffer[(y * ScreenWidth + x) / 4] |= (byte)(color << ((~x & 3) * 2));
    }

    private void SetObjectPixel(int x, int y, int color)
    {
        int colorShift = (~x & 3) * 2;
        int index = (y * ScreenWidth + x) / 4;

        _screenBuffer[index] &= (byte)~(0x3 << colorShift);
        _screenBuffer[index] |= (byte)(color << colorShift);
    }

    private void UpdateScreen()
    {
        int stride = (ScreenWidth + 3) / 4;
        _screenImage.WritePixels(new Int32Rect(0, 0, ScreenWidth, ScreenHeigth), _screenBuffer, stride, 0);
        Array.Clear(_screenBuffer);
    }
}
