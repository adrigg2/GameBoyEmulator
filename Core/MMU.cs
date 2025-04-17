namespace GameBoyEmulator.Core;
public class MMU
{
    public bool _inBios; // DEBUG: Public

    private readonly byte[] _bootROM;
    private byte[] _rom;    // NOTE: Move to Cartridge?
    private byte[] _wram;
    private byte[] _vram;   // NOTE: Move to GPU?
    private byte[] _hram;
    private byte[] _eram;   // NOTE: Move to Cartridge?
    private byte[] _oam;    // NOTE: Move to GPU?
    private byte[] _io;
    private byte _ie;

    public byte IE { get => _ie; set => _ie = value; }
    public byte IF { get => ReadByte(0xFF0F); set => WriteByte(0xFF0F, value); }
    public byte LCDC { get => ReadByte(0xFF40); set => WriteByte(0xFF40, value); }
    public byte STAT { get => ReadByte(0xFF41); set => WriteByte(0xFF41, value); }
    public byte SCY { get => ReadByte(0xFF42); set => WriteByte(0xFF42, value); }
    public byte SCX { get => ReadByte(0xFF43); set => WriteByte(0xFF43, value); }
    public byte LY { get => ReadByte(0xFF44); set => WriteByte(0xFF44, value); }
    public byte LYC { get => ReadByte(0xFF45); set => WriteByte(0xFF45, value); }
    public byte BGP { get => ReadByte(0xFF47); set => WriteByte(0xFF47, value); }

    public MMU()
    {
        _inBios = true;
        _bootROM = new byte[0x100];
        _rom = new byte[0x8000];
        _vram = new byte[0x2000];
        _eram = new byte[0x2000];
        _wram = new byte[0x2000];
        _oam = new byte[0xA0];
        _io = new byte[0x80];
        _hram = new byte[0x80];
        _ie = 0;
    }

    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case ushort _ when address <= 0x00FF:
                if (_inBios)
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
            case ushort _ when address <= 0xFF7F:
                return _io[address & 0x7F];
            case ushort _ when address <= 0xFFFF:
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
        if (address == 0xFF50)
        {
            _inBios = false;
        }

        switch (address)
        {
            case ushort _ when address <= 0x7FFF:
                _rom[address] = value;
                break;
            case ushort _ when address <= 0x9FFF:
                _vram[address & 0x1FFF] = value;
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
            case ushort _ when address <= 0xFF7F:
                _io[address & 0x7F] = value;
                break;
            case ushort _ when address <= 0xFFFF:
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
    }

    public void LoadBootRom(byte[] rom)
    {
        Array.Copy(rom, _bootROM, Math.Min(_bootROM.Length, rom.Length));
    }
}
