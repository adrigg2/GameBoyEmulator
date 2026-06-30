using System.Windows.Media.Animation;

namespace GameBoyEmulator.Core;
public class CPU
{
    private MMU _mmu;

    private byte _a, _b, _c, _d, _e, _h, _l, _f;

    private ushort _pc, _sp;

    private bool _ime, _halted, _haltBug;
    // private bool _stopped;   // TODO: Stop

    private bool _eiPending;

    public byte A { get => _a; set => _a = value; }
    public byte B { get => _b; set => _b = value; }
    public byte C { get => _c; set => _c = value; }
    public byte D { get => _d; set => _d = value; }
    public byte E { get => _e; set => _e = value; }
    public byte H { get => _h; set => _h = value; }
    public byte L { get => _l; set => _l = value; }
    public byte F { get => _f; set => _f = value; }

    public ushort AF { get => (ushort)((_a << 8) | _f); set { _a = (byte)(value >> 8); _f = (byte)(value & 0xF0); } }
    public ushort BC { get => (ushort)((_b << 8) | _c); set { _b = (byte)(value >> 8); _c = (byte)(value & 0xFF); } }
    public ushort DE { get => (ushort)((_d << 8) | _e); set { _d = (byte)(value >> 8); _e = (byte)(value & 0xFF); } }
    public ushort HL { get => (ushort)((_h << 8) | _l); set { _h = (byte)(value >> 8); _l = (byte)(value & 0xFF); } }

    public bool ZeroFlag { get => (_f & 0x80) != 0; set => _f = (byte)(_f & 0x7F | (value ? 0x80 : 0)); }
    public bool SubtractionFlag { get => (_f & 0x40) != 0; set => _f = (byte)(_f & 0xBF | (value ? 0x40 : 0)); }
    public bool HalfCarryFlag { get => (_f & 0x20) != 0; set => _f = (byte)(_f & 0xDF | (value ? 0x20 : 0)); }
    public bool CarryFlag { get => (_f & 0x10) != 0; set => _f = (byte)(_f & 0xEF | (value ? 0x10 : 0)); }

    public byte _lastInstruction;  // DEBUG: Debug only
    public ushort _lastInstructionPC; // DEBUG: Debug only

    public CPU(MMU mmu)
    {
        _a = _b = _c = _d = _e = _h = _l = _f = 0;
        _pc = _sp = 0;
        _ime = _halted = _haltBug = _eiPending = false;
        _mmu = mmu;
    }

    public int Execute()
    {
        // if (_stopped) return;    // TODO: STOP
        _lastInstructionPC = _pc;

        int cycles = 0;
        if (_ime || _halted)
        {
            if ((_mmu.IE & _mmu.IF) != 0)
            {
                cycles += HandleInterrupt();
            }
        }

        if (_halted)
        {
            return cycles + 4; // Cycles advance while CPU is halted
        }

        byte instruction = _mmu.ReadByte(_pc++);
        cycles += CPUCycles.Cycles[instruction];

        if (_haltBug)
        {
            _pc--;
            _haltBug = false;
        }

        switch (instruction)
        {
            case 0x00:                                                      break;  // NOP
            case 0x01: BC = _mmu.ReadWord(_pc); _pc += 2;                   break;  // LD BC, n16
            case 0x02: _mmu.WriteByte(BC, A);                               break;  // LD [BC], A
            case 0x03: BC++;                                                break;  // INC BC
            case 0x04: B = INC(B);                                          break;  // INC B
            case 0x05: B = DEC(B);                                          break;  // DEC B
            case 0x06: B = _mmu.ReadByte(_pc++);                            break;  // LD B, n8
            case 0x07:                                                              // RLCA
                F = 0;
                CarryFlag = (A & 0x80) != 0;
                A = (byte)((A << 1) | (A >> 7));
                break;
            case 0x08: _mmu.WriteWord(_mmu.ReadWord(_pc), _sp); _pc += 2;   break;  // LD [a16], SP
            case 0x09: ADDHL(BC);                                           break;  // ADDHL BC
            case 0x0A: A = _mmu.ReadByte(BC);                               break;  // LD A, [BC]
            case 0x0B: BC--;                                                break;  // DEC BC
            case 0x0C: C = INC(C);                                          break;  // INC C
            case 0x0D: C = DEC(C);                                          break;  // DEC C
            case 0x0E: C = _mmu.ReadByte(_pc++);                            break;  // LD C, n8
            case 0x0F:                                                              // RRCA
                F = 0;
                CarryFlag = (A & 0x1) != 0;
                A = (byte)((A >> 1) | (A << 7));
                break;
            case 0x10:                                                      break;  // TODO: STOP
            case 0x11: DE = _mmu.ReadWord(_pc); _pc += 2;                   break;  // LD DE, n16
            case 0x12: _mmu.WriteByte(DE, A);                               break;  // LD [DE], A
            case 0x13: DE++;                                                break;  // INC DE
            case 0x14: D = INC(D);                                          break;  // INC D
            case 0x15: D = DEC(D);                                          break;  // DEC D
            case 0x16: D = _mmu.ReadByte(_pc++);                            break;  // LD D, n8
            case 0x17:                                                              // RLA
            {
                bool carry = CarryFlag;
                F = 0;
                CarryFlag = (A & 0x80) != 0;
                A = (byte)((A << 1) | (carry ? 0x1 : 0));
                break;
            } 
            case 0x18: cycles += JR(true, (sbyte)_mmu.ReadByte(_pc++));     break;  // JR e8
            case 0x19: ADDHL(DE);                                           break;  // ADDHL DE
            case 0x1A: A = _mmu.ReadByte(DE);                               break;  // LD A, [DE]
            case 0x1B: DE--;                                                break;  // DEC DE
            case 0x1C: E = INC(E);                                          break;  // INC E
            case 0x1D: E = DEC(E);                                          break;  // DEC E
            case 0x1E: E = _mmu.ReadByte(_pc++);                            break;  // LD E, n8
            case 0x1F:                                                              // RRA
            {
                bool carry = CarryFlag;
                F = 0;
                CarryFlag = (A & 0x1) != 0;
                A = (byte)((A >> 1) | (carry ? 0x80 : 0));
                break;
            }
            case 0x20: cycles += JR(!ZeroFlag, (sbyte)_mmu.ReadByte(_pc++));break;  // JR NZ, e8
            case 0x21: HL = _mmu.ReadWord(_pc); _pc += 2;                   break;  // LD HL, n16
            case 0x22: _mmu.WriteByte(HL++, A);                             break;  // LD [HL+], A
            case 0x23: HL++;                                                break;  // INC HL
            case 0x24: H = INC(H);                                          break;  // INC H
            case 0x25: H = DEC(H);                                          break;  // DEC H
            case 0x26: H = _mmu.ReadByte(_pc++);                            break;  // LD H, n8
            case 0x27:                                                              // DAA
                int adj = 0;
                if (SubtractionFlag)
                {
                    adj += HalfCarryFlag ? 0x6 : 0;
                    adj += CarryFlag ? 0x60 : 0;
                    A -= (byte)adj;
                }
                else
                {
                    adj += HalfCarryFlag || ((A & 0xF) > 0x9) ? 0x6 : 0;
                    if (CarryFlag || (A > 0x99))
                    {
                        adj += 0x60;
                        CarryFlag = true;
                    }
                    A += (byte)adj;
                }
                SetZeroFlag(A);
                HalfCarryFlag = false;
                break;
            case 0x28: cycles += JR(ZeroFlag, (sbyte)_mmu.ReadByte(_pc++)); break;  // JR Z, e8
            case 0x29: ADDHL(HL);                                           break;  // ADDHL HL
            case 0x2A: A = _mmu.ReadByte(HL++);                             break;  // LD A, [HL+]
            case 0x2B: HL--;                                                break;  // DEC HL
            case 0x2C: L = INC(L);                                          break;  // INC L
            case 0x2D: L = DEC(L);                                          break;  // DEC L
            case 0x2E: L = _mmu.ReadByte(_pc++);                            break;  // LD L, n8
            case 0x2F:                                                              // CPL
                A = (byte)~A;
                SubtractionFlag = true;
                HalfCarryFlag = true;
                break;
            case 0x30: cycles +=JR(!CarryFlag, (sbyte)_mmu.ReadByte(_pc++));break;  // JR NC, e8
            case 0x31: _sp = _mmu.ReadWord(_pc); _pc += 2;                  break;  // LD SP, n16
            case 0x32: _mmu.WriteByte(HL--, A);                             break;  // LD [HL-], A
            case 0x33: _sp++;                                               break;  // INC SP
            case 0x34: _mmu.WriteByte(HL, INC(_mmu.ReadByte(HL)));          break;  // INC [HL]
            case 0x35: _mmu.WriteByte(HL, DEC(_mmu.ReadByte(HL)));          break;  // DEC [HL]
            case 0x36: _mmu.WriteByte(HL, _mmu.ReadByte(_pc++));            break;  // LD H, n8
            case 0x37:                                                              // SCF
                HalfCarryFlag = false;
                SubtractionFlag = false; 
                CarryFlag = true; 
                break;
            case 0x38: cycles += JR(CarryFlag, (sbyte)_mmu.ReadByte(_pc++));break;  // JR C, e8
            case 0x39: ADDHL(_sp);                                          break;  // ADDHL SP
            case 0x3A: A = _mmu.ReadByte(HL--);                             break;  // LD A, [HL-]
            case 0x3B: _sp--;                                               break;  // DEC SP
            case 0x3C: A = INC(A);                                          break;  // INC A
            case 0x3D: A = DEC(A);                                          break;  // DEC A
            case 0x3E: A = _mmu.ReadByte(_pc++);                            break;  // LD A, n8
            case 0x3F:                                                              // CCF
                HalfCarryFlag = false;
                SubtractionFlag = false;
                CarryFlag = !CarryFlag; 
                break;
            case 0x40:                                                      break;  // LD B, B
            case 0x41: B = C;                                               break;  // LD B, C
            case 0x42: B = D;                                               break;  // LD B, D
            case 0x43: B = E;                                               break;  // LD B, E
            case 0x44: B = H;                                               break;  // LD B, H
            case 0x45: B = L;                                               break;  // LD B, L
            case 0x46: B = _mmu.ReadByte(HL);                               break;  // LD B, [HL]
            case 0x47: B = A;                                               break;  // LD B, A
            case 0x48: C = B;                                               break;  // LD C, B
            case 0x49:                                                      break;  // LD C, C
            case 0x4A: C = D;                                               break;  // LD C, D
            case 0x4B: C = E;                                               break;  // LD C, E
            case 0x4C: C = H;                                               break;  // LD C, H
            case 0x4D: C = L;                                               break;  // LD C, L
            case 0x4E: C = _mmu.ReadByte(HL);                               break;  // LD C, [HL]
            case 0x4F: C = A;                                               break;  // LD C, A
            case 0x50: D = B;                                               break;  // LD D, B
            case 0x51: D = C;                                               break;  // LD D, C
            case 0x52:                                                      break;  // LD D, D
            case 0x53: D = E;                                               break;  // LD D, E
            case 0x54: D = H;                                               break;  // LD D, H
            case 0x55: D = L;                                               break;  // LD D, L
            case 0x56: D = _mmu.ReadByte(HL);                               break;  // LD D, [HL]
            case 0x57: D = A;                                               break;  // LD D, A
            case 0x58: E = B;                                               break;  // LD E, B
            case 0x59: E = C;                                               break;  // LD E, C
            case 0x5A: E = D;                                               break;  // LD E, D
            case 0x5B:                                                      break;  // LD E, E
            case 0x5C: E = H;                                               break;  // LD E, H
            case 0x5D: E = L;                                               break;  // LD E, L
            case 0x5E: E = _mmu.ReadByte(HL);                               break;  // LD E, [HL]
            case 0x5F: E = A;                                               break;  // LD E, A
            case 0x60: H = B;                                               break;  // LD H, B
            case 0x61: H = C;                                               break;  // LD H, C
            case 0x62: H = D;                                               break;  // LD H, D
            case 0x63: H = E;                                               break;  // LD H, E
            case 0x64:                                                      break;  // LD H, H
            case 0x65: H = L;                                               break;  // LD H, L
            case 0x66: H = _mmu.ReadByte(HL);                               break;  // LD H, [HL]
            case 0x67: H = A;                                               break;  // LD H, A
            case 0x68: L = B;                                               break;  // LD L, B
            case 0x69: L = C;                                               break;  // LD L, C
            case 0x6A: L = D;                                               break;  // LD L, D
            case 0x6B: L = E;                                               break;  // LD L, E
            case 0x6C: L = H;                                               break;  // LD L, H
            case 0x6D:                                                      break;  // LD L, L
            case 0x6E: L = _mmu.ReadByte(HL);                               break;  // LD L, [HL]
            case 0x6F: L = A;                                               break;  // LD L, A
            case 0x70: _mmu.WriteByte(HL, B);                               break;  // LD [HL], B
            case 0x71: _mmu.WriteByte(HL, C);                               break;  // LD [HL], C
            case 0x72: _mmu.WriteByte(HL, D);                               break;  // LD [HL], D
            case 0x73: _mmu.WriteByte(HL, E);                               break;  // LD [HL], E
            case 0x74: _mmu.WriteByte(HL, H);                               break;  // LD [HL], H
            case 0x75: _mmu.WriteByte(HL, L);                               break;  // LD [HL], L
            case 0x76:                                                              // HALT
                if (_ime)
                {
                    _halted = true;
                }
                else
                {
                    if ((_mmu.IE & _mmu.IF & 0x1F) == 0)
                    {
                        _halted = true;
                    }
                    else
                    {
                        _haltBug = true;
                    }
                }
                break;  
            case 0x77: _mmu.WriteByte(HL, A);                               break;  // LD [HL], A
            case 0x78: A = B;                                               break;  // LD A, B
            case 0x79: A = C;                                               break;  // LD A, C
            case 0x7A: A = D;                                               break;  // LD A, D
            case 0x7B: A = E;                                               break;  // LD A, E
            case 0x7C: A = H;                                               break;  // LD A, H
            case 0x7D: A = L;                                               break;  // LD A, L
            case 0x7E: A = _mmu.ReadByte(HL);                               break;  // LD A, [HL]
            case 0x7F:                                                      break;  // LD A, A
            case 0x80: ADD(B);                                              break;  // ADD B
            case 0x81: ADD(C);                                              break;  // ADD C
            case 0x82: ADD(D);                                              break;  // ADD D
            case 0x83: ADD(E);                                              break;  // ADD E
            case 0x84: ADD(H);                                              break;  // ADD H
            case 0x85: ADD(L);                                              break;  // ADD L
            case 0x86: ADD(_mmu.ReadByte(HL));                              break;  // ADD [HL]
            case 0x87: ADD(A);                                              break;  // ADD A
            case 0x88: ADC(B);                                              break;  // ADC B
            case 0x89: ADC(C);                                              break;  // ADC C
            case 0x8A: ADC(D);                                              break;  // ADC D
            case 0x8B: ADC(E);                                              break;  // ADC E
            case 0x8C: ADC(H);                                              break;  // ADC H
            case 0x8D: ADC(L);                                              break;  // ADC L
            case 0x8E: ADC(_mmu.ReadByte(HL));                              break;  // ADC [HL]
            case 0x8F: ADC(A);                                              break;  // ADC A
            case 0x90: SUB(B);                                              break;  // SUB B
            case 0x91: SUB(C);                                              break;  // SUB C
            case 0x92: SUB(D);                                              break;  // SUB D
            case 0x93: SUB(E);                                              break;  // SUB E
            case 0x94: SUB(H);                                              break;  // SUB H
            case 0x95: SUB(L);                                              break;  // SUB L
            case 0x96: SUB(_mmu.ReadByte(HL));                              break;  // SUB [HL]
            case 0x97: SUB(A);                                              break;  // SUB A
            case 0x98: SBC(B);                                              break;  // SBC B
            case 0x99: SBC(C);                                              break;  // SBC C
            case 0x9A: SBC(D);                                              break;  // SBC D
            case 0x9B: SBC(E);                                              break;  // SBC E
            case 0x9C: SBC(H);                                              break;  // SBC H
            case 0x9D: SBC(L);                                              break;  // SBC L
            case 0x9E: SBC(_mmu.ReadByte(HL));                              break;  // SBC [HL]
            case 0x9F: SBC(A);                                              break;  // SBC A
            case 0xA0: AND(B);                                              break;  // AND B
            case 0xA1: AND(C);                                              break;  // AND C
            case 0xA2: AND(D);                                              break;  // AND D
            case 0xA3: AND(E);                                              break;  // AND E
            case 0xA4: AND(H);                                              break;  // AND H
            case 0xA5: AND(L);                                              break;  // AND L
            case 0xA6: AND(_mmu.ReadByte(HL));                              break;  // AND [HL]
            case 0xA7: AND(A);                                              break;  // AND A
            case 0xA8: XOR(B);                                              break;  // XOR B
            case 0xA9: XOR(C);                                              break;  // XOR C
            case 0xAA: XOR(D);                                              break;  // XOR D
            case 0xAB: XOR(E);                                              break;  // XOR E
            case 0xAC: XOR(H);                                              break;  // XOR H
            case 0xAD: XOR(L);                                              break;  // XOR L
            case 0xAE: XOR(_mmu.ReadByte(HL));                              break;  // XOR [HL]
            case 0xAF: XOR(A);                                              break;  // XOR A
            case 0xB0: OR(B);                                               break;  // OR B
            case 0xB1: OR(C);                                               break;  // OR C
            case 0xB2: OR(D);                                               break;  // OR D
            case 0xB3: OR(E);                                               break;  // OR E
            case 0xB4: OR(H);                                               break;  // OR H
            case 0xB5: OR(L);                                               break;  // OR L
            case 0xB6: OR(_mmu.ReadByte(HL));                               break;  // OR [HL]
            case 0xB7: OR(A);                                               break;  // OR A
            case 0xB8: CP(B);                                               break;  // CP B
            case 0xB9: CP(C);                                               break;  // CP C
            case 0xBA: CP(D);                                               break;  // CP D
            case 0xBB: CP(E);                                               break;  // CP E
            case 0xBC: CP(H);                                               break;  // CP H
            case 0xBD: CP(L);                                               break;  // CP L
            case 0xBE: CP(_mmu.ReadByte(HL));                               break;  // CP [HL]
            case 0xBF: CP(A);                                               break;  // CP A
            case 0xC0: cycles += RET(!ZeroFlag);                            break;  // RET NZ
            case 0xC1: BC = _mmu.ReadWord(_sp); _sp += 2;                   break;  // POP BC
            case 0xC2: cycles += JP(!ZeroFlag, _mmu.ReadWord(_pc));         break;  // JP NZ, a16
            case 0xC3: cycles += JP(true, _mmu.ReadWord(_pc));              break;  // JP a16
            case 0xC4: cycles += CALL(!ZeroFlag, _mmu.ReadWord(_pc));       break;  // CALL NZ, a16
            case 0xC5: _sp -= 2; _mmu.WriteWord(_sp, BC);                   break;  // PUSH BC
            case 0xC6: ADD(_mmu.ReadByte(_pc++));                           break;  // ADD n8
            case 0xC7: RST(0x0);                                            break;  // RST $00
            case 0xC8: cycles += RET(ZeroFlag);                             break;  // RET Z
            case 0xC9: cycles += RET(true);                                 break;  // RET
            case 0xCA: cycles += JP(ZeroFlag, _mmu.ReadWord(_pc));          break;  // JP Z, a16
            case 0xCB: cycles += ExecutePrefixed(_mmu.ReadByte(_pc++));     break;  // PREFIX
            case 0xCC: cycles += CALL(ZeroFlag, _mmu.ReadWord(_pc));        break;  // CALL Z, a16
            case 0xCD: cycles += CALL(true, _mmu.ReadWord(_pc));            break;  // CALL a16
            case 0xCE: ADC(_mmu.ReadByte(_pc++));                           break;  // ADC n8
            case 0xCF: RST(0x8);                                            break;  // RST $08
            case 0xD0: cycles += RET(!CarryFlag);                           break;  // RET NC
            case 0xD1: DE = _mmu.ReadWord(_sp); _sp += 2;                   break;  // POP DE
            case 0xD2: cycles += JP(!CarryFlag, _mmu.ReadWord(_pc));        break;  // JP NC, a16
            case 0xD4: cycles += CALL(!CarryFlag, _mmu.ReadWord(_pc));      break;  // CALL NC, a16
            case 0xD5: _sp -= 2; _mmu.WriteWord(_sp, DE);                   break;  // PUSH DE
            case 0xD6: SUB(_mmu.ReadByte(_pc++));                           break;  // SUB n8
            case 0xD7: RST(0x10);                                           break;  // RST $10
            case 0xD8: cycles += RET(CarryFlag);                            break;  // RET C
            case 0xD9: cycles += RET(true); _ime = true;                    break;  // RETI
            case 0xDA: cycles += JP(CarryFlag, _mmu.ReadWord(_pc));         break;  // JP C, a16
            case 0xDC: cycles += CALL(CarryFlag, _mmu.ReadWord(_pc));       break;  // CALL C, a16
            case 0xDE: SBC(_mmu.ReadByte(_pc++));                           break;  // SBC n8
            case 0xDF: RST(0x18);                                           break;  // RST $18
            case 0xE0:                                                              // LDH [a8], A
                _mmu.WriteByte((ushort)(0xFF00 + _mmu.ReadByte(_pc++)), A);
                break;
            case 0xE1: HL = _mmu.ReadWord(_sp); _sp += 2;                   break;  // POP HL
            case 0xE2:                                                              // LDH [C], A
                _mmu.WriteByte(_mmu.ReadByte((ushort)(0xFF00 + C)), A); 
                break;
            case 0xE5: _sp -= 2; _mmu.WriteWord(_sp, HL);                   break;  // PUSH HL
            case 0xE6: AND(_mmu.ReadByte(_pc++));                           break;  // AND n8
            case 0xE7: RST(0x20);                                           break;  // RST $20
            case 0xE8: _sp = ADDr16e8(_sp);                                 break;  // ADD SP, e8
            case 0xE9: cycles += JP(true, HL);                              break;  // JP HL
            case 0xEA: _mmu.WriteByte(_mmu.ReadWord(_pc), A); _pc += 2;     break;  // LD [a16], A
            case 0xEE: XOR(_mmu.ReadByte(_pc++));                           break;  // XOR n8
            case 0xEF: RST(0x28);                                           break;  // RST $28
            case 0xF0:                                                              // LDH A, [a8]
                A = _mmu.ReadByte((ushort)(0xFF00 + _mmu.ReadByte(_pc++)));         
                break;
            case 0xF1: AF = _mmu.ReadWord(_sp); _sp += 2;                   break;  // POP AF
            case 0xF2: A = _mmu.ReadByte((ushort)(0xFF00 + C));             break;  // LDH A, [C]
            case 0xF3: _ime = false;                                        break;  // DI
            case 0xF5: _sp -= 2; _mmu.WriteWord(_sp, AF);                   break;  // PUSH AF
            case 0xF6: OR(_mmu.ReadByte(_pc++));                            break;  // OR n8
            case 0xF7: RST(0x30);                                           break;  // RST $30
            case 0xF8: HL = ADDr16e8(_sp);                                  break;  // LD HL, SP + e8
            case 0xF9: _sp = HL;                                            break;  // LD SP, HL
            case 0xFA: A = _mmu.ReadByte(_mmu.ReadWord(_pc)); _pc += 2;     break;  // LD A, [a16]
            case 0xFB: _eiPending = true;                                      break;  // EI
            case 0xFE: CP(_mmu.ReadByte(_pc++));                            break;  // CP n8
            case 0xFF: RST(0x38);                                           break;  // RST $38
            default:
                throw new ArgumentException("The instruction given is either invalid or not implemented");

        }

        if (_eiPending)
        {
            _eiPending = false;
            _ime = true;
        }

        _lastInstruction = instruction; // DEBUG: debug only
        return cycles;
    }

    // TODO: Implement Interrupts
    public int HandleInterrupt()
    {
        if (_halted)
        {
            _halted = false;
        }

        if (!_ime)
        {
            return 0;
        }

        if ((_mmu.IE & _mmu.IF & 0x1) != 0)
        {
            _ime = false;
            CALL(true, 0x40);
            _mmu.IF &= 0xFE;
            return 20;
        }
        else if ((_mmu.IE & _mmu.IF & 0x2) != 0)
        {
            _ime = false;
            CALL(true, 0x48);
            _mmu.IF &= 0xFD;
            return 20;
        }
        else if ((_mmu.IE & _mmu.IF & 0x4) != 0)
        {
            _ime = false;
            CALL(true, 0x50);
            _mmu.IF &= 0xFB;
            return 20;
        }
        else if ((_mmu.IE & _mmu.IF & 0x8) != 0)
        {
            _ime = false;
            CALL(true, 0x58);
            _mmu.IF &= 0xF7;
            return 20;
        }
        else if ((_mmu.IE & _mmu.IF & 0x10) != 0)
        {
            _ime = false;
            CALL(true, 0x60);
            _mmu.IF &= 0xEF;
            return 20;
        }

        return 0;
    }

    private int ExecutePrefixed(byte instruction)
    {
        switch (instruction)
        {
            case 0x00: B = RLC(B);                                          break;  // RLC B
            case 0x01: C = RLC(C);                                          break;  // RLC C
            case 0x02: D = RLC(D);                                          break;  // RLC D
            case 0x03: E = RLC(E);                                          break;  // RLC E
            case 0x04: H = RLC(H);                                          break;  // RLC H
            case 0x05: L = RLC(L);                                          break;  // RLC L
            case 0x06: _mmu.WriteByte(HL, RLC(_mmu.ReadByte(HL)));          break;  // RLC [HL]
            case 0x07: A = RLC(A);                                          break;  // RLC A
            case 0x08: B = RRC(B);                                          break;  // RRC B
            case 0x09: C = RRC(C);                                          break;  // RRC C
            case 0x0A: D = RRC(D);                                          break;  // RRC D
            case 0x0B: E = RRC(E);                                          break;  // RRC E
            case 0x0C: H = RRC(H);                                          break;  // RRC H
            case 0x0D: L = RRC(L);                                          break;  // RRC L
            case 0x0E: _mmu.WriteByte(HL, RRC(_mmu.ReadByte(HL)));          break;  // RRC [HL]
            case 0x0F: A = RRC(A);                                          break;  // RRC A
            case 0x10: B = RL(B);                                           break;  // RL B
            case 0x11: C = RL(C);                                           break;  // RL C
            case 0x12: D = RL(D);                                           break;  // RL D
            case 0x13: E = RL(E);                                           break;  // RL E
            case 0x14: H = RL(H);                                           break;  // RL H
            case 0x15: L = RL(L);                                           break;  // RL L
            case 0x16: _mmu.WriteByte(HL, RL(_mmu.ReadByte(HL)));           break;  // RL [HL]
            case 0x17: A = RL(A);                                           break;  // RL A
            case 0x18: B = RR(B);                                           break;  // RR B
            case 0x19: C = RR(C);                                           break;  // RR C
            case 0x1A: D = RR(D);                                           break;  // RR D
            case 0x1B: E = RR(E);                                           break;  // RR E
            case 0x1C: H = RR(H);                                           break;  // RR H
            case 0x1D: L = RR(L);                                           break;  // RR L
            case 0x1E: _mmu.WriteByte(HL, RR(_mmu.ReadByte(HL)));           break;  // RR [HL]
            case 0x1F: A = RR(A);                                           break;  // RR A
            case 0x20: B = SLA(B);                                          break;  // SLA B
            case 0x21: C = SLA(C);                                          break;  // SLA C
            case 0x22: D = SLA(D);                                          break;  // SLA D
            case 0x23: E = SLA(E);                                          break;  // SLA E
            case 0x24: H = SLA(H);                                          break;  // SLA H
            case 0x25: L = SLA(L);                                          break;  // SLA L
            case 0x26: _mmu.WriteByte(HL, SLA(_mmu.ReadByte(HL)));          break;  // SLA [HL]
            case 0x27: A = SLA(A);                                          break;  // SLA A
            case 0x28: B = SRA(B);                                          break;  // SRA B
            case 0x29: C = SRA(C);                                          break;  // SRA C
            case 0x2A: D = SRA(D);                                          break;  // SRA D
            case 0x2B: E = SRA(E);                                          break;  // SRA E
            case 0x2C: H = SRA(H);                                          break;  // SRA H
            case 0x2D: L = SRA(L);                                          break;  // SRA L
            case 0x2E: _mmu.WriteByte(HL, SRA(_mmu.ReadByte(HL)));          break;  // SRA [HL]
            case 0x2F: A = SRA(A);                                          break;  // SRA A
            case 0x30: B = SWAP(B);                                         break;  // SWAP B
            case 0x31: C = SWAP(C);                                         break;  // SWAP C
            case 0x32: D = SWAP(D);                                         break;  // SWAP D
            case 0x33: E = SWAP(E);                                         break;  // SWAP E
            case 0x34: H = SWAP(H);                                         break;  // SWAP H
            case 0x35: L = SWAP(L);                                         break;  // SWAP L
            case 0x36: _mmu.WriteByte(HL, SWAP(_mmu.ReadByte(HL)));         break;  // SWAP [HL]
            case 0x37: A = SWAP(A);                                         break;  // SWAP A
            case 0x38: B = SRL(B);                                          break;  // SRL B
            case 0x39: C = SRL(C);                                          break;  // SRL C
            case 0x3A: D = SRL(D);                                          break;  // SRL D
            case 0x3B: E = SRL(E);                                          break;  // SRL E
            case 0x3C: H = SRL(H);                                          break;  // SRL H
            case 0x3D: L = SRL(L);                                          break;  // SRL L
            case 0x3E: _mmu.WriteByte(HL, SRL(_mmu.ReadByte(HL)));          break;  // SRL [HL]
            case 0x3F: A = SRL(A);                                          break;  // SRL A
            case 0x40: BIT(0x1, B);                                         break;  // BIT 0, B
            case 0x41: BIT(0x1, C);                                         break;  // BIT 0, C
            case 0x42: BIT(0x1, D);                                         break;  // BIT 0, D
            case 0x43: BIT(0x1, E);                                         break;  // BIT 0, E
            case 0x44: BIT(0x1, H);                                         break;  // BIT 0, H
            case 0x45: BIT(0x1, L);                                         break;  // BIT 0, L
            case 0x46: BIT(0x1, _mmu.ReadByte(HL));                         break;  // BIT 0, [HL]
            case 0x47: BIT(0x1, A);                                         break;  // BIT 0, A
            case 0x48: BIT(0x2, B);                                         break;  // BIT 1, B
            case 0x49: BIT(0x2, C);                                         break;  // BIT 1, C
            case 0x4A: BIT(0x2, D);                                         break;  // BIT 1, D
            case 0x4B: BIT(0x2, E);                                         break;  // BIT 1, E
            case 0x4C: BIT(0x2, H);                                         break;  // BIT 1, H
            case 0x4D: BIT(0x2, L);                                         break;  // BIT 1, L
            case 0x4E: BIT(0x2, _mmu.ReadByte(HL));                         break;  // BIT 1, [HL]
            case 0x4F: BIT(0x2, A);                                         break;  // BIT 1, A
            case 0x50: BIT(0x4, B);                                         break;  // BIT 2, B
            case 0x51: BIT(0x4, C);                                         break;  // BIT 2, C
            case 0x52: BIT(0x4, D);                                         break;  // BIT 2, D
            case 0x53: BIT(0x4, E);                                         break;  // BIT 2, E
            case 0x54: BIT(0x4, H);                                         break;  // BIT 2, H
            case 0x55: BIT(0x4, L);                                         break;  // BIT 2, L
            case 0x56: BIT(0x4, _mmu.ReadByte(HL));                         break;  // BIT 2, [HL]
            case 0x57: BIT(0x4, A);                                         break;  // BIT 2, A
            case 0x58: BIT(0x8, B);                                         break;  // BIT 3, B
            case 0x59: BIT(0x8, C);                                         break;  // BIT 3, C
            case 0x5A: BIT(0x8, D);                                         break;  // BIT 3, D
            case 0x5B: BIT(0x8, E);                                         break;  // BIT 3, E
            case 0x5C: BIT(0x8, H);                                         break;  // BIT 3, H
            case 0x5D: BIT(0x8, L);                                         break;  // BIT 3, L
            case 0x5E: BIT(0x8, _mmu.ReadByte(HL));                         break;  // BIT 3, [HL]
            case 0x5F: BIT(0x8, A);                                         break;  // BIT 3, A
            case 0x60: BIT(0x10, B);                                        break;  // BIT 4, B
            case 0x61: BIT(0x10, C);                                        break;  // BIT 4, C
            case 0x62: BIT(0x10, D);                                        break;  // BIT 4, D
            case 0x63: BIT(0x10, E);                                        break;  // BIT 4, E
            case 0x64: BIT(0x10, H);                                        break;  // BIT 4, H
            case 0x65: BIT(0x10, L);                                        break;  // BIT 4, L
            case 0x66: BIT(0x10, _mmu.ReadByte(HL));                        break;  // BIT 4, [HL]
            case 0x67: BIT(0x10, A);                                        break;  // BIT 4, A
            case 0x68: BIT(0x20, B);                                        break;  // BIT 5, B
            case 0x69: BIT(0x20, C);                                        break;  // BIT 5, C
            case 0x6A: BIT(0x20, D);                                        break;  // BIT 5, D
            case 0x6B: BIT(0x20, E);                                        break;  // BIT 5, E
            case 0x6C: BIT(0x20, H);                                        break;  // BIT 5, H
            case 0x6D: BIT(0x20, L);                                        break;  // BIT 5, L
            case 0x6E: BIT(0x20, _mmu.ReadByte(HL));                        break;  // BIT 5, [HL]
            case 0x6F: BIT(0x20, A);                                        break;  // BIT 5, A
            case 0x70: BIT(0x40, B);                                        break;  // BIT 6, B
            case 0x71: BIT(0x40, C);                                        break;  // BIT 6, C
            case 0x72: BIT(0x40, D);                                        break;  // BIT 6, D
            case 0x73: BIT(0x40, E);                                        break;  // BIT 6, E
            case 0x74: BIT(0x40, H);                                        break;  // BIT 6, H
            case 0x75: BIT(0x40, L);                                        break;  // BIT 6, L
            case 0x76: BIT(0x40, _mmu.ReadByte(HL));                        break;  // BIT 6, [HL]
            case 0x77: BIT(0x40, A);                                        break;  // BIT 6, A
            case 0x78: BIT(0x80, B);                                        break;  // BIT 7, B
            case 0x79: BIT(0x80, C);                                        break;  // BIT 7, C
            case 0x7A: BIT(0x80, D);                                        break;  // BIT 7, D
            case 0x7B: BIT(0x80, E);                                        break;  // BIT 7, E
            case 0x7C: BIT(0x80, H);                                        break;  // BIT 7, H
            case 0x7D: BIT(0x80, L);                                        break;  // BIT 7, L
            case 0x7E: BIT(0x80, _mmu.ReadByte(HL));                        break;  // BIT 7, [HL]
            case 0x7F: BIT(0x80, A);                                        break;  // BIT 7, A
            case 0x80: B = RES(0x1, B);                                     break;  // RES 0, B
            case 0x81: C = RES(0x1, C);                                     break;  // RES 0, C
            case 0x82: D = RES(0x1, D);                                     break;  // RES 0, D
            case 0x83: E = RES(0x1, E);                                     break;  // RES 0, E
            case 0x84: H = RES(0x1, H);                                     break;  // RES 0, H
            case 0x85: L = RES(0x1, L);                                     break;  // RES 0, L
            case 0x86: _mmu.WriteByte(HL, RES(0x1, _mmu.ReadByte(HL)));     break;  // RES 0, [HL]
            case 0x87: A = RES(0x1, A);                                     break;  // RES 0, A
            case 0x88: B = RES(0x2, B);                                     break;  // RES 1, B
            case 0x89: C = RES(0x2, C);                                     break;  // RES 1, C
            case 0x8A: D = RES(0x2, D);                                     break;  // RES 1, D
            case 0x8B: E = RES(0x2, E);                                     break;  // RES 1, E
            case 0x8C: H = RES(0x2, H);                                     break;  // RES 1, H
            case 0x8D: L = RES(0x2, L);                                     break;  // RES 1, L
            case 0x8E: _mmu.WriteByte(HL, RES(0x2, _mmu.ReadByte(HL)));     break;  // RES 1, [HL]
            case 0x8F: A = RES(0x2, A);                                     break;  // RES 1, A
            case 0x90: B = RES(0x4, B);                                     break;  // RES 2, B
            case 0x91: C = RES(0x4, C);                                     break;  // RES 2, C
            case 0x92: D = RES(0x4, D);                                     break;  // RES 2, D
            case 0x93: E = RES(0x4, E);                                     break;  // RES 2, E
            case 0x94: H = RES(0x4, H);                                     break;  // RES 2, H
            case 0x95: L = RES(0x4, L);                                     break;  // RES 2, L
            case 0x96: _mmu.WriteByte(HL, RES(0x4, _mmu.ReadByte(HL)));     break;  // RES 2, [HL]
            case 0x97: A = RES(0x4, A);                                     break;  // RES 2, A
            case 0x98: B = RES(0x8, B);                                     break;  // RES 3, B
            case 0x99: C = RES(0x8, C);                                     break;  // RES 3, C
            case 0x9A: D = RES(0x8, D);                                     break;  // RES 3, D
            case 0x9B: E = RES(0x8, E);                                     break;  // RES 3, E
            case 0x9C: H = RES(0x8, H);                                     break;  // RES 3, H
            case 0x9D: L = RES(0x8, L);                                     break;  // RES 3, L
            case 0x9E: _mmu.WriteByte(HL, RES(0x8, _mmu.ReadByte(HL)));     break;  // RES 3, [HL]
            case 0x9F: A = RES(0x8, A);                                     break;  // RES 3, A
            case 0xA0: B = RES(0x10, B);                                    break;  // RES 4, B
            case 0xA1: C = RES(0x10, C);                                    break;  // RES 4, C
            case 0xA2: D = RES(0x10, D);                                    break;  // RES 4, D
            case 0xA3: E = RES(0x10, E);                                    break;  // RES 4, E
            case 0xA4: H = RES(0x10, H);                                    break;  // RES 4, H
            case 0xA5: L = RES(0x10, L);                                    break;  // RES 4, L
            case 0xA6: _mmu.WriteByte(HL, RES(0x10, _mmu.ReadByte(HL)));    break;  // RES 4, [HL]
            case 0xA7: A = RES(0x10, A);                                    break;  // RES 4, A
            case 0xA8: B = RES(0x20, B);                                    break;  // RES 5, B
            case 0xA9: C = RES(0x20, C);                                    break;  // RES 5, C
            case 0xAA: D = RES(0x20, D);                                    break;  // RES 5, D
            case 0xAB: E = RES(0x20, E);                                    break;  // RES 5, E
            case 0xAC: H = RES(0x20, H);                                    break;  // RES 5, H
            case 0xAD: L = RES(0x20, L);                                    break;  // RES 5, L
            case 0xAE: _mmu.WriteByte(HL, RES(0x20, _mmu.ReadByte(HL)));    break;  // RES 5, [HL]
            case 0xAF: A = RES(0x20, A);                                    break;  // RES 5, A
            case 0xB0: B = RES(0x40, B);                                    break;  // RES 6, B
            case 0xB1: C = RES(0x40, C);                                    break;  // RES 6, C
            case 0xB2: D = RES(0x40, D);                                    break;  // RES 6, D
            case 0xB3: E = RES(0x40, E);                                    break;  // RES 6, E
            case 0xB4: H = RES(0x40, H);                                    break;  // RES 6, H
            case 0xB5: L = RES(0x40, L);                                    break;  // RES 6, L
            case 0xB6: _mmu.WriteByte(HL, RES(0x40, _mmu.ReadByte(HL)));    break;  // RES 6, [HL]
            case 0xB7: A = RES(0x40, A);                                    break;  // RES 6, A
            case 0xB8: B = RES(0x80, B);                                    break;  // RES 7, B
            case 0xB9: C = RES(0x80, C);                                    break;  // RES 7, C
            case 0xBA: D = RES(0x80, D);                                    break;  // RES 7, D
            case 0xBB: E = RES(0x80, E);                                    break;  // RES 7, E
            case 0xBC: H = RES(0x80, H);                                    break;  // RES 7, H
            case 0xBD: L = RES(0x80, L);                                    break;  // RES 7, L
            case 0xBE: _mmu.WriteByte(HL, RES(0x80, _mmu.ReadByte(HL)));    break;  // RES 7, [HL]
            case 0xBF: A = RES(0x80, A);                                    break;  // RES 7, A
            case 0xC0: B = SET(0x1, B);                                     break;  // SET 0, B
            case 0xC1: C = SET(0x1, C);                                     break;  // SET 0, C
            case 0xC2: D = SET(0x1, D);                                     break;  // SET 0, D
            case 0xC3: E = SET(0x1, E);                                     break;  // SET 0, E
            case 0xC4: H = SET(0x1, H);                                     break;  // SET 0, H
            case 0xC5: L = SET(0x1, L);                                     break;  // SET 0, L
            case 0xC6: _mmu.WriteByte(HL, SET(0x1, _mmu.ReadByte(HL)));     break;  // SET 0, [HL]
            case 0xC7: A = SET(0x1, A);                                     break;  // SET 0, A
            case 0xC8: B = SET(0x2, B);                                     break;  // SET 1, B
            case 0xC9: C = SET(0x2, C);                                     break;  // SET 1, C
            case 0xCA: D = SET(0x2, D);                                     break;  // SET 1, D
            case 0xCB: E = SET(0x2, E);                                     break;  // SET 1, E
            case 0xCC: H = SET(0x2, H);                                     break;  // SET 1, H
            case 0xCD: L = SET(0x2, L);                                     break;  // SET 1, L
            case 0xCE: _mmu.WriteByte(HL, SET(0x2, _mmu.ReadByte(HL)));     break;  // SET 1, [HL]
            case 0xCF: A = SET(0x2, A);                                     break;  // SET 1, A
            case 0xD0: B = SET(0x4, B);                                     break;  // SET 2, B
            case 0xD1: C = SET(0x4, C);                                     break;  // SET 2, C
            case 0xD2: D = SET(0x4, D);                                     break;  // SET 2, D
            case 0xD3: E = SET(0x4, E);                                     break;  // SET 2, E
            case 0xD4: H = SET(0x4, H);                                     break;  // SET 2, H
            case 0xD5: L = SET(0x4, L);                                     break;  // SET 2, L
            case 0xD6: _mmu.WriteByte(HL, SET(0x4, _mmu.ReadByte(HL)));     break;  // SET 2, [HL]
            case 0xD7: A = SET(0x4, A);                                     break;  // SET 2, A
            case 0xD8: B = SET(0x8, B);                                     break;  // SET 3, B
            case 0xD9: C = SET(0x8, C);                                     break;  // SET 3, C
            case 0xDA: D = SET(0x8, D);                                     break;  // SET 3, D
            case 0xDB: E = SET(0x8, E);                                     break;  // SET 3, E
            case 0xDC: H = SET(0x8, H);                                     break;  // SET 3, H
            case 0xDD: L = SET(0x8, L);                                     break;  // SET 3, L
            case 0xDE: _mmu.WriteByte(HL, SET(0x8, _mmu.ReadByte(HL)));     break;  // SET 3, [HL]
            case 0xDF: A = SET(0x8, A);                                     break;  // SET 3, A
            case 0xE0: B = SET(0x10, B);                                    break;  // SET 4, B
            case 0xE1: C = SET(0x10, C);                                    break;  // SET 4, C
            case 0xE2: D = SET(0x10, D);                                    break;  // SET 4, D
            case 0xE3: E = SET(0x10, E);                                    break;  // SET 4, E
            case 0xE4: H = SET(0x10, H);                                    break;  // SET 4, H
            case 0xE5: L = SET(0x10, L);                                    break;  // SET 4, L
            case 0xE6: _mmu.WriteByte(HL, SET(0x10, _mmu.ReadByte(HL)));    break;  // SET 4, [HL]
            case 0xE7: A = SET(0x10, A);                                    break;  // SET 4, A
            case 0xE8: B = SET(0x20, B);                                    break;  // SET 5, B
            case 0xE9: C = SET(0x20, C);                                    break;  // SET 5, C
            case 0xEA: D = SET(0x20, D);                                    break;  // SET 5, D
            case 0xEB: E = SET(0x20, E);                                    break;  // SET 5, E
            case 0xEC: H = SET(0x20, H);                                    break;  // SET 5, H
            case 0xED: L = SET(0x20, L);                                    break;  // SET 5, L
            case 0xEE: _mmu.WriteByte(HL, SET(0x20, _mmu.ReadByte(HL)));    break;  // SET 5, [HL]
            case 0xEF: A = SET(0x20, A);                                    break;  // SET 5, A
            case 0xF0: B = SET(0x40, B);                                    break;  // SET 6, B
            case 0xF1: C = SET(0x40, C);                                    break;  // SET 6, C
            case 0xF2: D = SET(0x40, D);                                    break;  // SET 6, D
            case 0xF3: E = SET(0x40, E);                                    break;  // SET 6, E
            case 0xF4: H = SET(0x40, H);                                    break;  // SET 6, H
            case 0xF5: L = SET(0x40, L);                                    break;  // SET 6, L
            case 0xF6: _mmu.WriteByte(HL, SET(0x40, _mmu.ReadByte(HL)));    break;  // SET 6, [HL]
            case 0xF7: A = SET(0x40, A);                                    break;  // SET 6, A
            case 0xF8: B = SET(0x80, B);                                    break;  // SET 7, B
            case 0xF9: C = SET(0x80, C);                                    break;  // SET 7, C
            case 0xFA: D = SET(0x80, D);                                    break;  // SET 7, D
            case 0xFB: E = SET(0x80, E);                                    break;  // SET 7, E
            case 0xFC: H = SET(0x80, H);                                    break;  // SET 7, H
            case 0xFD: L = SET(0x80, L);                                    break;  // SET 7, L
            case 0xFE: _mmu.WriteByte(HL, SET(0x80, _mmu.ReadByte(HL)));    break;  // SET 7, [HL]
            case 0xFF: A = SET(0x80, A);                                    break;  // SET 7, A
            default:
                throw new ArgumentException("The instruction given is either invalid or not implemented");
        }

        return CPUCycles.PrefixCycles[instruction];
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
        SetHalfCarryFlagSub(num, 1);
        SubtractionFlag = true;
        return (byte)result;
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

    private void BIT(byte pos, byte num)
    {
        ZeroFlag = (num & pos) == 0;
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

    private ushort ADDr16e8(ushort num)
    {
        byte e8 = _mmu.ReadByte(_pc++);
        ZeroFlag = false;
        SubtractionFlag = false;
        SetHalfCarryFlag((byte)num, e8);
        SetCarryFlag((byte)num + e8);
        return (ushort)(num + (sbyte)e8);
    }

    private int JP(bool flag, ushort value)
    {
        if (flag)
        {
            _pc = value;
            return CPUCycles.JPT;
        }
        _pc += 2;
        return CPUCycles.JPF;
    }

    private int JR(bool flag, sbyte value)
    {
        if (flag)
        {
            _pc += (ushort)value;
            return CPUCycles.JRT;
        }
        return CPUCycles.JRF;
    }

    private int CALL(bool flag, ushort value)
    {
        if (flag)
        {
            _sp -= 2;
            _mmu.WriteWord(_sp, (ushort)(_pc + 2));

            _pc = value;
            return CPUCycles.CALLT;
        }
        return CPUCycles.CALLF;
    }


    private void RST(ushort value)
    {
        _sp -= 2;
        _mmu.WriteWord(_sp, _pc);

        _pc = value;
    }

    private int RET(bool flag)
    {
        if (flag)
        {
            _pc = _mmu.ReadWord(_sp);
            _sp += 2;
            return CPUCycles.RETT;
        }
        return CPUCycles.RETF;
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

    // DEBUG: Debug only
    public override string ToString()
    {
        return $"""
            A = {A}
            B = {B}
            C = {C}
            D = {D}
            E = {E}
            H = {H}
            L = {L}

            Carry = {CarryFlag}
            HalfC = {HalfCarryFlag}
            Zero  = {ZeroFlag}
            Subtr = {SubtractionFlag}

            PC = {_pc:X2}
            SP = {_sp:X2}

            LastInstruction = {_lastInstruction:X2}

            InBIOS = {_mmu._inBios}
            """;
    }
}
