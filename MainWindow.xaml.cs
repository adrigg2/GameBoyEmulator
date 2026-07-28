using System.Diagnostics;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GameBoyEmulator;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const int GbWidth = 160;
    const int GbHeight = 144;

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
}
