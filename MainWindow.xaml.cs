using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace GameBoyEmulator;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Emulator _emulator;

    public MainWindow(Emulator emulator)
    {
        InitializeComponent();
        _emulator = emulator;
        _emulator.PPU.WindowDispatcher = Dispatcher; // TODO: Consider transfering logic to the main window
        _emulator.PPU.SetWindowSource(this); // TODO: Consider transfering logic to the main window
    }

    private void Tick(object? sender, EventArgs e)
    {
        Task.Run(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            _emulator.ProcessFrame();
            var frames = 1;

            while (frames <= 6960)
            {
                if (stopwatch.ElapsedMilliseconds >= Emulator.FrameTime * 1000)
                {
                    //Console.WriteLine("Tick");
                    stopwatch.Restart();
                    _emulator.ProcessFrame();
                    frames++;
                }
            }
        });
    }
}
