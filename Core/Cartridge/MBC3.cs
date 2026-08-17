using GameBoyEmulator.SaveState.Components;
using System.IO;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC3 : ICartridge
{
    private const int SRamOffset = 0x2000;
    private const int RomOffset = 0x4000;

    private readonly byte[] _rom;
    private byte[]? _sram;

    private readonly bool _battery;
    private readonly bool _hasRTC;
    private bool _ramEnabled;
    private bool _latchReady;
    private bool _rtcHalted;

    private readonly string _romName;

    private byte _rtcS;
    private byte _rtcM;
    private byte _rtcH;
    private byte _rtcDL;
    private byte _rtcDH;

    private int _romBank;
    private int _sramBank;
    private double _rtcTime;


    private DateTime _lastDateTime;

    public byte[] HeaderCheck => [.. _rom[0x0134..0x0144], .. _rom[0x014D..0x0150]];

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

        if (type == 0x0F || type == 0x10)
        {
            _hasRTC = true;
            _lastDateTime = DateTime.UtcNow;
        }

        _romBank = 1;

        if (_battery && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/GEGB/saves/{_romName}.save"))
        {
            using FileStream stream = File.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/GEGB/saves/{_romName}.save");
            using BinaryReader reader = new(stream);
            if (_sram != null)
            {
                int sramLength = reader.ReadInt32();
                _sram = reader.ReadBytes(sramLength);
            }

            if (_hasRTC)
            {
                _rtcTime = reader.ReadDouble();
                long serializedDate = reader.ReadInt64();
                _lastDateTime = DateTime.FromBinary(serializedDate);

                var now = DateTime.UtcNow;
                _rtcTime += (now - _lastDateTime).TotalSeconds;
                _lastDateTime = now;

                _rtcDH = reader.ReadByte();
                _rtcHalted = (_rtcDH & 0x40) != 0;
            }
        }
    }

    public byte ReadRam(ushort address)
    {
        if (_ramEnabled && _sramBank <= 0x07)
        {
            return _sram?[(_sramBank * SRamOffset + (address & 0x1FFF)) % _sram.Length] ?? 0xFF;
        }
        else if (_ramEnabled && _sramBank > 0x07 && _hasRTC)
        {
            return _sramBank switch
            {
                0x08 => _rtcS,
                0x09 => _rtcM,
                0x0A => _rtcH,
                0x0B => _rtcDL,
                0x0C => _rtcDH,
                _ => 0xFF,
            };
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
        if (_battery)
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GEGB/saves/");
            using FileStream stream = File.OpenWrite(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/GEGB/saves/{_romName}.save");
            using BinaryWriter writer = new(stream);
            if (_sram != null)
            {
                writer.Write(_sram.Length);
                writer.Write(_sram);
            }

            if (_hasRTC)
            {
                writer.Write(_rtcTime);
                writer.Write(_lastDateTime.ToBinary());
                writer.Write(_rtcDH);
            }
        }
    }

    public void WriteRam(ushort address, byte value)
    {
        if (_ramEnabled && _sram != null && _sramBank <= 0x07)
        {
            _sram[(_sramBank * SRamOffset + (address & 0x1FFF)) % _sram.Length] = value;
        }
        else if (_ramEnabled && _sramBank > 0x07 && _hasRTC)
        {
            switch (_sramBank)
            {
                case 0x08:
                    byte oldRtcS = _rtcS;
                    _rtcS = (byte)(value % 60);
                    _rtcTime += _rtcS - oldRtcS;
                    break;
                case 0x09:
                    byte oldRtcM = _rtcM;
                    _rtcM = (byte)(value % 60);
                    _rtcTime += _rtcM * 60 - oldRtcM * 60;
                    break;
                case 0x0A:
                    byte oldRtcH = _rtcH;
                    _rtcH = (byte)(value % 24);
                    _rtcTime += _rtcH * 3600 - oldRtcH * 3600;
                    break;
                case 0x0B:
                    byte oldRtcDl = _rtcDL;
                    _rtcDL = value;
                    _rtcTime += _rtcDL * 24 * 3600 - oldRtcDl * 24 * 3600;
                    break;
                case 0x0C:
                    int oldRtcDHday = (_rtcDH & 0x01) << 8;
                    _rtcDH = value;
                    _rtcHalted = (_rtcDH & 0x40) != 0;

                    int newRtcDHday = (_rtcDH & 0x01) << 8;
                    _rtcTime += newRtcDHday * 24 * 3600 - oldRtcDHday * 24 * 3600;
                    break;
            };
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
            _sramBank = value & 0x0F;
        }
        else if (address <= 0x7FFF)
        {
            if (!_hasRTC)
            {
                return;
            }

            if (value == 0)
            {
                _latchReady = true;
            }
            else if (value == 1 && _latchReady)
            {
                if (!_rtcHalted)
                {
                    var now = DateTime.UtcNow;
                    _rtcTime += (now - _lastDateTime).TotalSeconds;
                    _lastDateTime = now;
                }

                _rtcS = (byte)(_rtcTime % 60);
                _rtcM = (byte)(_rtcTime / 60 % 60);
                _rtcH = (byte)(_rtcTime / 3600 % 24);

                int day = (int)_rtcTime / 3600 / 24;
                bool overflow = day > 0x1FF;
                bool previousOverflow = (_rtcDH & 0x80) != 0;

                _rtcDL = (byte)(day & 0xFF);

                _rtcDH &= 0xFE;
                _rtcDH |= (byte)((day >> 8) & 0x01);
                _rtcDH |= (byte)(overflow || previousOverflow ? 0x80 : 0);

                if (overflow)
                {
                    _rtcTime -= 0x1FF * 3600 * 24 + 23 * 3600 + 59 * 60 + 59;
                }

                _latchReady = false;
            }
        }
        else
        {
            _latchReady = false;
        }
    }

    public MBCState SaveState()
    {
        byte[] additionalRegisters = [
            (byte)(_latchReady ? 1 : 0),
            (byte)(_rtcHalted ? 1 : 0),
            _rtcS,
            _rtcM,
            _rtcH,
            _rtcDL,
            _rtcDH,
            .. BitConverter.GetBytes(_lastDateTime.ToBinary()),
            .. BitConverter.GetBytes(_rtcTime),
            ];
        return new MBCState(
            _romBank,
            _sramBank,
            _ramEnabled,
            HeaderCheck,
            additionalRegisters,
            _sram
            );
    }

    public void LoadState(MBCState state)
    {
        _romBank = state.ROMBank;
        _sramBank = state.SRAMBank;
        _ramEnabled = state.RAMEnabled;
        _sram = state.SRAM;
        _latchReady = state.AdditionalRegisters?[0] == 1;
        _rtcHalted = state.AdditionalRegisters?[1] == 1;
        _rtcS = state.AdditionalRegisters?[2] ?? 0;
        _rtcM = state.AdditionalRegisters?[3] ?? 0;
        _rtcH = state.AdditionalRegisters?[4] ?? 0;
        _rtcDL = state.AdditionalRegisters?[5] ?? 0;
        _rtcDH = state.AdditionalRegisters?[6] ?? 0;
        _lastDateTime = DateTime.FromBinary(BitConverter.ToInt64(state.AdditionalRegisters ?? new byte[8], 7));
        _rtcTime = BitConverter.ToDouble(state.AdditionalRegisters ?? new byte[8], 15);
    }
}
