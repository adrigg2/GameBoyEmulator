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
            case 0x80: ADD(B);         break;   // ADD B
            case 0x81: ADD(C);         break;   // ADD C
            case 0x82: ADD(D);         break;   // ADD D
            case 0x83: ADD(E);         break;   // ADD E
            case 0x84: ADD(H);         break;   // ADD H
            case 0x85: ADD(L);         break;   // ADD L
            case 0x86: ADD(_mmu.ReadByte(HL));         break;   // ADD [HL]
            case 0x87: ADD(A);         break;   // ADD A
            case 0x88: ADC(B);         break;   // ADC B
            case 0x89: ADC(C);         break;   // ADC C
            case 0x8A: ADC(D);         break;   // ADC D
            case 0x8B: ADC(E);         break;   // ADC E
            case 0x8C: ADC(H);         break;   // ADC H
            case 0x8D: ADC(L);         break;   // ADC L
            case 0x8E: ADC(_mmu.ReadByte(HL));         break;   // ADC [HL]
            case 0x8F: ADC(A);         break;   // ADC A
            case 0x90: SUB(B);         break;   // SUB B
            case 0x91: SUB(C);         break;   // SUB C
            case 0x92: SUB(D);         break;   // SUB D
            case 0x93: SUB(E);         break;   // SUB E
            case 0x94: SUB(H); break;   // SUB H
            case 0x95: SUB(L); break;   // SUB L
            case 0x96: SUB(_mmu.ReadByte(HL)); break;   // SUB [HL]
            case 0x97: SUB(A); break;   // SUB A
            case 0x98: SBC(B); break;   // SBC B
            case 0x99: SBC(C); break;   // SBC C
            case 0x9A: SBC(D); break;   // SBC D
            case 0x9B: SBC(E); break;   // SBC E
            case 0x9C: SBC(H); break;   // SBC H
            case 0x9D: SBC(L); break;   // SBC L
            case 0x9E: SBC(_mmu.ReadByte(HL)); break;   // SBC [HL]
            case 0x9F: SBC(A); break;   // SBC A
            case 0xA0: AND(B);         break;   // AND B
            case 0xA1: AND(C); break;   // AND C
            case 0xA2: AND(D); break;   // AND D
            case 0xA3: AND(E); break;   // AND E
            case 0xA4: AND(H); break;   // AND H
            case 0xA5: AND(L); break;   // AND L
            case 0xA6: AND(_mmu.ReadByte(HL)); break;   // AND [HL]
            case 0xA7: AND(A); break;   // AND A
            case 0xA8: XOR(B);         break;   // XOR B
            case 0xA9: XOR(C); break;   // XOR C
            case 0xAA: XOR(D); break;   // XOR D
            case 0xAB: XOR(E); break;   // XOR E
            case 0xAC: XOR(H); break;   // XOR H
            case 0xAD: XOR(L); break;   // XOR L
            case 0xAE: XOR(_mmu.ReadByte(HL)); break;   // XOR [HL]
            case 0xAF: XOR(A); break;   // XOR A
            case 0xB0: OR(B);         break;   // OR B
            case 0xB1: OR(C); break;   // OR C
            case 0xB2: OR(D); break;   // OR D
            case 0xB3: OR(E); break;   // OR E
            case 0xB4: OR(H); break;   // OR H
            case 0xB5: OR(L); break;   // OR L
            case 0xB6: OR(_mmu.ReadByte(HL)); break;   // OR [HL]
            case 0xB7: OR(A); break;   // OR A
            case 0xB8: CP(B);         break;   // CP B
            case 0xB9: CP(C); break;   // CP C
            case 0xBA: CP(D); break;   // CP D
            case 0xBB: CP(E); break;   // CP E
            case 0xBC: CP(H); break;   // CP H
            case 0xBD: CP(L); break;   // CP L
            case 0xBE: CP(_mmu.ReadByte(HL)); break;   // CP [HL]
            case 0xBF: CP(A); break;   // CP A
            case 0xC0:          break;
            case 0xC1:          break;
            case 0xC2:          break;
            case 0xC3:          break;
            case 0xC4:          break;
            case 0xC5:          break;
            case 0xC6: ADD(_mmu.ReadByte(instruction++));         break;   // ADD n8 // TODO: Change the instruction to the proper one
            case 0xC7:          break;
            case 0xC8:          break;
            case 0xC9:          break;
            case 0xCA:          break;
            case 0xCB: ExecutePrefixed(instruction++);          break; // PREFIX // TODO: Change the instruction to the proper one
            case 0xCC:          break;
            case 0xCD:          break;
            case 0xCE: ADC(_mmu.ReadByte(instruction++)); break;   // ADC n8 // TODO: Change the instruction to the proper one
            case 0xCF:          break;
            case 0xD0:          break;
            case 0xD1:          break;
            case 0xD2:          break;
            case 0xD4:          break;
            case 0xD5:          break;
            case 0xD6: SUB(_mmu.ReadByte(instruction++)); break;   // SUB n8 // TODO: Change the instruction to the proper one
            case 0xD7:          break;
            case 0xD8:          break;
            case 0xD9:          break;
            case 0xDA:          break;
            case 0xDC:          break;
            case 0xDE: SBC(_mmu.ReadByte(instruction++)); break;   // SBC n8 // TODO: Change the instruction to the proper one
            case 0xDF:          break;
            case 0xE0:          break;
            case 0xE1:          break;
            case 0xE2:          break;
            case 0xE5:          break;
            case 0xE6: AND(_mmu.ReadByte(instruction++)); break;   // AND n8 // TODO: Change the instruction to the proper one
            case 0xE7:          break;
            case 0xE8:          break;
            case 0xE9:          break;
            case 0xEA:          break;
            case 0xEE: XOR(_mmu.ReadByte(instruction++)); break;   // XOR n8 // TODO: Change the instruction to the proper one
            case 0xEF:          break;
            case 0xF0:          break;
            case 0xF1:          break;
            case 0xF2:          break;
            case 0xF3:          break;
            case 0xF4:          break;
            case 0xF6: OR(_mmu.ReadByte(instruction++)); break;   // OR n8 // TODO: Change the instruction to the proper one
            case 0xF7:          break;
            case 0xF8:          break;
            case 0xF9:          break;
            case 0xFA:          break;
            case 0xFB:          break;
            case 0xFE: CP(_mmu.ReadByte(instruction++)); break;   // CP n8 // TODO: Change the instruction to the proper one
            case 0xFF:          break;
            default:
                throw new ArgumentException("The instruction given is either invalid or not implemented");
        }
    }

    private void ExecutePrefixed(byte instruction)
    {
        switch (instruction)
        {
            case 0x00: B = RLC(B);                              break;
            case 0x01: C = RLC(C);                              break;
            case 0x02: D = RLC(D);                              break;
            case 0x03: E = RLC(E);                              break;
            case 0x04: H = RLC(H);                              break;
            case 0x05: L = RLC(L);                              break;
            case 0x06: _mmu.WriteByte(HL, RLC(_mmu.ReadByte(HL)));   break;
            case 0x07: A = RLC(A);                              break;
            case 0x08: B = RRC(B); break;
            case 0x09: C = RRC(C);                              break;
            case 0x0A: D = RRC(D);                              break;
            case 0x0B: E = RRC(E);                              break;
            case 0x0C: H = RRC(H);                              break;
            case 0x0D: L = RRC(L);                              break;
            case 0x0E: _mmu.WriteByte(HL, RRC(_mmu.ReadByte(HL)));   break;
            case 0x0F: A = RRC(A);                              break;
            case 0x10: B = RL(B); break;
            case 0x11: C = RL(C);                              break;
            case 0x12: D = RL(D);                              break;
            case 0x13: E = RL(E);                              break;
            case 0x14: H = RL(H);                              break;
            case 0x15: L = RL(L);                              break;
            case 0x16: _mmu.WriteByte(HL, RL(_mmu.ReadByte(HL)));   break;
            case 0x17: A = RL(A);                              break;
            case 0x18: B = RR(B); break;
            case 0x19: C = RR(C);                              break;
            case 0x1A: D = RR(D);                              break;
            case 0x1B: E = RR(E);                              break;
            case 0x1C: H = RR(H);                              break;
            case 0x1D: L = RR(L);                              break;
            case 0x1E: _mmu.WriteByte(HL, RR(_mmu.ReadByte(HL)));   break;
            case 0x1F: A = RR(A);                              break;
            case 0x20: B = SLA(B); break;
            case 0x21: C = SLA(C);                              break;
            case 0x22: D = SLA(D);                              break;
            case 0x23: E = SLA(E);                              break;
            case 0x24: H = SLA(H);                              break;
            case 0x25: L = SLA(L);                              break;
            case 0x26: _mmu.WriteByte(HL, SLA(_mmu.ReadByte(HL)));   break;
            case 0x27: A = SLA(A);                              break;
            case 0x28: B = SRA(B); break;
            case 0x29: C = SRA(C);                              break;
            case 0x2A: D = SRA(D);                              break;
            case 0x2B: E = SRA(E);                              break;
            case 0x2C: H = SRA(H);                              break;
            case 0x2D: L = SRA(L);                              break;
            case 0x2E: _mmu.WriteByte(HL, SRA(_mmu.ReadByte(HL)));   break;
            case 0x2F: A = SRA(A);                              break;
            case 0x30: B = SWAP(B); break;
            case 0x31: C = SWAP(C);                              break;
            case 0x32: D = SWAP(D);                              break;
            case 0x33: E = SWAP(E);                              break;
            case 0x34: H = SWAP(H);                              break;
            case 0x35: L = SWAP(L);                              break;
            case 0x36: _mmu.WriteByte(HL, SWAP(_mmu.ReadByte(HL)));   break;
            case 0x37: A = SWAP(A);                              break;
            case 0x38: B = SRL(B); break;
            case 0x39: C = SRL(C);                              break;
            case 0x3A: D = SRL(D);                              break;
            case 0x3B: E = SRL(E);                              break;
            case 0x3C: H = SRL(H);                              break;
            case 0x3D: L = SRL(L);                              break;
            case 0x3E: _mmu.WriteByte(HL, SRL(_mmu.ReadByte(HL)));   break;
            case 0x3F: A = SRL(A);                              break;
            case 0x40: BIT(0x1, B); break;
            case 0x41: BIT(0x1, C); break;
            case 0x42: BIT(0x1, D); break;
            case 0x43: BIT(0x1, E); break;
            case 0x44: BIT(0x1, H); break;
            case 0x45: BIT(0x1, L); break;
            case 0x46: BIT(0x1, _mmu.ReadByte(HL)); break;
            case 0x47: BIT(0x1, A); break;
            case 0x48: BIT(0x2, B); break;
            case 0x49: BIT(0x2, C); break;
            case 0x4A: BIT(0x2, D); break;
            case 0x4B: BIT(0x2, E); break;
            case 0x4C: BIT(0x2, H); break;
            case 0x4D: BIT(0x2, L); break;
            case 0x4E: BIT(0x2, _mmu.ReadByte(HL)); break;
            case 0x4F: BIT(0x2, A); break;
            case 0x50: BIT(0x4, B); break;
            case 0x51: BIT(0x4, C); break;
            case 0x52: BIT(0x4, D); break;
            case 0x53: BIT(0x4, E); break;
            case 0x54: BIT(0x4, H); break;
            case 0x55: BIT(0x4, L); break;
            case 0x56: BIT(0x4, _mmu.ReadByte(HL)); break;
            case 0x57: BIT(0x4, A); break;
            case 0x58: BIT(0x8, B); break;
            case 0x59: BIT(0x8, C); break;
            case 0x5A: BIT(0x8, D); break;
            case 0x5B: BIT(0x8, E); break;
            case 0x5C: BIT(0x8, H); break;
            case 0x5D: BIT(0x8, L); break;
            case 0x5E: BIT(0x8, _mmu.ReadByte(HL)); break;
            case 0x5F: BIT(0x8, A); break;
            case 0x60: BIT(0x10, B); break;
            case 0x61: BIT(0x10, C); break;
            case 0x62: BIT(0x10, D); break;
            case 0x63: BIT(0x10, E); break;
            case 0x64: BIT(0x10, H); break;
            case 0x65: BIT(0x10, L); break;
            case 0x66: BIT(0x10, _mmu.ReadByte(HL)); break;
            case 0x67: BIT(0x10, A); break;
            case 0x68: BIT(0x20, B); break;
            case 0x69: BIT(0x20, C); break;
            case 0x6A: BIT(0x20, D); break;
            case 0x6B: BIT(0x20, E); break;
            case 0x6C: BIT(0x20, H); break;
            case 0x6D: BIT(0x20, L); break;
            case 0x6E: BIT(0x20, _mmu.ReadByte(HL)); break;
            case 0x6F: BIT(0x20, A); break;
            case 0x70: BIT(0x40, B); break;
            case 0x71: BIT(0x40, C); break;
            case 0x72: BIT(0x40, D); break;
            case 0x73: BIT(0x40, E); break;
            case 0x74: BIT(0x40, H); break;
            case 0x75: BIT(0x40, L); break;
            case 0x76: BIT(0x40, _mmu.ReadByte(HL)); break;
            case 0x77: BIT(0x40, A); break;
            case 0x78: BIT(0x80, B); break;
            case 0x79: BIT(0x80, C); break;
            case 0x7A: BIT(0x80, D); break;
            case 0x7B: BIT(0x80, E); break;
            case 0x7C: BIT(0x80, H); break;
            case 0x7D: BIT(0x80, L); break;
            case 0x7E: BIT(0x80, _mmu.ReadByte(HL)); break;
            case 0x7F: BIT(0x80, A); break;
            case 0x80: B = RES(0x1, B); break;
            case 0x81: C = RES(0x1, C); break;
            case 0x82: D = RES(0x1, D); break;
            case 0x83: E = RES(0x1, E); break;
            case 0x84: H = RES(0x1, H); break;
            case 0x85: L = RES(0x1, L); break;
            case 0x86: _mmu.WriteByte(HL, RES(0x1, _mmu.ReadByte(HL))); break;
            case 0x87: A = RES(0x1, A); break;
            case 0x88: B = RES(0x2, B); break;
            case 0x89: C = RES(0x2, C); break;
            case 0x8A: D = RES(0x2, D); break;
            case 0x8B: E = RES(0x2, E); break;
            case 0x8C: H = RES(0x2, H); break;
            case 0x8D: L = RES(0x2, L); break;
            case 0x8E: _mmu.WriteByte(HL, RES(0x2, _mmu.ReadByte(HL))); break;
            case 0x8F: A = RES(0x2, A); break;
            case 0x90: B = RES(0x4, B); break;
            case 0x91: C = RES(0x4, C); break;
            case 0x92: D = RES(0x4, D); break;
            case 0x93: E = RES(0x4, E); break;
            case 0x94: H = RES(0x4, H); break;
            case 0x95: L = RES(0x4, L); break;
            case 0x96: _mmu.WriteByte(HL, RES(0x4, _mmu.ReadByte(HL))); break;
            case 0x97: A = RES(0x4, A); break;
            case 0x98: B = RES(0x8, B); break;
            case 0x99: C = RES(0x8, C); break;
            case 0x9A: D = RES(0x8, D); break;
            case 0x9B: E = RES(0x8, E); break;
            case 0x9C: H = RES(0x8, H); break;
            case 0x9D: L = RES(0x8, L); break;
            case 0x9E: _mmu.WriteByte(HL, RES(0x8, _mmu.ReadByte(HL))); break;
            case 0x9F: A = RES(0x8, A); break;
            case 0xA0: B = RES(0x10, B); break;
            case 0xA1: C = RES(0x10, C); break;
            case 0xA2: D = RES(0x10, D); break;
            case 0xA3: E = RES(0x10, E); break;
            case 0xA4: H = RES(0x10, H); break;
            case 0xA5: L = RES(0x10, L); break;
            case 0xA6: _mmu.WriteByte(HL, RES(0x10, _mmu.ReadByte(HL))); break;
            case 0xA7: A = RES(0x10, A); break;
            case 0xA8: B = RES(0x20, B); break;
            case 0xA9: C = RES(0x20, C); break;
            case 0xAA: D = RES(0x20, D); break;
            case 0xAB: E = RES(0x20, E); break;
            case 0xAC: H = RES(0x20, H); break;
            case 0xAD: L = RES(0x20, L); break;
            case 0xAE: _mmu.WriteByte(HL, RES(0x20, _mmu.ReadByte(HL))); break;
            case 0xAF: A = RES(0x20, A); break;
            case 0xB0: B = RES(0x40, B); break;
            case 0xB1: C = RES(0x40, C); break;
            case 0xB2: D = RES(0x40, D); break;
            case 0xB3: E = RES(0x40, E); break;
            case 0xB4: H = RES(0x40, H); break;
            case 0xB5: L = RES(0x40, L); break;
            case 0xB6: _mmu.WriteByte(HL, RES(0x40, _mmu.ReadByte(HL))); break;
            case 0xB7: A = RES(0x40, A); break;
            case 0xB8: B = RES(0x80, B); break;
            case 0xB9: C = RES(0x80, C); break;
            case 0xBA: D = RES(0x80, D); break;
            case 0xBB: E = RES(0x80, E); break;
            case 0xBC: H = RES(0x80, H); break;
            case 0xBD: L = RES(0x80, L); break;
            case 0xBE: _mmu.WriteByte(HL, RES(0x80, _mmu.ReadByte(HL))); break;
            case 0xBF: A = RES(0x80, A); break;
            case 0xC0: B = SET(0x1, B); break;
            case 0xC1: C = SET(0x1, C); break;
            case 0xC2: D = SET(0x1, D); break;
            case 0xC3: E = SET(0x1, E); break;
            case 0xC4: H = SET(0x1, H); break;
            case 0xC5: L = SET(0x1, L); break;
            case 0xC6: _mmu.WriteByte(HL, SET(0x1, _mmu.ReadByte(HL))); break;
            case 0xC7: A = SET(0x1, A); break;
            case 0xC8: B = SET(0x2, B); break;
            case 0xC9: C = SET(0x2, C); break;
            case 0xCA: D = SET(0x2, D); break;
            case 0xCB: E = SET(0x2, E); break;
            case 0xCC: H = SET(0x2, H); break;
            case 0xCD: L = SET(0x2, L); break;
            case 0xCE: _mmu.WriteByte(HL, SET(0x2, _mmu.ReadByte(HL))); break;
            case 0xCF: A = SET(0x2, A); break;
            case 0xD0: B = SET(0x4, B); break;
            case 0xD1: C = SET(0x4, C); break;
            case 0xD2: D = SET(0x4, D); break;
            case 0xD3: E = SET(0x4, E); break;
            case 0xD4: H = SET(0x4, H); break;
            case 0xD5: L = SET(0x4, L); break;
            case 0xD6: _mmu.WriteByte(HL, SET(0x4, _mmu.ReadByte(HL))); break;
            case 0xD7: A = SET(0x4, A); break;
            case 0xD8: B = SET(0x8, B); break;
            case 0xD9: C = SET(0x8, C); break;
            case 0xDA: D = SET(0x8, D); break;
            case 0xDB: E = SET(0x8, E); break;
            case 0xDC: H = SET(0x8, H); break;
            case 0xDD: L = SET(0x8, L); break;
            case 0xDE: _mmu.WriteByte(HL, SET(0x8, _mmu.ReadByte(HL))); break;
            case 0xDF: A = SET(0x8, A); break;
            case 0xE0: B = SET(0x10, B); break;
            case 0xE1: C = SET(0x10, C); break;
            case 0xE2: D = SET(0x10, D); break;
            case 0xE3: E = SET(0x10, E); break;
            case 0xE4: H = SET(0x10, H); break;
            case 0xE5: L = SET(0x10, L); break;
            case 0xE6: _mmu.WriteByte(HL, SET(0x10, _mmu.ReadByte(HL))); break;
            case 0xE7: A = SET(0x10, A); break;
            case 0xE8: B = SET(0x20, B); break;
            case 0xE9: C = SET(0x20, C); break;
            case 0xEA: D = SET(0x20, D); break;
            case 0xEB: E = SET(0x20, E); break;
            case 0xEC: H = SET(0x20, H); break;
            case 0xED: L = SET(0x20, L); break;
            case 0xEE: _mmu.WriteByte(HL, SET(0x20, _mmu.ReadByte(HL))); break;
            case 0xEF: A = SET(0x20, A); break;
            case 0xF0: B = SET(0x40, B); break;
            case 0xF1: C = SET(0x40, C); break;
            case 0xF2: D = SET(0x40, D); break;
            case 0xF3: E = SET(0x40, E); break;
            case 0xF4: H = SET(0x40, H); break;
            case 0xF5: L = SET(0x40, L); break;
            case 0xF6: _mmu.WriteByte(HL, SET(0x40, _mmu.ReadByte(HL))); break;
            case 0xF7: A = SET(0x40, A); break;
            case 0xF8: B = SET(0x80, B); break;
            case 0xF9: C = SET(0x80, C); break;
            case 0xFA: D = SET(0x80, D); break;
            case 0xFB: E = SET(0x80, E); break;
            case 0xFC: H = SET(0x80, H); break;
            case 0xFD: L = SET(0x80, L); break;
            case 0xFE: _mmu.WriteByte(HL, SET(0x80, _mmu.ReadByte(HL))); break;
            case 0xFF: A = SET(0x80, A); break;
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
