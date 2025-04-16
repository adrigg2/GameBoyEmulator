using System.Windows;

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
        _emulator.PPU.WindowDispatcher = Dispatcher; // TODO: Consider transferring logic to the main window
        _emulator.PPU.SetWindowSource(this); // TODO: Consider transferring logic to the main window
        ContentRendered += OnRendered;
    }

    private void OnRendered(object? sender, EventArgs e)
    {
        Task.Run(() =>
        {
            while (true)
            {
                _emulator.Tick();
            }
        });
    }
}
