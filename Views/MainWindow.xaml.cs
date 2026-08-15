using GameBoyEmulator.SaveState;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoyEmulator.Views;

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
    private bool _paused;
    private bool _rewinding;

    private string _bootRomFilePath;

    private BitmapPalette _paletteInUse;
    
    private Emulator? _emulator;

    private Thread? _emulatorThread;
    private CancellationTokenSource? _cts;

    private RewindStack _rewindStack;

    public MainWindow(string[] args)
    {
        InitializeComponent();
        Directory.CreateDirectory("./saves/");
        Directory.CreateDirectory("./states/");

        SizeChanged += (_, _) => UpdateScale();

        Closed += (_, _) =>
        {
            _cts?.Cancel();

            if (_emulatorThread?.IsAlive == true)
            {
                _emulatorThread.Join();
            }

            _emulatorThread = null;

            _cts?.Dispose();
            _cts = null;
        };

        _paletteInUse = lcd1;
        _bootRomFilePath = args[0];

        _rewindStack = new(50);
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
        if (e.Key == Key.D1)
        {
            _emulator?.APU.ToggleChannel(1);
        }
        else if (e.Key == Key.D2)
        {
            _emulator?.APU.ToggleChannel(2);
        }
        else if (e.Key == Key.D3)
        {
            _emulator?.APU.ToggleChannel(3);
        }
        else if (e.Key == Key.D4)
        {
            _emulator?.APU.ToggleChannel(4);
        }

        if (e.Key == Key.Space)
        {
            _turboMode = true;
        }

        if (e.Key == Key.LeftCtrl)
        {
            _rewinding = true;
        }

        if (e.Key == Key.F1 && _emulator != null)
        {
            _paused = true;
            var window = new VRAMInspector.VRAMViewer
            {
                Owner = this
            };
            window.RenderVRAM(_emulator.MMU.VRAM, _paletteInUse.Colors);
            window.ShowDialog();
            _paused = false;
        }

        _emulator?.JOYPAD.HandleKeyDown(e.Key);
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            _turboMode = false;
        }

        if (e.Key == Key.LeftCtrl)
        {
            _rewinding = false;
        }

        _emulator?.JOYPAD.HandleKeyUp(e.Key);
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
            _emulator?.PPU.SetBitmapPalette(this, lcd1);
            _paletteInUse = lcd1;
        }
        else if (sender.Equals(LCD2))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = true;
            LCD3.IsChecked = false;
            BaW.IsChecked = false;
            _emulator?.PPU.SetBitmapPalette(this, lcd2);
            _paletteInUse = lcd2;
        }
        else if (sender.Equals(LCD3))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = true;
            BaW.IsChecked = false;
            _emulator?.PPU.SetBitmapPalette(this, lcd3);
            _paletteInUse = lcd3;
        }
        else if (sender.Equals(BaW))
        {
            LCD1.IsChecked = false;
            LCD2.IsChecked = false;
            LCD3.IsChecked = false;
            BaW.IsChecked = true;
            _emulator?.PPU.SetBitmapPalette(this, baw);
            _paletteInUse = baw;
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
            if (romFilePath.Split('\\').Last().EndsWith(".gb"))
            {
                _cts?.Cancel();

                if (_emulatorThread?.IsAlive == true)
                {
                    _emulatorThread.Join();
                }

                _emulator = new Emulator(romFilePath, _bootRomFilePath, Dispatcher);
                _emulator?.PPU.SetWindowSource(this);
                _emulator?.PPU.SetBitmapPalette(this, _paletteInUse);

                string romName = romFilePath.Split('\\').Last();
                romName = romName[..^3];
                Title = romName;

                _rewindStack?.Clear();

                _cts = new CancellationTokenSource();
                _emulatorThread = new Thread(() => Tick(_cts.Token));
                _emulatorThread.Start();
            }

        }
    }
}
