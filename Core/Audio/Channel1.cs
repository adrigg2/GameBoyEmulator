namespace GameBoyEmulator.Core.Audio;

public class Channel1
{
    private byte _nr10;
    private byte _nr11;
    private byte _nr12;
    private byte _nr13;
    private byte _nr14;

    private readonly byte[] _dutyCycles = [
        0b00000001, // 12.5%
        0b10000001, // 25%
        0b10000111, // 50%
        0b01111110, // 75%
        ];

    private int _sweepCounter;
    private int _currentPace;
    private int _lengthTimer;
    private int _periodDiv;
    private int _sweepFrequency;
    private int _sampleIndex;
    private int _volume;
    private int _envSweepPace;
    private int _envSweepCounter;

    private float _output;

    private bool _active;
    private bool _dacActive;
    private bool _envDir;
    private bool _sweepEnabled;
    private bool _envFinished;

    public byte NR10 { get => _nr10; set => _nr10 = value; }
    public byte NR11 
    {
        get => (byte)(_nr11 & 0xC0);
        set
        {
            _nr11 = value;
            _lengthTimer = _nr11 & 0x3F;
        }
    }
    public byte NR12
    {
        get => _nr12;
        set
        {
            _nr12 = value;

            if ((_nr12 & 0xF8) == 0)
            {
                _active = false;
                _dacActive = false;
            }
            else
            {
                _dacActive = true;
            }
        }
    }
    public byte NR13 { set => _nr13 = value; }
    public byte NR14 {
        get => (byte)(_nr14 & 0x40);
        set
        {
            _nr14 = value;
            if ((_nr14 & 0x80) != 0)
            {
                Active = true;

                if (_lengthTimer >= 64)
                {
                    _lengthTimer = _nr11 & 0x3F;
                }

                _periodDiv = _nr13 | ((_nr14 & 0x07) << 8);
                _sweepCounter = 0;
                _sweepFrequency = _nr13 | ((_nr14 & 0x07) << 8);
                _volume = (_nr12 & 0xF0) >> 4;
                _envSweepPace = _nr12 & 0x7;
                _envDir = (_nr12 & 0x8) > 0;
                _envSweepCounter = 0;
                _envFinished = false;

                int step = _nr10 & 0x07;
                int pace = (_nr10 & 0x70) >> 4;
                _sweepEnabled = pace != 0 || step != 0;
                _currentPace = pace == 0 ? 8 : pace;
                if (step != 0)
                {
                    FrequencyCalculation();
                }
            }
        }
    }

    public bool Active { get => _active; private set => _active = value && _dacActive; }

    public float Output { get => _output; }

    public void Tick(int cycles)
    {
        if (!_dacActive)
        {
            _output = 0;
            return;
        }

        if (!Active)
        {
            _output = 1;
            return;
        }

        _periodDiv += cycles / 4;
        if (_periodDiv > 0x7FF)
        {
            cycles = _periodDiv - 0x800;
            _periodDiv = _nr13 | ((_nr14 & 0x07) << 8);
            _periodDiv += cycles;
            _sampleIndex = ++_sampleIndex % 8;
        }

        int dutyCycle = (_nr11 & 0xC0) >> 6;
        int digitalSignal = ((_dutyCycles[dutyCycle] >> _sampleIndex) & 0x1) * _volume;
        _output = (-2.0f * digitalSignal / 15.0f) + 1.0f;
    }

    public void ClearRegisters()
    {
        Active = false;
        _nr10 = 0;
        _nr11 &= 0x3F;
        _nr12 = 0;
        _nr13 = 0;
        _nr14 = 0;
        _sampleIndex = 0;
    }

    public void FrequencySweep()
    {
        if (!Active || !_sweepEnabled)
        {
            return;
        }

        //if (_currentPace == 0)
        //{
        //    int pace = (_nr10 & 0x70) >> 4;
        //    _sweepCounter = 0;
        //    _currentPace = pace == 0 ? 8 : pace;
        //}
        _sweepCounter++;

        if (_sweepCounter < _currentPace)
        {
            return;
        }

        _sweepCounter = 0;
        int pace = (_nr10 & 0x70) >> 4;
        _currentPace = pace == 0 ? 8 : pace;

        int period = FrequencyCalculation();
        int step = _nr10 & 0x07;

        if (period <= 0x7FF && step != 0)
        {
            _nr13 = (byte)(period & 0xFF);
            _nr14 = (byte)(_nr14 & ~(0x7));
            _nr14 = (byte)(_nr14 | ((period >> 8) & 0x7));
            _sweepFrequency = period;
        }

        FrequencyCalculation();
    }

    public void LengthTimer()
    {
        if (!Active)
        {
            return;
        }

        int timerActive = _nr14 & 0x40;
        if (timerActive != 0)
        {
            _lengthTimer++;
            if (_lengthTimer >= 64)
            {
                Active = false;
            }
        }
    }

    public void EnvelopeSweep()
    {
        if (!Active || _envSweepPace == 0 || _envFinished)
        {
            return;
        }

        _envSweepCounter++;
        if (_envSweepCounter >= _envSweepPace)
        {
            if (!_envDir && _volume > 0)
            {
                _volume--;
            }
            else if (_envDir && _volume < 0x0F)
            {
                _volume++;
            }
            else
            {
                _envFinished = true;
            }

            _envSweepCounter = 0;
        }
    }

    private int FrequencyCalculation()
    {
        int period = _sweepFrequency;
        int direction = _nr10 & 0x08;
        int step = _nr10 & 0x07;

        int periodStep = period >> step;
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
            Active = false;
        }

        return period;
    }
}
