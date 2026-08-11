using GameBoyEmulator.SaveState.Components;
using System.IO;

namespace GameBoyEmulator.SaveState;

public static class SaveStateSerializer
{
    private const string Magic = "GEGB";
    private const byte Version = 1;

    public static void SerializeSaveState(string path, SaveState saveState)
    {
        using FileStream stream = File.OpenWrite(path);
        using BinaryWriter writer = new(stream);

        writer.Write(Magic);
        writer.Write(Version);

        writer.Write(saveState.CPU.Serialize());
    }
}
