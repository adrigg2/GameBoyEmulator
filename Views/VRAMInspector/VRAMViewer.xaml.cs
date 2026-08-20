using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoyEmulator.Views.VRAMInspector;

/// <summary>
/// Lógica de interacción para VRAMViewer.xaml
/// </summary>
public partial class VRAMViewer : Window
{
    private const int BaseWidth = 665;
    private const int BaseHeight = 256;
    private const int TilesWidth = 143;
    private const int TilesHeightOffset = 20;
    private const int TileMapWidth = 256;
    private const int TileMap0Offset = TilesWidth + 5;
    private const int TileMap1Offset = TileMap0Offset + TileMapWidth + 5;

    public VRAMViewer()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdateScale();
    }

    public void RenderVRAM(byte[] vram, byte lcdc, byte scx, byte scy, IList<Color> colorsInUse)
    {
        List<Color> colors = [.. colorsInUse];
        colors.Add(Color.FromRgb(255, 255, 255));
        colors.Add(Color.FromRgb(0, 0, 255));
        colors.Add(Color.FromRgb(255, 0, 0));

        BitmapPalette palette = new(colors);
        WriteableBitmap screenImage = new((int)Screen.Width, (int)Screen.Height, 96, 96, PixelFormats.Indexed8, palette);
        int stride = (int)Screen.Width;
        int totalBytes = (int)Screen.Height * stride;
        byte[] pixels = [.. Enumerable.Repeat((byte)4, totalBytes)];
        Screen.Source = screenImage;

        byte[,] tiles = new byte[384, 64];
        int offsetX = 0;
        int offsetY = TilesHeightOffset;
        int y = 0;
        int tileNum = 0;
        int tileColor = 0;

        for (int i = 0; i < 0x1800; i += 2)
        {
            byte tileRowLow = vram[i];
            byte tileRowHigh = vram[i + 1];

            for (int x = offsetX; x < offsetX + 8; x++)
            {
                int colorIdLow = (tileRowLow >> (7 - (x - offsetX))) & 0x1;
                int colorIdHigh = (tileRowHigh >> (7 - (x - offsetX) - 1)) & 0x2;
                int colorId = colorIdLow + colorIdHigh;

                pixels[(y + offsetY) * (int)Screen.Width + x] = (byte)colorId;
                tiles[tileNum, tileColor] = (byte)colorId;
                tileColor++;
            }

            if (offsetX + 9 < Screen.Width)
            {
                pixels[(y + offsetY) * (int)Screen.Width + offsetX + 8] = 4;
            }

            y++;

            if (y >= 8 && offsetX + 9 < TilesWidth)
            {
                y = 0;
                offsetX += 9;
                tileNum++;
                tileColor = 0;
            }
            else if (y >= 8)
            {
                y = 0;
                offsetY += 9;
                offsetX = 0;
                tileNum++;
                tileColor = 0;

                if (offsetY / 9 == 8 || offsetY / 9 == 16)
                {
                    for (int j = 0; j < TilesWidth; j++)
                    {
                        pixels[(offsetY - 1) * (int)Screen.Width + j] = 5;
                    }
                }
            }
        }

        bool tileAddressing = (lcdc & 0x10) != 0;
        bool bgEnable = (lcdc & 0x01) != 0;
        bool bgTileMap = (lcdc & 0x08) != 0;
        int bottomY = (scy + 143) % 256;
        int rightX = (scx + 159) % 256;

        Console.WriteLine(scx);
        Console.WriteLine(scy);
        Console.WriteLine(bottomY);
        Console.WriteLine(rightX);
        Console.WriteLine(bgTileMap);

        offsetX = TileMap0Offset;
        offsetY = 0;
        for (int i = 0x1800; i < 0x1C00; i++)
        {
            byte tile = vram[i];
            int tileIndex;
            if (tileAddressing)
            {
                tileIndex = tile;
            }
            else
            {
                tileIndex = 256 + (sbyte)tile;
            }

            int colorIndex = 0;
            for (y = offsetY; y < offsetY + 8; y++)
            {
                for (int x = offsetX; x < offsetX + 8; x++)
                {
                    pixels[y * (int)Screen.Width + x] = tiles[tileIndex, colorIndex];
                    colorIndex++;

                    int xWithoutOffset = x - TileMap0Offset;
                    if (bgEnable && !bgTileMap)
                    {
                        if (scx > rightX)
                        {
                            if ((y == scy || y == bottomY) && xWithoutOffset > scx || xWithoutOffset < rightX)
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }
                        else
                        {
                            if (xWithoutOffset > scx && xWithoutOffset <  rightX && (y == scy || y == bottomY))
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }

                        if (scy > bottomY)
                        {
                            if ((xWithoutOffset == scx || xWithoutOffset == rightX) && (y > scy || y < bottomY))
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }
                        else
                        {
                            if (y > scy && y < bottomY && (xWithoutOffset == scx || xWithoutOffset == rightX))
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }
                    }
                }
            }

            offsetX += 8;
            if (offsetX - TileMap0Offset >= TileMapWidth)
            {
                offsetX = TileMap0Offset;
                offsetY += 8;
            }
        }

        offsetX = TileMap1Offset;
        offsetY = 0;
        for (int i = 0x1C00; i < 0x2000; i++)
        {
            byte tile = vram[i];
            int tileIndex;
            if (tileAddressing)
            {
                tileIndex = tile;
            }
            else
            {
                tileIndex = 256 + (sbyte)tile;
            }

            int colorIndex = 0;
            for (y = offsetY; y < offsetY + 8; y++)
            {
                for (int x = offsetX; x < offsetX + 8; x++)
                {
                    pixels[y * (int)Screen.Width + x] = tiles[tileIndex, colorIndex];
                    colorIndex++;

                    int xWithoutOffset = x - TileMap0Offset;
                    if (bgEnable && !bgTileMap)
                    {
                        if (scx > rightX)
                        {
                            if ((y == scy || y == bottomY) && xWithoutOffset > scx || xWithoutOffset < rightX)
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }
                        else
                        {
                            if (xWithoutOffset > scx && xWithoutOffset < rightX && (y == scy || y == bottomY))
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }

                        if (scy > bottomY)
                        {
                            if ((xWithoutOffset == scx || xWithoutOffset == rightX) && (y > scy || y < bottomY))
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }
                        else
                        {
                            if (y > scy && y < bottomY && (xWithoutOffset == scx || xWithoutOffset == rightX))
                            {
                                pixels[y * (int)Screen.Width + x] = 6;
                            }
                        }
                    }
                }
            }

            offsetX += 8;
            if (offsetX - TileMap1Offset >= TileMapWidth)
            {
                offsetX = TileMap1Offset;
                offsetY += 8;
            }
        }

        screenImage.WritePixels(new Int32Rect(0, 0, (int)Screen.Width, (int)Screen.Height), pixels, stride, 0);
    }

    private void UpdateScale()
    {
        double scaleX = ScreenContainer.ActualWidth / BaseWidth;
        double scaleY = ScreenContainer.ActualHeight / BaseHeight;

        int scale = Math.Max(1, (int)Math.Floor(Math.Min(scaleX, scaleY)));

        Screen.Width = BaseWidth * scale;
        Screen.Height = BaseHeight * scale;
    }
}
