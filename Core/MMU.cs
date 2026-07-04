namespace GameBoyEmulator.Core;
public class MMU
{
    private DMA _dma;
    private JOYPAD _joypad;
    private PPU _ppu;
    private TIMER _timer;

    public bool _bootRomMapped; // DEBUG: Public

    private readonly byte[] _bootROM;
    private byte[] _rom;    // NOTE: Move to Cartridge?
    private byte[] _wram;
    private byte[] _vram;   // NOTE: Move to GPU?
    private object[] _occupiedVram; // DEBUG: debug info
    private byte[] _hram;
    private byte[] _eram;   // NOTE: Move to Cartridge?
    private byte[] _oam;    // NOTE: Move to PPU?
    private byte _ie;
    private byte _if;

    public byte IE { get => _ie; set => _ie = value; }
    public byte IF { get => _if; set => _if = value; }

    public MMU(DMA dma, JOYPAD joypad, PPU ppu, TIMER timer)
    {
        _bootRomMapped = true;
        _bootROM = new byte[0x100];
        _rom = new byte[0x8000];
        _vram = new byte[0x2000];
        _eram = new byte[0x2000];
        _wram = new byte[0x2000];
        _oam = new byte[0xA0];
        _hram = new byte[0x7F];
        _occupiedVram = new object[0x2000]; // DEBUG: debug info
        _ie = 0;
        _dma = dma;
        _joypad = joypad;
        _ppu = ppu;
        _timer = timer;
    }

    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case ushort _ when address <= 0x00FF:
                if (_bootRomMapped)
                {
                    return _bootROM[address];
                }
                return _rom[address];
            case ushort _ when address <= 0x7FFF:
                return _rom[address];
            case ushort _ when address <= 0x9FFF:
                return _vram[address & 0x1FFF];
            case ushort _ when address <= 0xBFFF:   // NOTE: Move to cartridge?
                return _eram[address & 0x1FFF];
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
                //_rom[address] = value;
                break;
            case ushort _ when address <= 0x9FFF:
                _vram[address & 0x1FFF] = value;
                /*_occupiedVram = _vram.Select((val, index) => new { val, index })
                    .Where(x => x.val != 0)
                    .ToArray(); // DEBUG: debug info*/
                break;
            case ushort _ when address <= 0xBFFF:   // NOTE: Move to cartridge?
                _eram[address & 0x1FFF] = value;
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

    public void LoadGame(byte[] rom)
    {
        Array.Copy(rom, _rom, Math.Min(_rom.Length, rom.Length));
        Console.WriteLine($"{rom[0x0147]:x2}");
        Console.WriteLine($"{rom[0x0148]:x2}");
        Console.WriteLine($"{rom[0x0149]:x2}");
        Console.WriteLine($"{rom[0x0038]:x2}");
    }

    public void LoadBootRom(byte[] rom)
    {
        Array.Copy(rom, _bootROM, Math.Min(_bootROM.Length, rom.Length));
    }
}
