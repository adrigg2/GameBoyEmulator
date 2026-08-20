using GameBoyEmulator.SaveState;
using GameBoyEmulator.Views.SettingsWindow;
using GameBoyEmulator.Views.VRAMInspector;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GameBoyEmulator.Views;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const int GbWidth = 160;
    const int GbHeight = 144;

    readonly Color[] lcd1 = [Color.FromRgb(198, 222, 140), Color.FromRgb(132, 165, 99), Color.FromRgb(57, 97, 57), Color.FromRgb(8, 24, 16)];
    readonly Color[] lcd2 = [Color.FromRgb(136, 240, 0), Color.FromRgb(32, 152, 96), Color.FromRgb(64, 128, 16), Color.FromRgb(8, 72, 0)];
    readonly Color[] lcd3 = [Color.FromRgb(155, 188, 15), Color.FromRgb(139, 172, 15), Color.FromRgb(48, 98, 48), Color.FromRgb(15, 56, 15)];
    readonly Color[] baw = [Color.FromRgb(255, 255, 255), Color.FromRgb(170, 170, 170), Color.FromRgb(85, 85, 85), Color.FromRgb(0, 0, 0)];

    private bool _turboMode;
    private bool _paused;
    private bool _rewinding;

    private string _bootRomFilePath;
    
    private Emulator? _emulator;

    private Thread? _emulatorThread;
    private CancellationTokenSource? _cts;

    private RewindStack _rewindStack;

    public MainWindow(string[] args)
    {
        InitializeComponent();
        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GEGB/states/");

        SizeChanged += (_, _) => UpdateScale();

        Closed += (_, _) =>
        {
            _cts?.Cancel();

            if (_emulatorThread?.IsAlive == true)
            {
                _emulatorThread.Join();
            }

            _emulator?.MMU.Cartridge.SaveRam();

            _emulatorThread = null;

            _cts?.Dispose();
            _cts = null;
        };

        _bootRomFilePath = args[0];

        _rewindStack = new(50);

        MinWidth = GbWidth + 100;
        MinHeight = GbHeight + 100;

        if (Enumerable.SequenceEqual(lcd1, Settings.Palette))
        {
            LCD1.IsChecked = true;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            CustomPalette.IsChecked = false;
        }
        else if (Enumerable.SequenceEqual(lcd2, Settings.Palette))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = true;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            CustomPalette.IsChecked = false;
        }
        else if (Enumerable.SequenceEqual(lcd3, Settings.Palette))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = true;
            BaW.IsChecked = false;
            CustomPalette.IsChecked = false;
        }
        else if (Enumerable.SequenceEqual(baw, Settings.Palette))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = true;
            CustomPalette.IsChecked = false;
        }
        else
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            CustomPalette.IsChecked = true;
        }
    }

    private void Tick(CancellationToken token)
    {
        int frames = 0;

        const int throttleTarget = 44100 / 50;
        const int framesPerSave = 6; // 5 seconds of rewind

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (_emulator == null)
        {
            return;
        }

        while (!token.IsCancellationRequested)
        {
            _emulator.ProcessFrame();
            frames++;

            if (frames % framesPerSave == 0)
            {
                _rewindStack.Push(_emulator.SaveState());
            }

            while (_rewinding && _rewindStack.Count > 0)
            {
                _emulator.LoadState(_rewindStack.Pop());
                _emulator.ProcessFrame();
                Thread.Sleep(10);
            }

            while ((_emulator.APU.SampleProvider.SampleCount > throttleTarget && !_turboMode) || _paused)
            {
                Thread.Sleep(1);
            }

            if (_turboMode)
            {
                _emulator.APU.ClearAudioBuffer();
            }

            if (stopwatch.ElapsedMilliseconds >= 60 * 1000)
            {
                Console.WriteLine(frames / (stopwatch.ElapsedMilliseconds / 1000.0f));
                frames = 0;
                stopwatch.Restart();
            }
        }

        _emulator.MMU.Cartridge.SaveRam();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        //DEBUG
        if (e.Key == Settings.ToggleChannel1)
        {
            _emulator?.APU.ToggleChannel(1);
        }
        else if (e.Key == Settings.ToggleChannel2)
        {
            _emulator?.APU.ToggleChannel(2);
        }
        else if (e.Key == Settings.ToggleChannel3)
        {
            _emulator?.APU.ToggleChannel(3);
        }
        else if (e.Key == Settings.ToggleChannel4)
        {
            _emulator?.APU.ToggleChannel(4);
        }

        if (e.Key == Settings.TurboMode)
        {
            _turboMode = true;
        }

        if (e.Key == Settings.RewindButton)
        {
            _rewinding = true;
        }

        if (e.Key == Settings.OpenVramViewer && _emulator != null)
        {
            _paused = true;
            var window = new VRAMViewer
            {
                Owner = this
            };
            window.RenderVRAM((byte[])_emulator.MMU.VRAM.Clone(), _emulator.PPU.LCDC, _emulator.PPU.SCX, _emulator.PPU.SCY, Settings.Palette);
            window.ShowDialog();
            _paused = false;
        }

        _emulator?.JOYPAD.HandleKeyDown(e.Key);
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Settings.TurboMode)
        {
            _turboMode = false;
        }

        if (e.Key == Settings.RewindButton)
        {
            _rewinding = false;
        }

        _emulator?.JOYPAD.HandleKeyUp(e.Key);
    }

    private void UpdateScale()
    {
        double scaleX = ScreenContainer.ActualWidth / GbWidth;
        double scaleY = ScreenContainer.ActualHeight / GbHeight;

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
            CustomPalette.IsChecked = false;
            _emulator?.PPU.SetBitmapPalette(this, lcd1);
            Settings.Palette = lcd1;
        }
        else if (sender.Equals(LCD2))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = true;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            CustomPalette.IsChecked = false;
            _emulator?.PPU.SetBitmapPalette(this, lcd2);
            Settings.Palette = lcd2;
        }
        else if (sender.Equals(LCD3))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = true;
            BaW.IsChecked = false;
            CustomPalette.IsChecked = false;
            _emulator?.PPU.SetBitmapPalette(this, lcd3);
            Settings.Palette = lcd3;
        }
        else if (sender.Equals(BaW))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = true;
            CustomPalette.IsChecked = false;
            _emulator?.PPU.SetBitmapPalette(this, baw);
            Settings.Palette = baw;
        }
        else if (sender.Equals(CustomPalette))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            CustomPalette.IsChecked = true;
            _emulator?.PPU.SetBitmapPalette(this, Settings.CustomPalette);
            Settings.Palette = Settings.CustomPalette;
        }
    }

    private void OpenROM(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = "Game",
            DefaultExt = ".gb",
            Filter = "GameBoy ROM (.gb)|*.gb"
        };

        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            string romFilePath = dialog.FileName;
            string fileExtension = Path.GetExtension(romFilePath);
            if (fileExtension.ToLower().Equals(".gb"))
            {
                _cts?.Cancel();

                if (_emulatorThread?.IsAlive == true)
                {
                    _emulatorThread.Join();
                }

                _emulator?.MMU.Cartridge.SaveRam();

                _emulator = new Emulator(romFilePath, _bootRomFilePath, Dispatcher);
                _emulator?.PPU.SetWindowSource(this);
                _emulator?.PPU.SetBitmapPalette(this, Settings.Palette);

                string romName = Path.GetFileNameWithoutExtension(romFilePath);
                Title = romName;

                _rewindStack?.Clear();

                _cts = new CancellationTokenSource();
                _emulatorThread = new Thread(() => Tick(_cts.Token));
                _emulatorThread.Start();
            }

        }
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        _paused = true;
        var window = new SettingsWindow.SettingsWindow
        {
            Owner = this
        };

        window.ShowDialog();
        Settings.SaveSettings();
        if (CustomPalette.IsChecked)
        {
            _emulator?.PPU.SetBitmapPalette(this, Settings.CustomPalette);
        }

        _paused = false;
    }
}
