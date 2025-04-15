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
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _emulator.Start(this);
    }
}
