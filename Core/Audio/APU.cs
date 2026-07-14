namespace GameBoyEmulator.Core.Audio;

public class APU
{
    private byte _nr50;
    private byte _nr51;
    private byte _nr52;

    private Channel1 _channel1;
    private Channel2 _channel2;
    private Channel3 _channel3;
    private Channel4 _channel4;
    
    public byte NR50 { get => _nr50; set => _nr50 = value; }
    public byte NR51 { get => _nr51; set => _nr51 = value; }
    public byte NR52 { get => _nr52; set => _nr52 = (byte)(value & 0x80); }

    public Channel1 Channel1 { get => _channel1; }
    public Channel2 Channel2 { get => _channel2; }
    public Channel3 Channel3 { get => _channel3; }
    public Channel4 Channel4 { get => _channel4; }

    public APU()
    {
        _channel1 = new();
        _channel2 = new();
        _channel3 = new();
        _channel4 = new();
    }
}
