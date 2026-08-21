using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoyEmulator;

public static class Settings
{
    private readonly static SettingsData _data = new();

    public static float Volume { get => _data.Volume; set => _data.Volume = value; }
    public static Color[] Palette { get => _data.Palette; set { _data.Palette = value; SaveSettings(); } }
    public static Color[] CustomPalette { get => _data.CustomPalette; set => _data.CustomPalette = value; }
    public static Key DPadUp { get => _data.DPadUp; set => _data.DPadUp = value; }
    public static Key DPadLeft { get => _data.DPadLeft; set => _data.DPadLeft = value; }
    public static Key DPadRight { get => _data.DPadRight; set => _data.DPadRight = value; }
    public static Key DPadDown { get => _data.DPadDown; set => _data.DPadDown = value; }
    public static Key ButtonA { get => _data.ButtonA; set => _data.ButtonA = value; }
    public static Key ButtonB { get => _data.ButtonB; set => _data.ButtonB = value; }
    public static Key ButtonStart { get => _data.ButtonStart; set => _data.ButtonStart = value; }
    public static Key ButtonSelect { get => _data.ButtonSelect; set => _data.ButtonSelect = value; }
    public static Key AllButtons { get => _data.AllButtons; set => _data.AllButtons = value; }
    public static Key ToggleChannel1 { get => _data.ToggleChannel1; set => _data.ToggleChannel1 = value; }
    public static Key ToggleChannel2 { get => _data.ToggleChannel2; set => _data.ToggleChannel2 = value; }
    public static Key ToggleChannel3 { get => _data.ToggleChannel3; set => _data.ToggleChannel3 = value; }
    public static Key ToggleChannel4 { get => _data.ToggleChannel4; set => _data.ToggleChannel4 = value; }
    public static Key TurboMode { get => _data.TurboMode; set => _data.TurboMode = value; }
    public static Key RewindButton { get => _data.RewindButton; set => _data.RewindButton = value; }
    public static Key OpenVramViewer { get => _data.OpenVramViewer; set => _data.OpenVramViewer = value; }
    public static Key QuickSave { get => _data.QuickSave; set => _data.QuickSave = value; }
    public static Key QuickLoad { get => _data.QuickLoad; set => _data.QuickLoad = value; }
    public static string BootRomFilePath { get => _data.BootRomPath; set => _data.BootRomPath = value; }


    static Settings()
    {
        if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GEGB/config.json"))
        {
            string json = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GEGB/config.json");
            _data = JsonSerializer.Deserialize<SettingsData>(json) ?? new();
        }
    }

    public static void SaveSettings()
    {
        string json = JsonSerializer.Serialize(_data);

        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GEGB/");
        File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GEGB/config.json", json);
    }

    private class SettingsData
    {
        public float Volume { get; set; } = 1.0f;
        public Color[] Palette { get; set; } = [Color.FromRgb(198, 222, 140), Color.FromRgb(132, 165, 99), Color.FromRgb(57, 97, 57), Color.FromRgb(8, 24, 16)];
        public Color[] CustomPalette { get; set; } = [Color.FromRgb(0, 0, 0), Color.FromRgb(0, 0, 0), Color.FromRgb(0, 0, 0), Color.FromRgb(0, 0, 0)];
        public Key DPadUp { get; set; } = Key.W;
        public Key DPadLeft { get; set; } = Key.A;
        public Key DPadRight { get; set; } = Key.D;
        public Key DPadDown { get; set; } = Key.S;
        public Key ButtonA { get; set; } = Key.U;
        public Key ButtonB { get; set; } = Key.I;
        public Key ButtonStart { get; set; } = Key.O;
        public Key ButtonSelect { get; set; } = Key.L;
        public Key AllButtons { get; set; } = Key.K;
        public Key ToggleChannel1 { get; set; } = Key.D1;
        public Key ToggleChannel2 { get; set; } = Key.D2;
        public Key ToggleChannel3 { get; set; } = Key.D3;
        public Key ToggleChannel4 { get; set; } = Key.D4;
        public Key TurboMode { get; set; } = Key.Space;
        public Key RewindButton { get; set; } = Key.LeftCtrl;
        public Key OpenVramViewer { get; set; } = Key.F1;
        public Key QuickSave { get; set; } = Key.F2;
        public Key QuickLoad { get; set; } = Key.F3;
        public string BootRomPath { get; set; } = "";
    }
}
