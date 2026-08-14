using GameBoyEmulator.SaveState.Components;
using GameBoyEmulator.SaveState.Components.APU;
using System.IO;

namespace GameBoyEmulator.SaveState;

public static class SaveStateSerializer
{
    private static readonly byte[] Magic = "GEGB"u8.ToArray();
    private const byte Version = 1;

    public static void SerializeSaveState(string path, SaveState saveState)
    {
        using FileStream stream = File.OpenWrite(path);
        using BinaryWriter writer = new(stream);

        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(saveState.MMU.Cartridge.HeaderCheck);

        saveState.CPU.Write(writer);
        saveState.PPU.Write(writer);
        saveState.JOYPAD.Write(writer);
        saveState.DMA.Write(writer);
        saveState.TIMER.Write(writer);
        saveState.MMU.Write(writer);
        saveState.APU.Write(writer);
    }

    public static SaveState DeserializeSaveState(string path, byte[] headerCheck)
    {
        using FileStream stream = File.OpenRead(path);
        using BinaryReader reader = new(stream);

        byte[] magic = reader.ReadBytes(Magic.Length);
        byte version = reader.ReadByte();
        if (!Enumerable.SequenceEqual(magic, Magic) || version != Version)
        {
            throw new FileFormatException("The file given is not a save state file for this emulator");
        }

        byte[] fileHeaderCheck = reader.ReadBytes(headerCheck.Length);
        if (!Enumerable.SequenceEqual(fileHeaderCheck, headerCheck))
        {
            throw new FileFormatException("The save state given is not for the ROM currently loaded");
        }

        CPUState cpu = CPUState.FromBinaryData(reader);
        PPUState ppu = PPUState.FromBinaryData(reader);
        JOYPADState joypad = JOYPADState.FromBinaryData(reader);
        DMAState dma = DMAState.FromBinaryData(reader);
        TIMERState timer = TIMERState.FromBinaryData(reader);
        MMUState mmu = MMUState.FromBinaryData(reader);
        APUState apu = APUState.FromBinaryData(reader);

        return new SaveState(cpu, dma, joypad, mmu, ppu, timer, apu);
    }
}
