using System.IO;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC3 : ICartridge
{
    private const int SRamOffset = 0x2000;
    private const int RomOffset = 0x4000;

    private readonly byte[] _rom;
    private readonly byte[]? _sram;

    private readonly bool _battery;
    private readonly bool _hasRTC;
    private bool _ramEnabled;
    private bool _latchReady;

    private string _romName;

    private byte _rtcS;
    private byte _rtcM;
    private byte _rtcH;
    private byte _rtcDL;
    private byte _rtcDH;

    private int _romBank;
    private int _sramBank;
    private double _rtcTime;


    private DateTime _lastDateTime;

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

        if (_battery && File.Exists($"./saves/{_romName}.save"))
        {
            using BinaryReader reader = new(File.OpenRead($"./saves/{_romName}.save"));
            if (_sram != null)
            {
                int sramLength = reader.ReadInt32();
                _sram = reader.ReadBytes(sramLength);
            }
            _rtcTime = reader.ReadDouble();
            long serializedDate = reader.ReadInt64();
            _lastDateTime = DateTime.FromBinary(serializedDate);

            var now = DateTime.UtcNow;
            _rtcTime += (now - _lastDateTime).TotalSeconds;
            _lastDateTime = now;
        }
    }

    public byte ReadRam(ushort address)
    {
        if (_ramEnabled && _sramBank <= 0x07)
        {
            return _sram?[(_sramBank * SRamOffset + (address & 0x1FFF)) & (_sram.Length - 1)] ?? 0xFF;
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
            return _rom[(_romBank * RomOffset + (address & 0x3FFF)) & (_rom.Length - 1)];
        }
    }

    public void SaveRam()
    {
        if (_battery)
        {
            using BinaryWriter writer = new(File.OpenWrite($"./saves/{_romName}.save"));
            if (_sram != null)
            {
                writer.Write(_sram.Length);
                writer.Write(_sram);
            }
            writer.Write(_rtcTime);
            writer.Write(_lastDateTime.ToBinary());
        }
    }

    public void WriteRam(ushort address, byte value)
    {
        if (_ramEnabled && _sram != null && _sramBank <= 0x07)
        {
            _sram[(_sramBank * SRamOffset + (address & 0x1FFF)) & (_sram.Length - 1)] = value;
        }
        else if (_ramEnabled && _sramBank > 0x07 && _hasRTC)
        {
            switch (_sramBank)
            {
                case 0x08:
                    _rtcS = value;
                    break;
                case 0x09:
                    _rtcM = value;
                    break;
                case 0x0A:
                    _rtcH = value;
                    break;
                case 0x0B:
                    _rtcDL = value;
                    break;
                case 0x0C:
                    _rtcDH = value;
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
                var now = DateTime.UtcNow;
                _rtcTime += (now - _lastDateTime).TotalSeconds;
                _lastDateTime = now;

                _rtcS = (byte)(_rtcTime % 60);
                _rtcM = (byte)(_rtcTime / 60 % 60);
                _rtcH = (byte)(_rtcTime / 3600 % 24);

                int day = (int)_rtcTime / 3600 / 24;
                bool overflow = day > 0x1FF;
                bool previousOverflow = (_rtcDH & 0x80) != 0;

                _rtcDL = (byte)(day & 0xFF);
                _rtcDH = (byte)((day >> 8) & 0x01);
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
}
