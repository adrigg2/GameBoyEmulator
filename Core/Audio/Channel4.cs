namespace GameBoyEmulator.Core.Audio;

public class Channel4
{
    private byte _nr41;
    private byte _nr42;
    private byte _nr43;
    private byte _nr44;

    private bool _active;

    public byte NR41 { set => _nr41 = value; }
    public byte NR42 { get => _nr42; set => _nr42 = value; }
    public byte NR43 { get => _nr43; set => _nr43 = value; }
    public byte NR44 { get => (byte)(_nr44 & 0x40); set => _nr44 = value; }

    public bool Active { get => _active; }

    public void ClearRegisters()
    {
        _nr41 &= 0x3F;
        _nr42 = 0;
        _nr43 = 0;
        _nr44 = 0;
    }
}
