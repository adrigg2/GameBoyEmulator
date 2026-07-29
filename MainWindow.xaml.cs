using System.Diagnostics;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GameBoyEmulator;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const int GbWidth = 160;
    const int GbHeight = 144;

    readonly BitmapPalette lcd1 = new([Color.FromRgb(198, 222, 140), Color.FromRgb(132, 165, 99), Color.FromRgb(57, 97, 57), Color.FromRgb(8, 24, 16)]);
    readonly BitmapPalette lcd2 = new([Color.FromRgb(136, 240, 0), Color.FromRgb(32, 152, 96), Color.FromRgb(64, 128, 16), Color.FromRgb(8, 72, 0)]);
    readonly BitmapPalette lcd3 = new([Color.FromRgb(155, 188, 15), Color.FromRgb(139, 172, 15), Color.FromRgb(48, 98, 48), Color.FromRgb(15, 56, 15)]);
    readonly BitmapPalette baw = new([Color.FromRgb(255, 255, 255), Color.FromRgb(170, 170, 170), Color.FromRgb(85, 85, 85), Color.FromRgb(0, 0, 0)]);

    private bool _turboMode;
    private bool _closed;
    private bool _paused;
    
    private Emulator _emulator;

    public MainWindow(string[] args)
    {
        InitializeComponent();
        Directory.CreateDirectory("./saves/");
        _emulator = new Emulator(args[0], args[1], Dispatcher);
        _emulator.PPU.SetWindowSource(this);

        string romName = args[0].Split('\\').Last();
        romName = romName[..^3];
        Title = romName; // TODO: Improve this

        SizeChanged += (_, _) => UpdateScale();

        Closed += (_, _) => _closed = true;
    }

    private void Tick(object? sender, EventArgs e)
    {
        Task.Run(() =>
        {
            /*var stopwatch = Stopwatch.StartNew();
            _emulator.ProcessFrame();

            while (_emulator.APU.SampleProvider.SampleCount < 44100 * 2 / 20)
            {
                if (stopwatch.ElapsedMilliseconds >= Emulator.FrameTime * 1000)
                {
                    stopwatch.Restart();
                    _emulator.ProcessFrame();
                }
            }

            _emulator.APU.StartAudio();

            while (true)
            {
                if (stopwatch.ElapsedMilliseconds >= Emulator.FrameTime * 1000)
                {
                    stopwatch.Restart();
                    _emulator.ProcessFrame();
                }
            }*/

            int frames = 0;

            const int primeTarget = 44100 * 2 / 20;
            const int throttleTarget = 44100 * 2 / 10;

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (_emulator.APU.SampleProvider.SampleCount < primeTarget)
            {
                _emulator.ProcessFrame();
                frames++;
            }


            _emulator.APU.StartAudio();

            while (!_closed)
            {
                _emulator.ProcessFrame();
                frames++;

                while ((_emulator.APU.SampleProvider.SampleCount > throttleTarget && !_turboMode) || _paused)
                {
                    Thread.Sleep(1);
                }

                if (stopwatch.ElapsedMilliseconds >= 60 * 1000)
                {
                    Console.WriteLine(frames / (stopwatch.ElapsedMilliseconds / 1000.0f));
                    frames = 0;
                    stopwatch.Restart();
                }
            }

            _emulator.MMU.Cartridge.SaveRam();
        });
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        //DEBUG
        if (e.Key == Key.D1)
        {
            _emulator.APU.ToggleChannel(1);
        }
        else if (e.Key == Key.D2)
        {
            _emulator.APU.ToggleChannel(2);
        }
        else if (e.Key == Key.D3)
        {
            _emulator.APU.ToggleChannel(3);
        }
        else if (e.Key == Key.D4)
        {
            _emulator.APU.ToggleChannel(4);
        }

        if (e.Key == Key.Space)
        {
            _turboMode = true;
        }

        if (e.Key == Key.F1)
        {
            _paused = true;
            var window = new VRAMViewer();

            window.Owner = this;
            window.RenderVRAM(_emulator.MMU.VRAM);
            window.ShowDialog();
            _paused = false;
        }

        _emulator.JOYPAD.HandleKeyDown(e.Key);
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            _turboMode = false;
        }

        _emulator.JOYPAD.HandleKeyUp(e.Key);
    }

    private void UpdateScale()
    {
        double scaleX = ActualWidth / GbWidth;
        double scaleY = ActualHeight / GbHeight;

        int scale = Math.Max(1, (int)Math.Floor(Math.Min(scaleX, scaleY)));

        Screen.Width = GbWidth * scale;
        Screen.Height = GbHeight * scale;
    }

    private void ChangePalette(object sender, RoutedEventArgs e)
    {
        if (sender.Equals(LCD1))
        {
            LCD1.IsChecked = true;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            _emulator.PPU.SetBitmapPalette(this, lcd1);
        }
        else if (sender.Equals(LCD2))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = true;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            _emulator.PPU.SetBitmapPalette(this, lcd2);
        }
        else if (sender.Equals(LCD3))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = true;
            BaW.IsChecked = false;
            _emulator.PPU.SetBitmapPalette(this, lcd3);
        }
        else if (sender.Equals(BaW))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = true;
            _emulator.PPU.SetBitmapPalette(this, baw);
        }
    }
}
