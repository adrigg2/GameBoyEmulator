using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record MMUState(bool BootRomMapped, byte IE, byte IF, byte[] WRAM, byte[] VRAM, byte[] HRAM, byte[] OAM, MBCState Cartridge)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(BootRomMapped);
        writer.Write(IE);
        writer.Write(IF);
        writer.Write(WRAM);
        writer.Write(VRAM);
        writer.Write(HRAM);
        writer.Write(OAM);
        Cartridge.Write(writer);
    }

    public static MMUState FromBinaryData(BinaryReader reader, byte[] headerCheck)
    {
        return new MMUState(
            reader.ReadBoolean(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadBytes(0x2000),
            reader.ReadBytes(0x2000),
            reader.ReadBytes(0x7F),
            reader.ReadBytes(0xA0),
            MBCState.FromBinaryData(reader, headerCheck)
            );
    }
}
