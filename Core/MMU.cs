namespace GameBoyEmulator.Core;
public class MMU
{
    private bool _inBios;

    private byte[] _bios;
    private byte[] _rom;    // NOTE: Move to Cartridge?
    private byte[] _wram;
    private byte[] _vram;   // NOTE: Move to GPU?
    private byte[] _hram;
    private byte[] _eram;   // NOTE: Move to Cartridge?
    private byte[] _oam;    // NOTE: Move to GPU?
    private byte[] _io;

    public byte IE { get; set; } // TODO: Implement IE
    public byte IF { get; set; } // TODO: Implement IF

    public MMU()
    {
        _inBios = true;
        _bios = new byte[0x100];
        _rom = new byte[0x8000];
        _vram = new byte[0x2000];
        _eram = new byte[0x2000];
        _wram = new byte[0x2000];
        _oam = new byte[0xA0];
        _io = new byte[0x80];
        _hram = new byte[0x80];
    }

    public byte ReadByte(ushort address)
    {
        switch(address)
        {
            case ushort _ when address <= 0x00FF:
                if (_inBios)
                {
                    return _bios[address];
                }
                return _rom[address];
            case ushort _ when address <= 0x7FFF:
                _inBios = false;
                return _rom[address];
            case ushort _ when address <= 0x9FFF:
                return _vram[address & 0x1FFF];
            case ushort _ when address <= 0xDFFF:
                return _wram[address & 0x1FFF];
            case ushort _ when address <= 0xFDFF:
                return _wram[address & 0x1FFF];
            case ushort _ when address <= 0xFE9F:
                return _oam[address & 0x9F];
            case ushort _ when address <= 0xFEFF:
                return 0;
            case ushort _ when address <= 0xFF7F:
                return _io[address & 0x7F];
            case ushort _ when address <= 0xFFFF:
                return _hram[address & 0x7F];
            default:
                return 0;
        }
    }

    public ushort ReadWord(ushort address)
    {
        throw new NotImplementedException("MMU is not implemented yet");
    }

    public void WriteByte(ushort address, byte value)
    {
        throw new NotImplementedException("MMU is not implemented yet");
    }

    public void WriteWord(ushort address, ushort value)
    {
        throw new NotImplementedException("MMU is not implemented yet");
    }
}
