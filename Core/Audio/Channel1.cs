namespace GameBoyEmulator.Core.Audio;

public class Channel1
{
    private byte _nr10;
    private byte _nr11;
    private byte _nr12;
    private byte _nr13;
    private byte _nr14;

    private bool _active;

    public byte NR10 { get => _nr10; set => _nr10 = value; }
    public byte NR11 { get => (byte)(_nr11 & 0xC0); set => _nr11 = value; }
    public byte NR12 { get => _nr12; set => _nr12 = value; }
    public byte NR13 { set => _nr13 = value; }
    public byte NR14 { get => (byte)(_nr14 & 0x40); set => _nr14 = value; }

    public bool Active { get => _active; }

    public void ClearRegisters()
    {
        _nr10 = 0;
        _nr11 &= 0x3F;
        _nr12 = 0;
        _nr13 = 0;
        _nr14 = 0;
    }
}
