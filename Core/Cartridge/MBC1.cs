using System.IO;

namespace GameBoyEmulator.Core.Cartridge;

internal class MBC1 : ICartridge
{
    private const int SRamOffset = 0x2000;
    private const int RomOffset = 0x4000;

    private readonly byte[] _rom;
    private readonly byte[]? _sram;

    private readonly bool _battery;
    private bool _ramEnabled;
    private bool _advancedBanking;

    private readonly string _romName;

    private int _romBank;
    private int _ramBank;

    public MBC1(byte[] rom, string romName)
    {
        _rom = rom;

        _battery = _rom[0x0147] == 0x03;
        if (_rom[0x0147] == 0x02 || _rom[0x0147] == 0x03)
        {
            if (_rom[0x0149] == 0x02)
            {
                _sram = new byte[0x2000];
            }
            else if (_rom[0x0149] == 0x03)
            {
                _sram = new byte[0x8000];
            }
        }

        _romName = romName;

        _romBank = 1;

        if (_battery && File.Exists($"./saves/{_romName}.save"))
        {
            _sram = File.ReadAllBytes($"./saves/{_romName}.save");
        }
    }

    public byte ReadRam(ushort address)
    {
        if (_ramEnabled)
        {
            int bank = _advancedBanking ? _ramBank : 0;
            return _sram?[(bank * SRamOffset + (address & 0x1FFF)) & (_sram.Length - 1)] ?? 0xFF;
        }
        return 0xFF;
    }

    public byte ReadRom(ushort address)
    {
        if (address <= 0x3FFF)
        {
            if (_advancedBanking)
            {
                int bank = _ramBank << 5;
                return _rom[(bank * RomOffset + address) & (_rom.Length - 1)];
            }
            return _rom[address];
        }
        else
        {
            int bank = (_ramBank << 5) + _romBank;
            return _rom[(bank * RomOffset + (address & 0x3FFF)) & (_rom.Length - 1)];
        }
    }

    public void SaveRam()
    {
        if (_battery && _sram != null)
        {
            File.WriteAllBytes($"./saves/{_romName}.save", _sram);
        }
    }

    public void WriteRam(ushort address, byte value)
    {
        if (_ramEnabled && _sram != null)
        {
            int bank = _advancedBanking ? _ramBank : 0;
            _sram[(bank * SRamOffset + (address & 0x1FFF)) & (_sram.Length - 1)] = value;
        }
    }

    public void WriteRegister(ushort address, byte value)
    {
        if (address <= 0x1FFF)
        {
            _ramEnabled = (value & 0xF) == 0xA;
            if (!_ramEnabled)
            {
                SaveRam();
            }
        }
        else if (address <= 0x3FFF)
        {
            _romBank = value & 0x1F;
            if (_romBank == 0)
            {
                _romBank = 1;
            }
        }
        else if (address <= 0x5FFF)
        {
            _ramBank = value & 0x03;
        }
        else if (address <= 0x7FFF)
        {
            _advancedBanking = (value & 0x1) == 1;
        }
    }
}
