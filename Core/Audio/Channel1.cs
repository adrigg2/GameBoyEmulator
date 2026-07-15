namespace GameBoyEmulator.Core.Audio;

public class Channel1
{
    private byte _nr10;
    private byte _nr11;
    private byte _nr12;
    private byte _nr13;
    private byte _nr14;

    private int _sweepCounter;
    private int _currentPace;
    private int _lengthTimer;
    private int _periodDiv;
    private int _sweepFrequency;

    private bool _active;

    public byte NR10 { get => _nr10; set => _nr10 = value; }
    public byte NR11 { get => (byte)(_nr11 & 0xC0); set => _nr11 = value; }
    public byte NR12 { get => _nr12; set => _nr12 = value; }
    public byte NR13 { set => _nr13 = value; }
    public byte NR14 {
        get => (byte)(_nr14 & 0x40);
        set
        {
            _nr14 = value;
            if ((_nr14 & 0x80) != 0 && !_active)
            {
                _active = true;
                _lengthTimer = _nr11 & 0x3F;
                _periodDiv = _nr13 | ((_nr14 & 0x07) << 8);
                _sweepCounter = 0;
                _sweepFrequency = _nr13 | ((_nr14 & 0x07) << 8);

                int step = _nr10 & 0x07;
                if (step != 0)
                {
                    FrequencyCalculation();
                }
            }
        }
    }

    public bool Active { get => _active; }

    public void Tick()
    {

    }

    public void ClearRegisters()
    {
        _nr10 = 0;
        _nr11 &= 0x3F;
        _nr12 = 0;
        _nr13 = 0;
        _nr14 = 0;
    }

    public void FrequencySweep()
    {
        if (!_active)
        {
            return;
        }

        int period = FrequencyCalculation();

        if (_currentPace == 0)
        {
            _sweepCounter = 0;
            _currentPace = (_nr10 & 0x70) >> 4;
        }

        _sweepCounter++;
        if (_sweepCounter == _currentPace)
        {
            _sweepCounter = 0;
            _currentPace = (_nr10 & 0x70) >> 4;

            _nr13 = (byte)(period & 0xFF);
            _nr14 = (byte)(_nr14 & ~(0x7));
            _nr14 = (byte)(_nr14 | ((period >> 8) & 0x7));
            _sweepFrequency = period;

            FrequencyCalculation();
        }
    }

    public void LengthTimer()
    {
        if (!_active)
        {
            return;
        }

        int timerActive = _nr14 & 0x80;
        if (timerActive != 0)
        {
            _lengthTimer++;
            if (_lengthTimer >= 64)
            {
                _active = false;
            }
        }
    }

    public int FrequencyCalculation()
    {
        int period = _sweepFrequency;
        int direction = _nr10 & 0x08;
        int step = _nr10 & 0x07;

        int periodStep = period >> (2 * step);
        if (direction == 0)
        {
            period += periodStep;
        }
        else
        {
            period -= periodStep;
        }

        if (period > 0x7FF)
        {
            _active = false;
        }

        return period;
    }
}
