using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record MBCState(int ROMBank, int SRAMBank, bool RAMEnabled, byte[] HeaderCheck, byte[]? AdditionalRegisters, byte[]? SRAM)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(ROMBank);
        writer.Write(SRAMBank);
        writer.Write(RAMEnabled);

        if (AdditionalRegisters != null)
        {
            writer.Write(AdditionalRegisters.Length);
            writer.Write(AdditionalRegisters);
        }
        else
        {
            writer.Write(0);
        }

        if (SRAM != null)
        {
            writer.Write(SRAM.Length);
            writer.Write(SRAM);
        }
        else
        {
            writer.Write(0);
        }
    }

    public static MBCState FromBinaryData(BinaryReader reader, byte[] headerCheck)
    {
        int ROMBank = reader.ReadInt32();
        int SRAMBank = reader.ReadInt32();
        bool RAMEnabled = reader.ReadBoolean();

        int registersLength = reader.ReadInt32();
        byte[]? additionalRegisters = null;
        if (registersLength > 0)
        {
            additionalRegisters = reader.ReadBytes(registersLength);
        }

        int sramLength = reader.ReadInt32();
        byte[]? sram = null;
        if (sramLength > 0)
        {
            sram = reader.ReadBytes(sramLength);
        }

        return new MBCState(
            ROMBank,
            SRAMBank,
            RAMEnabled,
            headerCheck,
            additionalRegisters,
            sram
            );
    }
}
