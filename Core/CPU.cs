using System.Numerics;

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

    public void Execute()
    {
        byte instruction = 0x00;

        switch (instruction)
        {
            case 0x00:          break;          // NOP
            case 0x01:          break;
            case 0x02:          break;
            case 0x03: BC++;    break;          // INC BC
            case 0x04: B = INC(B);         break;   // INC B
            case 0x05: B = DEC(B);         break;   // DEC B
            case 0x06:          break;
            case 0x07: A = RLC(A); ZeroFlag = false;         break;   // RLCA
            case 0x08:          break;
            case 0x09: ADDHL(BC);         break; // ADDHL BC
            case 0x0A:          break;
            case 0x0B: BC--;         break; // DEC BC
            case 0x0C: C = INC(C);          break;  // INC C
            case 0x0D: C = DEC(C);         break;   // DEC C
            case 0x0E:          break;
            case 0x0F: A = RRC(A); ZeroFlag = false;        break;  // RRCA
            case 0x10:          break;
            case 0x11:          break;
            case 0x12:          break;
            case 0x13: DE++;         break;     // INC DE
            case 0x14: D = INC(D);         break;   // INC D
            case 0x15: D = DEC(D);         break;   // DEC D
            case 0x16:          break;
            case 0x17: A = RL(A); ZeroFlag = false;         break;  // RLA
            case 0x18:          break;
            case 0x19: ADDHL(DE);         break;    // ADDHL DE
            case 0x1A:          break;
            case 0x1B: DE--;         break;         // DEC DE
            case 0x1C: E = INC(E);         break;   // INC E
            case 0x1D: E = DEC(E);         break;   // DEC E
            case 0x1E:          break;
            case 0x1F: A = RR(A); ZeroFlag = false;         break;  // RRA
            case 0x20:          break;
            case 0x21:          break;
            case 0x22:          break;
            case 0x23: HL++;         break; // INC HL
            case 0x24: H = INC(H);         break;   // INC H
            case 0x25: H = DEC(H);         break;   // DEC H
            case 0x26:          break;
            case 0x27:          break;
            case 0x28:          break;
            case 0x29: ADDHL(HL);         break;    // ADDHL HL
            case 0x2A:          break;
            case 0x2B: HL--;         break; // DEC HL
            case 0x2C: L = INC(L);         break;   // INC L
            case 0x2D: L = DEC(L);         break;   // DEC L
            case 0x2E:          break;
            case 0x2F:          break;
            case 0x30:          break;
            case 0x31:          break;
            case 0x32:          break;
            case 0x33: _sp++;         break;    // INC SP
            case 0x34: _mmu.WriteByte(HL, INC(_mmu.ReadByte(HL)));         break;   // INC [HL]
            case 0x35: _mmu.WriteByte(HL, DEC(_mmu.ReadByte(HL)));         break;   // DEC [HL]
            case 0x36:          break;
            case 0x37:          break;
            case 0x38:          break;
            case 0x39: ADDHL(_sp);         break;   // ADDHL SP
            case 0x3A:          break;
            case 0x3B: _sp--;         break;    // DEC SP
            case 0x3C: A = INC(A);         break;   // INC A
            case 0x3D: A = DEC(A);         break;   // DEC A
            case 0x3E:          break;
            case 0x3F:          break;
            case 0x40:          break;
            case 0x41:          break;
            case 0x42:          break;
            case 0x43:          break;
            case 0x44:          break;
            case 0x45:          break;
            case 0x46:          break;
            case 0x47:          break;
            case 0x48:          break;
            case 0x49:          break;
            case 0x4A:          break;
            case 0x4B:          break;
            case 0x4C:          break;
            case 0x4D:          break;
            case 0x4E:          break;
            case 0x4F:          break;
            case 0x50:          break;
            case 0x51:          break;
            case 0x52:          break;
            case 0x53:          break;
            case 0x54:          break;
            case 0x55:          break;
            case 0x56:          break;
            case 0x57:          break;
            case 0x58:          break;
            case 0x59:          break;
            case 0x5A:          break;
            case 0x5B:          break;
            case 0x5C:          break;
            case 0x5D:          break;
            case 0x5E:          break;
            case 0x5F:          break;
            case 0x60:          break;
            case 0x61:          break;
            case 0x62:          break;
            case 0x63:          break;
            case 0x64:          break;
            case 0x65:          break;
            case 0x66:          break;
            case 0x67:          break;
            case 0x68:          break;
            case 0x69:          break;
            case 0x6A:          break;
            case 0x6B:          break;
            case 0x6C:          break;
            case 0x6D:          break;
            case 0x6E:          break;
            case 0x6F:          break;
            case 0x70:          break;
            case 0x71:          break;
            case 0x72:          break;
            case 0x73:          break;
            case 0x74:          break;
            case 0x75:          break;
            case 0x76:          break;
            case 0x77:          break;
            case 0x78:          break;
            case 0x79:          break;
            case 0x7A:          break;
            case 0x7B:          break;
            case 0x7C:          break;
            case 0x7D:          break;
            case 0x7E:          break;
            case 0x7F:          break;
            case 0x80:          break;
            case 0x81:          break;
            case 0x82:          break;
            case 0x83:          break;
            case 0x84:          break;
            case 0x85:          break;
            case 0x86:          break;
            case 0x87:          break;
            case 0x88:          break;
            case 0x89:          break;
            case 0x8A:          break;
            case 0x8B:          break;
            case 0x8C:          break;
            case 0x8D:          break;
            case 0x8E:          break;
            case 0x8F:          break;
            case 0x90:          break;
            case 0x91:          break;
            case 0x92:          break;
            case 0x93:          break;
            case 0x94:          break;
            case 0x95:          break;
            case 0x96:          break;
            case 0x97:          break;
            case 0x98:          break;
            case 0x99:          break;
            case 0x9A:          break;
            case 0x9B:          break;
            case 0x9C:          break;
            case 0x9D:          break;
            case 0x9E:          break;
            case 0x9F:          break;
            case 0xA0:          break;
            case 0xA1:          break;
            case 0xA2:          break;
            case 0xA3:          break;
            case 0xA4:          break;
            case 0xA5:          break;
            case 0xA6:          break;
            case 0xA7:          break;
            case 0xA8:          break;
            case 0xA9:          break;
            case 0xAA:          break;
            case 0xAB:          break;
            case 0xAC:          break;
            case 0xAD:          break;
            case 0xAE:          break;
            case 0xAF:          break;
            case 0xB0:          break;
            case 0xB1:          break;
            case 0xB2:          break;
            case 0xB3:          break;
            case 0xB4:          break;
            case 0xB5:          break;
            case 0xB6:          break;
            case 0xB7:          break;
            case 0xB8:          break;
            case 0xB9:          break;
            case 0xBA:          break;
            case 0xBB:          break;
            case 0xBC:          break;
            case 0xBD:          break;
            case 0xBE:          break;
            case 0xBF:          break;
            case 0xC0:          break;
            case 0xC1:          break;
            case 0xC2:          break;
            case 0xC3:          break;
            case 0xC4:          break;
            case 0xC5:          break;
            case 0xC6:          break;
            case 0xC7:          break;
            case 0xC8:          break;
            case 0xC9:          break;
            case 0xCA:          break;
            case 0xCB: ExecutePrefixed(instruction++);          break; // PREFIX // TODO: Change the instruction to the proper one
            case 0xCC:          break;
            case 0xCD:          break;
            case 0xCE:          break;
            case 0xCF:          break;
            case 0xD0:          break;
            case 0xD1:          break;
            case 0xD2:          break;
            case 0xD4:          break;
            case 0xD5:          break;
            case 0xD6:          break;
            case 0xD7:          break;
            case 0xD8:          break;
            case 0xD9:          break;
            case 0xDA:          break;
            case 0xDC:          break;
            case 0xDE:          break;
            case 0xDF:          break;
            case 0xE0:          break;
            case 0xE1:          break;
            case 0xE2:          break;
            case 0xE5:          break;
            case 0xE6:          break;
            case 0xE7:          break;
            case 0xE8:          break;
            case 0xE9:          break;
            case 0xEA:          break;
            case 0xEE:          break;
            case 0xEF:          break;
            case 0xF0:          break;
            case 0xF1:          break;
            case 0xF2:          break;
            case 0xF3:          break;
            case 0xF4:          break;
            case 0xF6:          break;
            case 0xF7:          break;
            case 0xF8:          break;
            case 0xF9:          break;
            case 0xFA:          break;
            case 0xFB:          break;
            case 0xFE:          break;
            case 0xFF:          break;
            default:
                throw new ArgumentException("The instruction given is either invalid or not implemented");
        }
    }

    private void ExecutePrefixed(byte instruction)
    {
        switch (instruction)
        {
            case 0x00: break;
            case 0x01: break;
            case 0x02: break;
            case 0x03: break;
            case 0x04: break;
            case 0x05: break;
            case 0x06: break;
            case 0x07: break;
            case 0x08: break;
            case 0x09: break;
            case 0x0A: break;
            case 0x0B: break;
            case 0x0C: break;
            case 0x0D: break;
            case 0x0E: break;
            case 0x0F: break;
            case 0x10: break;
            case 0x11: break;
            case 0x12: break;
            case 0x13: break;
            case 0x14: break;
            case 0x15: break;
            case 0x16: break;
            case 0x17: break;
            case 0x18: break;
            case 0x19: break;
            case 0x1A: break;
            case 0x1B: break;
            case 0x1C: break;
            case 0x1D: break;
            case 0x1E: break;
            case 0x1F: break;
            case 0x20: break;
            case 0x21: break;
            case 0x22: break;
            case 0x23: break;
            case 0x24: break;
            case 0x25: break;
            case 0x26: break;
            case 0x27: break;
            case 0x28: break;
            case 0x29: break;
            case 0x2A: break;
            case 0x2B: break;
            case 0x2C: break;
            case 0x2D: break;
            case 0x2E: break;
            case 0x2F: break;
            case 0x30: break;
            case 0x31: break;
            case 0x32: break;
            case 0x33: break;
            case 0x34: break;
            case 0x35: break;
            case 0x36: break;
            case 0x37: break;
            case 0x38: break;
            case 0x39: break;
            case 0x3A: break;
            case 0x3B: break;
            case 0x3C: break;
            case 0x3D: break;
            case 0x3E: break;
            case 0x3F: break;
            case 0x40: break;
            case 0x41: break;
            case 0x42: break;
            case 0x43: break;
            case 0x44: break;
            case 0x45: break;
            case 0x46: break;
            case 0x47: break;
            case 0x48: break;
            case 0x49: break;
            case 0x4A: break;
            case 0x4B: break;
            case 0x4C: break;
            case 0x4D: break;
            case 0x4E: break;
            case 0x4F: break;
            case 0x50: break;
            case 0x51: break;
            case 0x52: break;
            case 0x53: break;
            case 0x54: break;
            case 0x55: break;
            case 0x56: break;
            case 0x57: break;
            case 0x58: break;
            case 0x59: break;
            case 0x5A: break;
            case 0x5B: break;
            case 0x5C: break;
            case 0x5D: break;
            case 0x5E: break;
            case 0x5F: break;
            case 0x60: break;
            case 0x61: break;
            case 0x62: break;
            case 0x63: break;
            case 0x64: break;
            case 0x65: break;
            case 0x66: break;
            case 0x67: break;
            case 0x68: break;
            case 0x69: break;
            case 0x6A: break;
            case 0x6B: break;
            case 0x6C: break;
            case 0x6D: break;
            case 0x6E: break;
            case 0x6F: break;
            case 0x70: break;
            case 0x71: break;
            case 0x72: break;
            case 0x73: break;
            case 0x74: break;
            case 0x75: break;
            case 0x76: break;
            case 0x77: break;
            case 0x78: break;
            case 0x79: break;
            case 0x7A: break;
            case 0x7B: break;
            case 0x7C: break;
            case 0x7D: break;
            case 0x7E: break;
            case 0x7F: break;
            case 0x80: break;
            case 0x81: break;
            case 0x82: break;
            case 0x83: break;
            case 0x84: break;
            case 0x85: break;
            case 0x86: break;
            case 0x87: break;
            case 0x88: break;
            case 0x89: break;
            case 0x8A: break;
            case 0x8B: break;
            case 0x8C: break;
            case 0x8D: break;
            case 0x8E: break;
            case 0x8F: break;
            case 0x90: break;
            case 0x91: break;
            case 0x92: break;
            case 0x93: break;
            case 0x94: break;
            case 0x95: break;
            case 0x96: break;
            case 0x97: break;
            case 0x98: break;
            case 0x99: break;
            case 0x9A: break;
            case 0x9B: break;
            case 0x9C: break;
            case 0x9D: break;
            case 0x9E: break;
            case 0x9F: break;
            case 0xA0: break;
            case 0xA1: break;
            case 0xA2: break;
            case 0xA3: break;
            case 0xA4: break;
            case 0xA5: break;
            case 0xA6: break;
            case 0xA7: break;
            case 0xA8: break;
            case 0xA9: break;
            case 0xAA: break;
            case 0xAB: break;
            case 0xAC: break;
            case 0xAD: break;
            case 0xAE: break;
            case 0xAF: break;
            case 0xB0: break;
            case 0xB1: break;
            case 0xB2: break;
            case 0xB3: break;
            case 0xB4: break;
            case 0xB5: break;
            case 0xB6: break;
            case 0xB7: break;
            case 0xB8: break;
            case 0xB9: break;
            case 0xBA: break;
            case 0xBB: break;
            case 0xBC: break;
            case 0xBD: break;
            case 0xBE: break;
            case 0xBF: break;
            case 0xC0: break;
            case 0xC1: break;
            case 0xC2: break;
            case 0xC3: break;
            case 0xC4: break;
            case 0xC5: break;
            case 0xC6: break;
            case 0xC7: break;
            case 0xC8: break;
            case 0xC9: break;
            case 0xCA: break;
            case 0xCB: break;
            case 0xCC: break;
            case 0xCD: break;
            case 0xCE: break;
            case 0xCF: break;
            case 0xD0: break;
            case 0xD1: break;
            case 0xD2: break;
            case 0xD4: break;
            case 0xD5: break;
            case 0xD6: break;
            case 0xD7: break;
            case 0xD8: break;
            case 0xD9: break;
            case 0xDA: break;
            case 0xDC: break;
            case 0xDE: break;
            case 0xDF: break;
            case 0xE0: break;
            case 0xE1: break;
            case 0xE2: break;
            case 0xE5: break;
            case 0xE6: break;
            case 0xE7: break;
            case 0xE8: break;
            case 0xE9: break;
            case 0xEA: break;
            case 0xEE: break;
            case 0xEF: break;
            case 0xF0: break;
            case 0xF1: break;
            case 0xF2: break;
            case 0xF3: break;
            case 0xF4: break;
            case 0xF6: break;
            case 0xF7: break;
            case 0xF8: break;
            case 0xF9: break;
            case 0xFA: break;
            case 0xFB: break;
            case 0xFE: break;
            case 0xFF: break;
            default:
                throw new ArgumentException("The instruction given is either invalid or not implemented");
        }
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
        SetHalfCarryFlag(HL, num);
        SubtractionFlag = false;
        HL = (ushort)result;
    }

    private void ADC (byte num)
    {
        int result = _a + num + (CarryFlag ? 1 : 0);
        SetZeroFlag(result);
        SetCarryFlag(result);
        SetHalfCarryFlagC(_a, num);
        SubtractionFlag = false;
        _a = (byte)result;
    }

    private void SUB(byte num)
    {
        int result = _a - num;
        SetZeroFlag(result);
        SetCarryFlag(result);
        SetHalfCarryFlagSub(_a, num);
        SubtractionFlag = true;
        _a = (byte)result;
    }

    private void SBC(byte num)
    {
        int result = _a - num - (CarryFlag ? 1 : 0);
        SetZeroFlag(result);
        SetCarryFlag(result);
        SetHalfCarryFlagSubC(_a, num);
        SubtractionFlag = true;
        _a = (byte)result;
    }

    private void AND(byte num)
    {
        int result = _a & num;
        SetZeroFlag(result);
        CarryFlag = false;
        HalfCarryFlag = true;
        SubtractionFlag = false;
        _a = (byte)result;
    }

    private void OR(byte num)
    {
        int result = _a | num;
        SetZeroFlag(result);
        CarryFlag = false;
        HalfCarryFlag = false;
        SubtractionFlag = false;
        _a = (byte)result;
    }
    
    private void XOR(byte num)
    {
        int result = _a ^ num;
        SetZeroFlag(result);
        CarryFlag = false;
        HalfCarryFlag = false;
        SubtractionFlag = false;
        _a = (byte)result;
    }

    private void CP(byte num)
    {
        int result = _a - num;
        SetZeroFlag(result);
        SetCarryFlag(result);
        SetHalfCarryFlagSub(_a, num);
        SubtractionFlag = true;
    }

    private byte INC(byte num)
    {
        int result = num + 1;
        SetZeroFlag(result);
        SetHalfCarryFlag(num, 1);
        SubtractionFlag = false;
        return (byte)result;
    }

    private byte DEC(byte num)
    {
        int result = num - 1;
        SetZeroFlag(result);
        SetHalfCarryFlagSub(_a, num);
        SubtractionFlag = true;
        return (byte)result;
    }

    private void CCF()
    {
        HalfCarryFlag = false;
        SubtractionFlag = false;
        CarryFlag = !CarryFlag;
    }

    private void SCF()
    {
        HalfCarryFlag = false;
        SubtractionFlag = false;
        CarryFlag = true;
    }

    private byte RR(byte num)
    {
        byte result = (byte)((num >> 1) | (CarryFlag ? 0x80 : 0));
        SetZeroFlag(result);
        HalfCarryFlag = false;
        SubtractionFlag = false;
        CarryFlag = (num & 0x1) != 0;
        return result;
    }

    private byte RL(byte num)
    {
        byte result = (byte)((num << 1) | (CarryFlag ? 0x1 : 0));
        SetZeroFlag(result);
        HalfCarryFlag = false;
        SubtractionFlag = false;
        CarryFlag = (num & 0x80) != 0;
        return result;
    }

    private byte RRC(byte num)
    {
        byte result = (byte)((num >> 1) | (num << 7));
        SetZeroFlag(result);
        HalfCarryFlag = false;
        SubtractionFlag = false;
        CarryFlag = (num & 0x1) != 0;
        return result;
    }

    private byte RLC(byte num)
    {
        byte result = (byte)((num << 1) | (num >> 7));
        SetZeroFlag(result);
        HalfCarryFlag = false;
        SubtractionFlag = false;
        CarryFlag = (num & 0x80) != 0;
        return result;
    }

    private byte CPL(byte num)
    {
        SubtractionFlag = true;
        HalfCarryFlag = true;
        return (byte)~num;
    }

    private void BIT(byte pos, byte num)
    {
        ZeroFlag = (num & pos) != 0;
        SubtractionFlag = false;
        HalfCarryFlag = true;
    }

    private byte RES(byte pos, byte num)
    {
        return (byte)(num & ~pos);
    }

    private byte SET(byte pos, byte num)
    {
        return (byte)(num | pos);
    }

    private byte SRL(byte num)
    {
        byte result = (byte)(num >> 1);
        SetZeroFlag(result);
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = (num & 0x1) != 0;
        return result;
    }

    private byte SRA(byte num)
    {
        byte result = (byte)((num >> 1) | (num & 0x80));
        SetZeroFlag(result);
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = (num & 0x1) != 0;
        return result;
    }

    private byte SLA(byte num)
    {
        byte result = (byte)(num << 1);
        SetZeroFlag(result);
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = (num & 0x80) != 0;
        return result;
    }

    private byte SWAP(byte num)
    {
        byte result = (byte)(((num & 0xF0) >> 4) | ((num & 0xF) << 4));
        SetZeroFlag(result);
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = false;
        return result;
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

    private void SetHalfCarryFlagC(byte num1, byte num2)
    {
        HalfCarryFlag = ((num1 & 0xF) + (num2 & 0xF) + (CarryFlag ? 1 : 0)) > 0xF;
    }

    private void SetHalfCarryFlagSub(byte num1, byte num2)
    {
        HalfCarryFlag = (num1 & 0xF) < (num2 & 0xF);
    }
    private void SetHalfCarryFlagSubC(byte num1, byte num2)
    {
        HalfCarryFlag = (num1 & 0xF) < ((num2 & 0xF) + (CarryFlag ? 1 : 0));
    }
}
