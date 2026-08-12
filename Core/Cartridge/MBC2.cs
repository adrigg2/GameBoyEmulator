using GameBoyEmulator.SaveState.Components;
using System.IO;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC2 : ICartridge
{
    private const int RomOffset = 0x4000;

    private readonly byte[] _rom;
    private byte[] _sram;

    private readonly bool _battery;
    private bool _ramEnabled;

    private int _romBank;

    private readonly string _romName;

    public MBC2(byte[] rom, string romName)
    {
        _rom = rom;
        _sram = new byte[512];
        _romName = romName;
        _romBank = 1;

        if (rom[0x0147] == 0x06)
        {
            _battery = true;
            if (File.Exists($"./saves/{_romName}.save"))
            {
                _sram = File.ReadAllBytes($"./saves/{_romName}.save");
            }
        }
    }

    public byte ReadRam(ushort address)
    {
        if (_ramEnabled)
        {
            return _sram[address & 0x1FF];
        }
        return 0xFF;
    }

    public byte ReadRom(ushort address)
    {
        if (address <= 0x3FFF)
        {
            return _rom[address];
        }
        return _rom[(_romBank * RomOffset + (address & 0x3FFF)) & (_rom.Length - 1)];
    }

    public void SaveRam()
    {
        if (_battery)
        {
            File.WriteAllBytes($"./saves/{_romName}.save", _sram);
        }
    }

    public void WriteRam(ushort address, byte value)
    {
        if (_ramEnabled)
        {
            _sram[address & 0x1FF] = (byte)(value | 0xF0);
        }
    }

    public void WriteRegister(ushort address, byte value)
    {
        if (address > 0x3FFF)
        {
            return;
        }

        if ((address & 0x100) == 0)
        {
            _ramEnabled = (value & 0xF) == 0xA;
            if (!_ramEnabled)
            {
                SaveRam();
            }
        }
        else
        {
            _romBank = value & 0xF;
            if (_romBank == 0)
            {
                _romBank = 1;
            }
        }
    }

    public MBCState SaveState()
    {
        byte[] headerCheck = [.. _rom[0x0134..0x0144], .. _rom[0x014D..0x0150]];
        return new MBCState(
            _romBank,
            0,
            _ramEnabled,
            headerCheck,
            null,
            _sram
            );
    }

    public void LoadState(MBCState state)
    {
        _romBank = state.ROMBank;
        _ramEnabled = state.RAMEnabled;
        _sram = state.SRAM ?? new byte[512];
    }
}
