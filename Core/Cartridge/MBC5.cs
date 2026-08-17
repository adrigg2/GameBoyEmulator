using GameBoyEmulator.SaveState.Components;
using System.IO;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC5 : ICartridge
{
    private const int SRamOffset = 0x2000;
    private const int RomOffset = 0x4000;

    private readonly byte[] _rom;
    private byte[]? _sram;

    private readonly bool _battery;
    private bool _ramEnabled;

    private readonly string _romName;

    private int _romBank;
    private int _sramBank;

    public byte[] HeaderCheck => [.. _rom[0x0134..0x0144], .. _rom[0x014D..0x0150]];

    public MBC5(byte[] rom, string cartridgeName)
    {
        _rom = rom;
        _romName = cartridgeName;

        byte type = rom[0x0147];
        _battery = type == 0x1B || type == 0x1E;

        if (type == 0x1A || type == 0x1B || type == 0x1D || type == 0x1E)
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
            else if (ramSize == 0x04)
            {
                _sram = new byte[0x20000];
            }
        }

        _romBank = 1;

        if (_battery && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/saves/{_romName}.save"))
        {
            _sram = File.ReadAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/saves/{_romName}.save");
        }
    }

    public byte ReadRam(ushort address)
    {
        if (_ramEnabled)
        {
            return _sram?[(_sramBank * SRamOffset + (address & 0x1FFF)) % _sram.Length] ?? 0xFF;
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
            return _rom[(_romBank * RomOffset + (address & 0x3FFF)) % _rom.Length];
        }
    }

    public void SaveRam()
    {
        if (_battery && _sram != null)
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/saves/");
            File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/saves/{_romName}.save", _sram);
        }
    }

    public void WriteRam(ushort address, byte value)
    {
        if (_ramEnabled && _sram != null)
        {
            _sram[(_sramBank * SRamOffset + (address & 0x1FFF)) % _sram.Length] = value;
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
        else if (address <= 0x2FFF)
        {
            _romBank &= 0x100;
            _romBank |= value;
        }
        else if (address <= 0x3FFF)
        {
            _romBank &= 0x0FF;
            _romBank |= (value << 8) & 0x100;
        }
        else if (address <= 0x5FFF)
        {
            _sramBank = value & 0x0F;
        }
    }

    public MBCState SaveState()
    {
        return new MBCState(
            _romBank,
            _sramBank,
            _ramEnabled,
            HeaderCheck,
            null,
            _sram
            );
    }

    public void LoadState(MBCState state)
    {
        _romBank = state.ROMBank;
        _sramBank = state.SRAMBank;
        _ramEnabled = state.RAMEnabled;
        _sram = state.SRAM;
    }
}
