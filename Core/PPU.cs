using System.Drawing;
using System.IO;
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

    private byte _lcdc;
    private byte _stat;
    private byte _scy;
    private byte _scx;
    private byte _ly;
    private byte _lyc;
    private byte _bgp;
    private byte _obp0;
    private byte _obp1;
    private byte _wy;
    private byte _wx;

    private bool _STATInterruptRequest;
    private bool _screenOff;

    private WriteableBitmap _screenImage;
    private byte[] _screenBuffer;
    private byte[] _bgColorIds;

    private List<ushort> _objectPool;

    private Dispatcher _windowDispatcher; // NOTE: Consider transfering logic to the main window

    public byte LCDC { get => _lcdc; set => _lcdc = value; }
    public byte STAT { get => _stat; set => _stat = (byte)((value & 0xF8) | (_stat & 0x07)); }
    public byte SCY { get => _scy; set => _scy = value; }
    public byte SCX { get => _scx; set => _scx = value; }
    public byte LY { get => _ly; }
    public byte LYC { get => _lyc; set => _lyc = value; }
    public byte BGP { get => _bgp; set => _bgp = value; }
    public byte OBP0 { get => _obp0; set => _obp0 = value; }
    public byte OBP1 { get => _obp1; set => _obp1 = value; }
    public byte WY { get => _wy; set => _wy = value; }
    public byte WX { get => _wx; set => _wx = value; }

    private int frames = 0; // DEBUG

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
        if ((_lcdc & 0x80) == 0)
        {
            _ly = 0;
            _stat = (byte)(_stat & ~0x3);
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
        byte mode = (byte)(_stat & 0x3);

        switch (mode)
        {
            case OAMRead:
                if (_cycleCount >= OAMReadCycles)
                {
                    _objectPool.Clear();
                    for (ushort i = 0xFE00; i < 0xFEA0 && _objectPool.Count < 10; i += 4)
                    {
                        int y = mmu.ReadByte(i) - 16;
                        int size = (_lcdc & 0x4) != 0 ? 16 : 8;
                        if (y <= _ly && y + size > _ly)
                        {
                            _objectPool.Add(i);
                        }
                    }

                    _cycleCount -= OAMReadCycles;
                    ChangeMode(VRAMRead);
                }
                break;
            case VRAMRead:
                if (_cycleCount >= VRAMReadCycles)
                {
                    _cycleCount -= VRAMReadCycles;
                    ChangeMode(HBlank);

                    RenderScanLine(mmu);
                }
                break;
            case HBlank:
                if (_cycleCount >= HBlankCycles)
                {
                    _cycleCount -= HBlankCycles;
                    _ly++;

                    if (_ly == ScreenHeigth)
                    {
                        ChangeMode(VBlank);
                        _windowDispatcher.Invoke(UpdateScreen); // NOTE: Consider transferring logic to the main window
                        mmu.IF |= 0x1;
                    }
                    else
                    {
                        ChangeMode(OAMRead);
                    }
                }
                break;
            case VBlank:
                if (_cycleCount >= ScanlineCycles)
                {
                    _cycleCount -= ScanlineCycles;
                    _ly++;

                    if (_ly > MaxLines)
                    {
                        ChangeMode(OAMRead);
                        _ly = 0;
                    }
                }
                break;
        }

        if (_ly == _lyc)
        {
            _stat |= 0x4;
        }
        else
        {
            _stat = (byte)(_stat & ~0x4);
        }

        STATInterrupt(mmu);
    }

    private void ChangeMode(int mode)
    {
        int STAT = _stat & 0xFC; // Clear the mode bits
        _stat = (byte)(STAT | mode); // Set the new mode
    }

    private void STATInterrupt(MMU mmu)
    {
        bool previousSTATInterruptRequest = _STATInterruptRequest;

        if ((_stat & 0x40) != 0 && (_stat & 0x4) != 0)
        {
            _STATInterruptRequest = true;
        }
        else if ((_stat & 0x20) != 0 && (_stat & 0x3) == OAMRead)
        {
            _STATInterruptRequest = true;
        }
        else if ((_stat & 0x10) != 0 && (_stat & 0x3) == VBlank)
        {
            _STATInterruptRequest = true;
        }
        else if ((_stat & 0x8) != 0 && (_stat & 0x3) == HBlank)
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
        if ((_lcdc & 0x1) != 0)
        {
            RenderBG(mmu);
        }
        else
        {
            for (int i = 0; i < ScreenWidth; i++)
            {
                SetPixel(i, _ly, 0xFF);
            }
        }

        if ((_lcdc & 0x2) != 0)
        {
            RenderObjects(mmu);
        }
    }

    private void RenderBG(MMU mmu)
    {
        byte WX = (byte)(_wx - 7);
        bool isWindow = (_lcdc & 0x20) != 0 && _ly >= _wy;

        byte y = isWindow ? (byte)(_ly - _wy) : (byte)(_ly + _scy);
        byte tileLine = (byte)((y & 7) * 2);

        ushort tileRow = (ushort)(y / 8 * 32);
        ushort tileMapAddress;
        if (isWindow)
        {
            tileMapAddress = (_lcdc & 0x40) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
        }
        else
        {
            tileMapAddress = (_lcdc & 0x08) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
        }

        byte tileDataLow = 0;
        byte tileDataHigh = 0;
        for (int i = 0; i < ScreenWidth; i++)
        {
            byte x = isWindow && i >= WX ? (byte)(i - WX) : (byte)(i + _scx);
            if ((i & 0x7) == 0 || ((i + _scx) & 0x7) == 0)
            {
                ushort tileCol = (ushort)(x / 8);
                ushort tileIndex = (ushort)(tileMapAddress + tileRow + tileCol);

                ushort tileDataAddress = (_lcdc & 0x10) != 0 ? (ushort)0x8000 : (ushort)0x8800;
                ushort tileLoc;
                if ((_lcdc & 0x10) != 0)
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
            int color = (_bgp >> (colorId * 2)) & 0x3;
            color = ~color & 0x3; // Invert the color bits

            SetPixel(i, _ly, color);
            _bgColorIds[i] = (byte)colorId;
        }
    }

    private void RenderObjects(MMU mmu)
    {
        bool doubleSize = (_lcdc & 0x4) != 0;
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
                tile = _ly < y + 8 ? (byte)(tile & 0xFE) : (byte)(tile | 0x1);
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

            int addressShift = yFlip > 0 ? (~(_ly - y)) & 0x7 : (_ly - y);
            ushort tileRowAddress = (ushort)(tileAddress + addressShift * 2);

            byte tileLow = mmu.ReadByte(tileRowAddress);
            byte tileHigh = mmu.ReadByte((ushort)(tileRowAddress + 1));

            int colorBit = xFlip > 0 ? 1 << (7 - (~(i - x) & 7)) : 1 << (7 - ((i - x) & 7));
            int colorIdLow = (tileLow & colorBit) != 0 ? 1 : 0;
            int colorIdHigh = (tileHigh & colorBit) != 0 ? 2 : 0;
            int colorId = colorIdLow + colorIdHigh;

            if (colorId != 0)
            {
                byte OBP = palette > 0 ? _obp1 : _obp0;
                int color = (OBP >> (colorId * 2)) & 0x3;
                color = ~color & 0x3; // Invert the color bits

                SetObjectPixel(i, _ly, color);
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

        using FileStream stream = new($"./frames/frame{frames}.png", FileMode.Create);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(_screenImage));
        encoder.Save(stream);
        frames++;
    }
}
