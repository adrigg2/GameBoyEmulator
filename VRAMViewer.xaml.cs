using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoyEmulator;

/// <summary>
/// Lógica de interacción para VRAMViewer.xaml
/// </summary>
public partial class VRAMViewer : Window
{
    public VRAMViewer()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdateScale();
    }

    public void RenderVRAM(byte[] vram, IList<Color> colorsInUse)
    {
        List<Color> colors = [.. colorsInUse];
        colors.Add(Color.FromRgb(255, 255, 255));
        colors.Add(Color.FromRgb(0, 0, 255));

        BitmapPalette palette = new(colors);
        WriteableBitmap screenImage = new((int)Screen.Width, (int)Screen.Height, 96, 96, PixelFormats.Indexed8, palette);
        int stride = (int)Screen.Width;
        int totalBytes = (int)Screen.Height * stride;
        byte[] pixels = [.. Enumerable.Repeat((byte)4, totalBytes)];
        Screen.Source = screenImage;

        int offsetX = 0;
        int offsetY = 0;
        int y = 0;

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
            }

            if (offsetX + 9 < Screen.Width)
            {
                pixels[(y + offsetY) * (int)Screen.Width + offsetX + 8] = 4;
            }

            y++;

            if (y >= 8 && offsetX + 9 < Screen.Width)
            {
                y = 0;
                offsetX += 9;
            }
            else if (y >= 8)
            {
                y = 0;
                offsetY += 9;
                offsetX = 0;

                if (offsetY / 9 == 8 || offsetY / 9 == 16)
                {
                    for (int j = 0; j < Screen.Width; j++)
                    {
                        pixels[(offsetY - 1) * (int)Screen.Width + j] = 5;
                    }
                }
            }
        }

        screenImage.WritePixels(new Int32Rect(0, 0, (int)Screen.Width, (int)Screen.Height), pixels, stride, 0);
    }

    private void UpdateScale()
    {
        double scaleX = ActualWidth / Screen.Width;
        double scaleY = ActualHeight / Screen.Height;

        int scale = Math.Max(1, (int)Math.Floor(Math.Min(scaleX, scaleY)));

        Screen.Width = Screen.Width * scale;
        Screen.Height = Screen.Height * scale;
    }
}
