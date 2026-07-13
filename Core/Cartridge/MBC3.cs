using System.IO;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC3 : ICartridge
{
    private const int SRamOffset = 0x2000;
    private const int RomOffset = 0x4000;

    private readonly byte[] _rom;
    private readonly byte[]? _sram;

    private readonly bool _battery;
    private bool _ramEnabled;

    private string _romName;

    private int _romBank;
    private int _sramBank;
    private int _rtcRegister;   // TODO: RTC

    public MBC3(byte[] rom, string cartridgeName)
    {
        _rom = rom;
        _romName = cartridgeName;

        byte type = rom[0x0147];
        _battery = type == 0x0F || type == 0x10 || type == 0x13;

        if (type == 0x10 || type == 0x12 || type == 0x13)
        {
            byte ramSize = rom[0x0149];
            if (ramSize == 0x02)
            {
                _sram = new byte[0x2000];
            }
            else if (ramSize == 0x03)
            {
                _sram = new byte[0x8000];
            }
        }

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
            return _sram?[(_sramBank * SRamOffset + (address & 0x1FFF)) & (_sram.Length - 1)] ?? 0xFF;
        }
        return 0xFF;
    }

    public byte ReadRom(ushort address)
    {
        if (address <= 0x3FFF)
        {
            return _rom[address];
        }
        else
        {
            return _rom[(_romBank * RomOffset + (address & 0x3FFF)) & (_rom.Length - 1)];
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
            _sram[(_sramBank * SRamOffset + (address & 0x1FFF)) & (_sram.Length - 1)] = value;
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
            _romBank = value & 0x7F;
            if (_romBank == 0)
            {
                _romBank = 1;
            }
        }
        else if (address <= 0x5FFF)
        {
            if (value < 0x07)
            {
                _sramBank = value & 0x07;
            }
            else
            {
                _rtcRegister = value & 0x0F;
            }
        }
    }
}
