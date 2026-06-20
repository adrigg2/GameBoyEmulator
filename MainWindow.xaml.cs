using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace GameBoyEmulator;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Emulator _emulator;

    public MainWindow(string[] args)
    {
        InitializeComponent();
        _emulator = new Emulator(args[0], args[1], Dispatcher);
        _emulator.PPU.SetWindowSource(this); // TODO: Consider transfering logic to the main window
    }

    private void Tick(object? sender, EventArgs e)
    {
        Task.Run(() =>
        {
            using StreamWriter logFile = new("./emulatorLog.log");
            logFile.WriteLine("Frame 0");

            var stopwatch = Stopwatch.StartNew();
            _emulator.ProcessFrame(logFile);
            var frames = 1;

            while (true)
            {
                if (stopwatch.ElapsedMilliseconds >= Emulator.FrameTime * 1000)
                {
                    logFile.WriteLine($"Frame {frames}");
                    //Console.WriteLine("Tick");
                    stopwatch.Restart();
                    _emulator.ProcessFrame(logFile);
                    frames++;
                }
            }
        });
    }
}
