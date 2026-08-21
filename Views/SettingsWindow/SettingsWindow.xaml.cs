using GameBoyEmulator.SaveState;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GameBoyEmulator.Views.SettingsWindow;

/// <summary>
/// Lógica de interacción para Window1.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private object? _keybind = null;

    public SettingsWindow()
    {
        InitializeComponent();

        VolumeControl.Value = Settings.Volume;
        VolumeText.Content = $"{(int)(Settings.Volume * 100)}%";

        Color0.SelectedColor = Settings.CustomPalette[0];
        Color1.SelectedColor = Settings.CustomPalette[1];
        Color2.SelectedColor = Settings.CustomPalette[2];
        Color3.SelectedColor = Settings.CustomPalette[3];

        DPadUp.Content = Settings.DPadUp;
        DPadLeft.Content = Settings.DPadLeft;
        DPadRight.Content = Settings.DPadRight;
        DPadDown.Content = Settings.DPadDown;
        ButtonA.Content = Settings.ButtonA;
        ButtonB.Content = Settings.ButtonB;
        ButtonStart.Content = Settings.ButtonStart;
        ButtonSelect.Content = Settings.ButtonSelect;
        AllButtons.Content = Settings.AllButtons;
        ToggleChannel1.Content = Settings.ToggleChannel1;
        ToggleChannel2.Content = Settings.ToggleChannel2;
        ToggleChannel3.Content = Settings.ToggleChannel3;
        ToggleChannel4.Content = Settings.ToggleChannel4;
        TurboMode.Content = Settings.TurboMode;
        RewindMode.Content = Settings.RewindButton;
        VRAMInspector.Content = Settings.OpenVramViewer;
        QuickLoad.Content = Settings.QuickLoad;
        QuickSave.Content = Settings.QuickSave;

        BootRomFilePath.Content = Settings.BootRomFilePath;

        Closed += (_, _) =>
        {
            Settings.Volume = (float)VolumeControl.Value;
            Settings.CustomPalette[0] = Color0.SelectedColor ?? Color.FromRgb(255, 255, 255);
            Settings.CustomPalette[1] = Color1.SelectedColor ?? Color.FromRgb(255, 255, 255);
            Settings.CustomPalette[2] = Color2.SelectedColor ?? Color.FromRgb(255, 255, 255);
            Settings.CustomPalette[3] = Color3.SelectedColor ?? Color.FromRgb(255, 255, 255);
        };
    }

    public void UpdateVolumeValue(object sender, RoutedEventArgs e)
    {
        VolumeText.Content = $"{(int)(VolumeControl.Value * 100)}%";
    }

    public void BeginChangeKeyBinding(object sender, RoutedEventArgs e)
    {
        _keybind = sender;
        if (_keybind != null && _keybind is Button b)
        {
            string content = b.Content.ToString() ?? "";
            content = content.Trim('[');
            content = content.Trim(']');
            b.Content = content;
        }

        if (sender is Button button)
        {
            button.Content = $"[{button.Content}]";
        }
    }

    public void ChangeKeyBinding(object sender, KeyEventArgs e)
    {
        if (_keybind == null || e.Key == Key.Escape)
        {
            return;
        }

        if (_keybind.Equals(DPadUp))
        {
            Settings.DPadUp = e.Key;
            DPadUp.Content = Settings.DPadUp;
        }
        else if (_keybind.Equals(DPadLeft))
        {
            Settings.DPadLeft = e.Key;
            DPadLeft.Content = Settings.DPadLeft;
        }
        else if (_keybind.Equals(DPadRight))
        {
            Settings.DPadRight = e.Key;
            DPadRight.Content = Settings.DPadRight;
        }
        else if (_keybind.Equals(DPadDown))
        {
            Settings.DPadDown = e.Key;
            DPadDown.Content = Settings.DPadDown;
        }
        else if (_keybind.Equals(ButtonA))
        {
            Settings.ButtonA = e.Key;
            ButtonA.Content = Settings.ButtonA;
        }
        else if (_keybind.Equals(ButtonB))
        {
            Settings.ButtonB = e.Key;
            ButtonB.Content = Settings.ButtonB;
        }
        else if (_keybind.Equals(ButtonStart))
        {
            Settings.ButtonStart = e.Key;
            ButtonStart.Content = Settings.ButtonStart;
        }
        else if (_keybind.Equals(ButtonSelect))
        {
            Settings.ButtonSelect = e.Key;
            ButtonSelect.Content = Settings.ButtonSelect;
        }
        else if (_keybind.Equals(AllButtons))
        {
            Settings.AllButtons = e.Key;
            AllButtons.Content = Settings.AllButtons;
        }
        else if (_keybind.Equals(ToggleChannel1))
        {
            Settings.ToggleChannel1 = e.Key;
            ToggleChannel1.Content = Settings.ToggleChannel1;
        }
        else if (_keybind.Equals(ToggleChannel2))
        {
            Settings.ToggleChannel2 = e.Key;
            ToggleChannel2.Content = Settings.ToggleChannel2;
        }
        else if (_keybind.Equals(ToggleChannel3))
        {
            Settings.ToggleChannel3 = e.Key;
            ToggleChannel3.Content = Settings.ToggleChannel3;
        }
        else if (_keybind.Equals(ToggleChannel4))
        {
            Settings.ToggleChannel4 = e.Key;
            ToggleChannel4.Content = Settings.ToggleChannel4;
        }
        else if (_keybind.Equals(TurboMode))
        {
            Settings.TurboMode = e.Key;
            TurboMode.Content = Settings.TurboMode;
        }
        else if (_keybind.Equals(RewindMode))
        {
            Settings.RewindButton = e.Key;
            RewindMode.Content = Settings.RewindButton;
        }
        else if (_keybind.Equals(DPadLeft))
        {
            Settings.DPadLeft = e.Key;
            DPadLeft.Content = Settings.DPadLeft;
        }
        else if (_keybind.Equals(VRAMInspector))
        {
            Settings.OpenVramViewer = e.Key;
            VRAMInspector.Content = Settings.OpenVramViewer;
        }
        else if (_keybind.Equals(QuickSave))
        {
            Settings.QuickSave = e.Key;
            QuickSave.Content = Settings.QuickSave;
        }
        else if (_keybind.Equals(QuickLoad))
        {
            Settings.QuickLoad = e.Key;
            QuickLoad.Content = Settings.QuickLoad;
        }

        _keybind = null;
    }

    public void ResetKeyBinding(object sender, RoutedEventArgs e)
    {
        if (sender.Equals(ResetDPadUp))
        {
            Settings.DPadUp = Key.W;
            DPadUp.Content = Settings.DPadUp;
        }
        else if (sender.Equals(ResetDPadLeft))
        {
            Settings.DPadLeft = Key.A;
            DPadLeft.Content = Settings.DPadLeft;
        }
        else if (sender.Equals(ResetDPadRight))
        {
            Settings.DPadRight = Key.D;
            DPadRight.Content = Settings.DPadRight;
        }
        else if (sender.Equals(ResetDPadDown))
        {
            Settings.DPadDown = Key.S;
            DPadDown.Content = Settings.DPadDown;
        }
        else if (sender.Equals(ResetButtonA))
        {
            Settings.ButtonA = Key.U;
            ButtonA.Content = Settings.ButtonA;
        }
        else if (sender.Equals(ResetButtonB))
        {
            Settings.ButtonB = Key.I;
            ButtonB.Content = Settings.ButtonB;
        }
        else if (sender.Equals(ResetButtonStart))
        {
            Settings.ButtonStart = Key.O;
            ButtonStart.Content = Settings.ButtonStart;
        }
        else if (sender.Equals(ResetButtonSelect))
        {
            Settings.ButtonSelect = Key.L;
            ButtonSelect.Content = Settings.ButtonSelect;
        }
        else if (sender.Equals(ResetAllButtons))
        {
            Settings.AllButtons = Key.K;
            AllButtons.Content = Settings.AllButtons;
        }
        else if (sender.Equals(ResetToggleChannel1))
        {
            Settings.ToggleChannel1 = Key.D1;
            ToggleChannel1.Content = Settings.ToggleChannel1;
        }
        else if (sender.Equals(ResetToggleChannel2))
        {
            Settings.ToggleChannel2 = Key.D2;
            ToggleChannel2.Content = Settings.ToggleChannel2;
        }
        else if (sender.Equals(ResetToggleChannel3))
        {
            Settings.ToggleChannel3 = Key.D3;
            ToggleChannel3.Content = Settings.ToggleChannel3;
        }
        else if (sender.Equals(ResetToggleChannel4))
        {
            Settings.ToggleChannel4 = Key.D4;
            ToggleChannel4.Content = Settings.ToggleChannel4;
        }
        else if (sender.Equals(ResetTurboMode))
        {
            Settings.TurboMode = Key.Space;
            TurboMode.Content = Settings.TurboMode;
        }
        else if (sender.Equals(ResetRewindMode))
        {
            Settings.RewindButton = Key.LeftCtrl;
            RewindMode.Content = Settings.RewindButton;
        }
        else if (sender.Equals(ResetVRAMInspector))
        {
            Settings.OpenVramViewer = Key.F1;
            VRAMInspector.Content = Settings.OpenVramViewer;
        }
        else if (sender.Equals(ResetQuickSave))
        {
            Settings.QuickSave = Key.F2;
            QuickSave.Content = Settings.QuickSave;
        }
        else if (sender.Equals(ResetQuickLoad))
        {
            Settings.QuickLoad = Key.F3;
            QuickLoad.Content = Settings.QuickLoad;
        }
    }

    private void SelectBootRomFile(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = "boot",
            DefaultExt = ".bin",
        };

        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            Settings.BootRomFilePath = dialog.FileName;
            BootRomFilePath.Content = Settings.BootRomFilePath;
        }
    }
}
