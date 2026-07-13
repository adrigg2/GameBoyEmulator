using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Core;

public class APU
{
    private byte _nr10;
    private byte _nr11;
    private byte _nr12;
    private byte _nr13;
    private byte _nr14;
    private byte _nr21;
    private byte _nr22;
    private byte _nr23;
    private byte _nr24;
    private byte _nr30;
    private byte _nr31;
    private byte _nr32;
    private byte _nr33;
    private byte _nr34;
    private byte _nr41;
    private byte _nr42;
    private byte _nr43;
    private byte _nr44;
    private byte _nr50;
    private byte _nr51;
    private byte _nr52;

    private byte[] _waveRam;

    public byte NR10 { get => _nr10; set => _nr10 = value; }
    public byte NR11 { get => (byte)(_nr11 & 0xC0); set => _nr11 = value; }
    public byte NR12 { get => _nr12; set => _nr12 = value; }
    public byte NR13 { set => _nr13 = value; }
    public byte NR14 { get => (byte)(_nr14 & 0x40); set => _nr14 = value; }
    public byte NR21 { get => (byte)(_nr21 & 0xC0); set => _nr21 = value; }
    public byte NR22 { get => _nr22; set => _nr22 = value; }
    public byte NR23 { set => _nr23 = value; }
    public byte NR24 { get => (byte)(_nr24 & 0x40); set => _nr24 = value; }
    public byte NR30 { get => _nr30; set => _nr30 = value; }
    public byte NR31 { set => _nr31 = value; }
    public byte NR32 { get => _nr32; set => _nr32 = value; }
    public byte NR33 { set => _nr33 = value; }
    public byte NR34 { get => (byte)(_nr34 & 0x40); set => _nr34 = value; }
    public byte NR41 { set => _nr41 = value; }
    public byte NR42 { get => _nr42; set => _nr42 = value; }
    public byte NR43 { get => _nr43; set => _nr43 = value; }
    public byte NR44 { get => (byte)(_nr44 & 0x40); set => _nr44 = value; }
    public byte NR50 { get => _nr50; set => _nr50 = value; }
    public byte NR51 { get => _nr51; set => _nr51 = value; }
    public byte NR52 { get => _nr52; set => _nr52 = (byte)(value & 0x80); }

    public APU()
    {
        _waveRam = new byte[0x10];
    }

    public void WriteWaveRam(ushort address, byte value)
    {
        _waveRam[address & 0x000F] = value;
    }

    public byte ReadWaveRam(ushort address)
    {
        return _waveRam[address & 0x000F];
    }
}
