using System.IO;

namespace GameBoyEmulator.SaveState.Components;

public record PPUState(int CycleCount, int WindowY, byte LCDC, byte STAT, byte SCY, byte SCX, byte LY, byte LYC,
                       byte BGP, byte OBP0, byte OBP1, byte WY, byte WX, bool STATInterruptRequest, bool ScreenOff,
                       byte[] ScreenBuffer, byte[] BgColorIds, List<ushort> ObjectPool)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(CycleCount);
        writer.Write(WindowY);
        writer.Write(LCDC);
        writer.Write(STAT);
        writer.Write(SCY);
        writer.Write(SCX);
        writer.Write(LY);
        writer.Write(LYC);
        writer.Write(BGP);
        writer.Write(OBP0);
        writer.Write(OBP1);
        writer.Write(WY);
        writer.Write(WX);
        writer.Write(STATInterruptRequest);
        writer.Write(ScreenOff);
        writer.Write(ScreenBuffer);
        writer.Write(BgColorIds);
        writer.Write(ObjectPool.Count);

        foreach (ushort obj in ObjectPool)
        {
            writer.Write(obj);
        }
    }

    public static PPUState FromBinaryData(BinaryReader reader)
    {
        int CycleCount = reader.ReadInt32();
        int WindowY = reader.ReadInt32();
        byte LCDC = reader.ReadByte();
        byte STAT = reader.ReadByte();
        byte SCY = reader.ReadByte();
        byte SCX = reader.ReadByte();
        byte LY = reader.ReadByte();
        byte LYC = reader.ReadByte();
        byte BGP = reader.ReadByte();
        byte OBP0 = reader.ReadByte();
        byte OBP1 = reader.ReadByte();
        byte WY = reader.ReadByte();
        byte WX = reader.ReadByte();
        bool STATInterruptRequest = reader.ReadBoolean();
        bool ScreenOff = reader.ReadBoolean();
        byte[] ScreenBuffer = reader.ReadBytes(144 * 160 / 4);
        byte[] BgColorIds = reader.ReadBytes(160);

        int objectCount = reader.ReadInt32();
        List<ushort> ObjectPool = [];
        for (int i = 0; i < objectCount; i++)
        {
            ObjectPool.Add(reader.ReadUInt16());
        }

        return new PPUState(CycleCount, WindowY, LCDC, STAT, SCY, SCX, LY, LYC, BGP, OBP0, OBP1, WY, WX, STATInterruptRequest, ScreenOff, ScreenBuffer, BgColorIds, ObjectPool);
    }
}
