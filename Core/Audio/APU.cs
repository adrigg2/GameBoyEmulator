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
    public byte NR52 
    {
        get
        {
            byte nr52 = _nr52;
            if (_channel1.Active)
            {
                nr52 |= 0x1;
            }

            if (_channel2.Active)
            {
                nr52 |= 0x2;
            }

            if (_channel3.Active)
            {
                nr52 |= 0x4;
            }

            if (_channel4.Active)
            {
                nr52 |= 0x8;
            }

            return nr52;
        }
        set 
        {
            _nr52 = (byte)(value & 0x80);
            if ((_nr52 & 0x80) == 0)
            {
                _nr50 = 0;
                _nr51 = 0;

                _channel1.ClearRegisters();
                _channel2.ClearRegisters();
                _channel3.ClearRegisters();
                _channel4.ClearRegisters();
            }
        }
    }

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

    public void Tick(int cycles, int divApuCounter)
    {
        if (divApuCounter % 2 == 0)
        {
            _channel1.LengthTimer();
            _channel2.LengthTimer();
            _channel3.LengthTimer();
            // sound length
        }

        if (divApuCounter % 4 == 0)
        {
            _channel1.FrequencySweep();
        }

        if (divApuCounter % 8 == 0)
        {
            _channel1.EnvelopeSweep();
            _channel2.EnvelopeSweep();
            // envelope sweep
        }

        for (int i = 0; i < cycles; i++)
        {
            if (i % 2 == 0)
            {
                _channel3.Tick();
            }

            if (i % 4 == 0)
            {
                _channel1.Tick();
                _channel2.Tick();
            }
        }
    }
}
