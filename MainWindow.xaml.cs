using System.Diagnostics;
using System.IO;
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
    
    private Emulator _emulator;

    public MainWindow(string[] args)
    {
        InitializeComponent();
        Directory.CreateDirectory("./saves/");
        _emulator = new Emulator(args[0], args[1], Dispatcher);
        _emulator.PPU.SetWindowSource(this); // TODO: Consider transfering logic to the main window

        SizeChanged += (_, _) => UpdateScale();
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

            while (true)
            {
                _emulator.ProcessFrame();
                frames++;

                while (_emulator.APU.SampleProvider.SampleCount > throttleTarget && !_turboMode)
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
        });
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            _turboMode = true;
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
