namespace GameBoyEmulator.Core.Audio;

public class Channel2
{
    private byte _nr21;
    private byte _nr22;
    private byte _nr23;
    private byte _nr24;

    private bool _active;

    public byte NR21 { get => (byte)(_nr21 & 0xC0); set => _nr21 = value; }
    public byte NR22 { get => _nr22; set => _nr22 = value; }
    public byte NR23 { set => _nr23 = value; }
    public byte NR24 { get => (byte)(_nr24 & 0x40); set => _nr24 = value; }

    public bool Active { get => _active; }

    public void ClearRegisters()
    {
        _nr21 &= 0x3F;
        _nr22 = 0;
        _nr23 = 0;
        _nr24 = 0;
    }
}
