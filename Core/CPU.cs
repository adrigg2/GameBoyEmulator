namespace GameBoyEmulator.Core;
public class CPU
{
    private MMU _mmu;

    private byte _a, _b, _c, _d, _e, _h, _l, _f;

    private ushort _pc, _sp;

    public byte A { get => _a; set => _a = value; }
    public byte B { get => _b; set => _b = value; }
    public byte C { get => _c; set => _c = value; }
    public byte D { get => _d; set => _d = value; }
    public byte E { get => _e; set => _e = value; }
    public byte H { get => _h; set => _h = value; }
    public byte L { get => _l; set => _l = value; }
    public byte F { get => _f; set => _f = value; }

    public ushort AF { get => (ushort)((_a << 8) | _f); set { _a = (byte)(value >> 8); _f = (byte)(value & 0xFF); } }
    public ushort BC { get => (ushort)((_b << 8) | _c); set { _b = (byte)(value >> 8); _c = (byte)(value & 0xFF); } }
    public ushort DE { get => (ushort)((_d << 8) | _e); set { _d = (byte)(value >> 8); _e = (byte)(value & 0xFF); } }
    public ushort HL { get => (ushort)((_h << 8) | _l); set { _h = (byte)(value >> 8); _l = (byte)(value & 0xFF); } }

    public bool ZeroFlag { get => (_f & 0x80) != 0; set => _f = (byte)(_f & 0x7F | (value ? 0x80 : 0)); }
    public bool SubtractionFlag { get => (_f & 0x40) != 0; set => _f = (byte)(_f & 0xBF | (value ? 0x40 : 0)); }
    public bool HalfCarryFlag { get => (_f & 0x20) != 0; set => _f = (byte)(_f & 0xDF | (value ? 0x20 : 0)); }
    public bool CarryFlag { get => (_f & 0x10) != 0; set => _f = (byte)(_f & 0xEF | (value ? 0x10 : 0)); }

    public CPU(MMU mmu)
    {
        _a = _b = _c = _d = _e = _h = _l = _f = 0;
        _mmu = mmu;
    }

    private void ADD(byte num)
    {
        int result = _a + num;
        SetZeroFlag(result);
        SetCarryFlag(result);
        SetHalfCarryFlag(_a, num);
        SubtractionFlag = false;
        _a = (byte)result;
    }

    private void ADDHL(ushort num)
    {
        int result = HL + num;
        SetZeroFlag16(result);
        SetCarryFlag16(result);
        HalfCarryFlag = false;
        SubtractionFlag = false;
        HL = (ushort)result;
    }

    private void ADC (byte num)
    {
        int result = _a + num + (CarryFlag ? 1 : 0);
        SetZeroFlag(result);
        SetCarryFlag(result);
        SetHalfCarryFlag(_a, num);
        SubtractionFlag = false;
        _a = (byte)result;
    }

    private void SetZeroFlag(int result)
    {
        ZeroFlag = (result & 0xFF) == 0;
    }

    private void SetZeroFlag16(int result)
    {
        ZeroFlag = (result & 0xFFFF) == 0;
    }

    private void SetCarryFlag(int result)
    {
        CarryFlag = (result >> 8) != 0;
    }

    private void SetCarryFlag16 (int result)
    {
        CarryFlag = (result >> 16) != 0;
    }

    private void SetHalfCarryFlag(byte num1, byte num2)
    {
        HalfCarryFlag = ((num1 & 0xF) + (num2 & 0xF)) > 0xF;
    }

    private void SetHalfCarryFlag(ushort num1, ushort num2)
    {
        HalfCarryFlag = ((num1 & 0xFFF) + (num2 & 0xFFF)) > 0xFFF;
    }
}
