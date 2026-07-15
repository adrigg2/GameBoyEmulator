namespace GameBoyEmulator.Core.Audio;

public class Channel3
{
    private byte _nr30;
    private byte _nr31;
    private byte _nr32;
    private byte _nr33;
    private byte _nr34;

    private byte[] _waveRam;

    private bool _active;

    public byte NR30 { get => _nr30; set => _nr30 = value; }
    public byte NR31 { set => _nr31 = value; }
    public byte NR32 { get => _nr32; set => _nr32 = value; }
    public byte NR33 { set => _nr33 = value; }
    public byte NR34 { get => (byte)(_nr34 & 0x40); set => _nr34 = value; }

    public bool Active { get => _active; }

    public Channel3()
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

    public void ClearRegisters()
    {
        _nr30 = 0;
        _nr32 = 0;
        _nr33 = 0;
        _nr34 = 0;
    }
}
