using GameBoyEmulator.Core.Audio;
using GameBoyEmulator.Core.Cartridge;
using GameBoyEmulator.SaveState.Components;

namespace GameBoyEmulator.Core;
public class MMU(DMA dma, JOYPAD joypad, PPU ppu, TIMER timer, APU apu)
{
    private DMA _dma = dma;
    private JOYPAD _joypad = joypad;
    private PPU _ppu = ppu;
    private TIMER _timer = timer;
    private APU _apu = apu;

    public bool _bootRomMapped = true; // DEBUG: Public

    private readonly byte[] _bootROM = new byte[0x100];
    private byte[] _wram = new byte[0x2000];
    private byte[] _vram = new byte[0x2000];   // NOTE: Move to PPU?
    private byte[] _hram = new byte[0x7F];
    private byte[] _oam = new byte[0xA0];    // NOTE: Move to PPU?
    private byte _ie = 0;
    private byte _if;
    private ICartridge _cartridge = new NoCartridge();

    public byte IE { get => _ie; set => _ie = value; }
    public byte IF { get => _if; set => _if = value; }

    public ICartridge Cartridge { get => _cartridge; }
    public byte[] VRAM { get => _vram; }

    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case ushort _ when address <= 0x00FF:
                if (_bootRomMapped)
                {
                    return _bootROM[address];
                }
                return _cartridge.ReadRom(address);
            case ushort _ when address <= 0x7FFF:
                return _cartridge.ReadRom(address);
            case ushort _ when address <= 0x9FFF:
                return _vram[address & 0x1FFF];
            case ushort _ when address <= 0xBFFF:
                return _cartridge.ReadRam((ushort)(address & 0x1FFF));
            case ushort _ when address <= 0xDFFF:
                return _wram[address & 0x1FFF];
            case ushort _ when address <= 0xFDFF:   // Echo RAM
                return _wram[address & 0x1FFF];
            case ushort _ when address <= 0xFE9F:
                return _oam[address - 0xFE00];
            case ushort _ when address <= 0xFEFF:
                return 0;
            case 0xFF00:
                return _joypad.JOYP;
            case 0xFF04:
                return _timer.DIV;
            case 0xFF05:
                return _timer.TIMA;
            case 0xFF06:
                return _timer.TMA;
            case 0xFF07:
                return _timer.TAC;
            case 0xFF0F:
                return IF;
            case 0xFF10:
                return _apu.Channel1.NR10;
            case 0xFF11:
                return _apu.Channel1.NR11;
            case 0xFF12:
                return _apu.Channel1.NR12;
            case 0xFF14:
                return _apu.Channel1.NR14;
            case 0xFF16:
                return _apu.Channel2.NR21;
            case 0xFF17:
                return _apu.Channel2.NR22;
            case 0xFF19:
                return _apu.Channel2.NR24;
            case 0xFF1A:
                return _apu.Channel3.NR30;
            case 0xFF1C:
                return _apu.Channel3.NR32;
            case 0xFF1E:
                return _apu.Channel3.NR34;
            case 0xFF21:
                return _apu.Channel4.NR42;
            case 0xFF22:
                return _apu.Channel4.NR43;
            case 0xFF23:
                return _apu.Channel4.NR44;
            case 0xFF24:
                return _apu.NR50;
            case 0xFF25:
                return _apu.NR51;
            case 0xFF26:
                return _apu.NR52;
            case ushort _ when address >= 0xFF30 && address <= 0xFF3F:
                return _apu.Channel3.ReadWaveRam(address);
            case 0xFF40:
                return _ppu.LCDC;
            case 0xFF41:
                return _ppu.STAT;
            case 0xFF42:
                return _ppu.SCY;
            case 0xFF43:
                return _ppu.SCX;
            case 0xFF44:
                return _ppu.LY;
            case 0xFF45:
                return _ppu.LYC;
            case 0xFF46:
                return _dma.Address;
            case 0xFF47:
                return _ppu.BGP;
            case 0xFF48:
                return _ppu.OBP0;
            case 0xFF49:
                return _ppu.OBP1;
            case 0xFF4A:
                return _ppu.WY;
            case 0xFF4B:
                return _ppu.WX;
            case ushort _ when address <= 0xFF7F:
                return 0;
            case ushort _ when address < 0xFFFF:
                return _hram[address & 0x7F];
            case 0xFFFF:
                return _ie;
            default:
                return 0;
        }
    }

    public ushort ReadWord(ushort address)
    {
        return (ushort)(ReadByte(address) + (ReadByte((ushort)(address + 1)) << 8));
    }

    public void WriteByte(ushort address, byte value)
    {
        switch (address)
        {
            case ushort _ when address <= 0x7FFF:
                _cartridge.WriteRegister(address, value);
                break;
            case ushort _ when address <= 0x9FFF:
                _vram[address & 0x1FFF] = value;
                break;
            case ushort _ when address <= 0xBFFF:
                _cartridge.WriteRam((ushort)(address & 0x1FFF), value);
                break;
            case ushort _ when address <= 0xDFFF:
                _wram[address & 0x1FFF] = value;
                break;
            case ushort _ when address <= 0xFDFF:   // Echo RAM
                _wram[address & 0x1FFF] = value;
                break;
            case ushort _ when address <= 0xFE9F:
                _oam[address - 0xFE00] = value;
                break;
            case ushort _ when address <= 0xFEFF:
                break;
            case 0xFF00:
                _joypad.JOYP = value;
                break;
            case 0xFF04:
                _timer.DIV = value;
                break;
            case 0xFF05:
                _timer.TIMA = value;
                break;
            case 0xFF06:
                _timer.TMA = value;
                break;
            case 0xFF07:
                _timer.TAC = value;
                break;
            case 0xFF0F:
                IF = value;
                break;
            case 0xFF10:
                _apu.Channel1.NR10 = value;
                break;
            case 0xFF11:
                _apu.Channel1.NR11 = value;
                break;
            case 0xFF12:
                _apu.Channel1.NR12 = value;
                break;
            case 0xFF13:
                _apu.Channel1.NR13 = value;
                break;
            case 0xFF14:
                _apu.Channel1.NR14 = value;
                break;
            case 0xFF16:
                _apu.Channel2.NR21 = value;
                break;
            case 0xFF17:
                _apu.Channel2.NR22 = value;
                break;
            case 0xFF18:
                _apu.Channel2.NR23 = value;
                break;
            case 0xFF19:
                _apu.Channel2.NR24 = value;
                break;
            case 0xFF1A:
                _apu.Channel3.NR30 = value;
                break;
            case 0xFF1B:
                _apu.Channel3.NR31 = value;
                break;
            case 0xFF1C:
                _apu.Channel3.NR32 = value;
                break;
            case 0xFF1D:
                _apu.Channel3.NR33 = value;
                break;
            case 0xFF1E:
                _apu.Channel3.NR34 = value;
                break;
            case 0xFF20:
                _apu.Channel4.NR41 = value;
                break;
            case 0xFF21:
                _apu.Channel4.NR42 = value;
                break;
            case 0xFF22:
                _apu.Channel4.NR43 = value;
                break;
            case 0xFF23:
                _apu.Channel4.NR44 = value;
                break;
            case 0xFF24:
                _apu.NR50 = value;
                break;
            case 0xFF25:
                _apu.NR51 = value;
                break;
            case 0xFF26:
                _apu.NR52 = value;
                break;
            case ushort _ when address >= 0xFF30 && address <= 0xFF3F:
                _apu.Channel3.WriteWaveRam(address, value);
                break;
            case 0xFF40:
                _ppu.LCDC = value;
                break;
            case 0xFF41:
                _ppu.STAT = value;
                break;
            case 0xFF42:
                _ppu.SCY = value;
                break;
            case 0xFF43:
                _ppu.SCX = value;
                break;
            case 0xFF45:
                _ppu.LYC = value;
                break;
            case 0xFF46:
                _dma.Address = value;
                break;
            case 0xFF47:
                _ppu.BGP = value;
                break;
            case 0xFF48:
                _ppu.OBP0 = value;
                break;
            case 0xFF49:
                _ppu.OBP1 = value;
                break;
            case 0xFF4A:
                _ppu.WY = value;
                break;
            case 0xFF4B:
                _ppu.WX = value;
                break;
            case 0xFF50:
                _bootRomMapped = false;
                break;
            case ushort _ when address <= 0xFF7F:
                break;
            case ushort _ when address < 0xFFFF:
                _hram[address & 0x7F] = value;
                break;
            case 0xFFFF:
                _ie = value;
                break;
            default:
                break;
        }
    }

    public void WriteWord(ushort address, ushort value)
    {
        WriteByte(address, (byte)value);
        WriteByte((ushort)(address + 1), (byte)(value >> 8));
    }

    public void LoadGame(byte[] rom, string romName)
    {
        Console.WriteLine($"{rom[0x0147]:x2}");
        Console.WriteLine($"{rom[0x0148]:x2}");
        Console.WriteLine($"{rom[0x0149]:x2}");
        Console.WriteLine($"{rom[0x0038]:x2}");

        string s = "";
        for (int i = 0x0134; i <= 0x0143; i++)
        {
            s += (char)rom[i];
        }
        Console.WriteLine(s);

        switch(rom[0x147])
        {
            case 0x00:
                _cartridge = new NoMBC(rom);
                break;
            case 0x01:
            case 0x02:
            case 0x03:
                _cartridge = new MBC1(rom, romName);
                break;
            case 0x05:
            case 0x06:
                _cartridge = new MBC2(rom, romName);
                break;
            case 0x0F:
            case 0x10:
            case 0x11:
            case 0x12:
            case 0x13:
                _cartridge = new MBC3(rom, romName);
                break;
            case 0x19:
            case 0x1A:
            case 0x1B:
            case 0x1C:
            case 0x1D:
            case 0x1E:
                _cartridge = new MBC5(rom, romName);
                break;
            default:
                Console.Beep();
                Console.WriteLine($"Unsuported MBC: {rom[0x147]:X2}");
                break;
        }
    }

    public void LoadBootRom(byte[] rom)
    {
        Array.Copy(rom, _bootROM, Math.Min(_bootROM.Length, rom.Length));
    }

    public MMUState SaveState()
    {
        return new MMUState(
            _bootRomMapped,
            _ie,
            _if,
            (byte[])_wram.Clone(),
            (byte[])_vram.Clone(),
            (byte[])_hram.Clone(),
            (byte[])_oam.Clone(),
            _cartridge.SaveState()
            );
    }

    public void LoadState(MMUState state)
    {
        _bootRomMapped = state.BootRomMapped;
        _ie = state.IE;
        _if = state.IF;
        _wram = (byte[])state.WRAM.Clone();
        _vram = (byte[])state.VRAM.Clone();
        _hram = (byte[])state.HRAM.Clone();
        _oam = (byte[])state.OAM.Clone();
        _cartridge.LoadState(state.Cartridge);
    }
}
