namespace GameBoyEmulator.Core.Audio;

public class Channel2
{
    private byte _nr21;
    private byte _nr22;
    private byte _nr23;
    private byte _nr24;

    private readonly byte[] _dutyCycles = [
        0b01111111,
        0b01111110,
        0b00011110,
        0b10000001,
        ];

    private int _lengthTimer;
    private int _periodDiv;
    private int _sampleIndex;
    private int _volume;
    private int _envSweepPace;
    private int _envSweepCounter;

    private bool _active;
    private bool _envDir;

    public byte NR21 { get => (byte)(_nr21 & 0xC0); set => _nr21 = value; }
    public byte NR22
    {
        get => _nr22;
        set
        {
            _nr22 = value;
            if ((_nr22 & 0xF8) == 0)
            {
                _active = false;
            }
        }
    }
    public byte NR23 { set => _nr23 = value; }
    public byte NR24
    {
        get => (byte)(_nr24 & 0x40);
        set
        {
            _nr24 = value;
            if ((_nr24 & 0x80) != 0 && !_active)
            {
                _active = true;
                _lengthTimer = _nr21 & 0x3F;
                _periodDiv = _nr23 | ((_nr24 & 0x07) << 8);
                _volume = (_nr22 & 0xF0) >> 4;
                _envSweepPace = _nr22 & 0x7;
                _envDir = (_nr22 & 0x8) > 0;
            }
        }
    }

    public bool Active { get => _active; }

    public int Tick()
    {
        if (!_active)
        {
            return 0;
        }

        if (++_periodDiv > 0x7FF)
        {
            _periodDiv = _nr23 | ((_nr24 & 0x07) << 8);
            _sampleIndex = ++_sampleIndex % 8;
        }

        int dutyCycle = (_nr21 & 0xC0) >> 6;
        return ((_dutyCycles[dutyCycle] >> _sampleIndex) & 0x1) * _volume;
    }

    public void ClearRegisters()
    {
        _active = false;
        _nr21 &= 0x3F;
        _nr22 = 0;
        _nr23 = 0;
        _nr24 = 0;
    }

    public void LengthTimer()
    {
        if (!_active)
        {
            return;
        }

        int timerActive = _nr24 & 0x80;
        if (timerActive != 0)
        {
            _lengthTimer++;
            if (_lengthTimer >= 64)
            {
                _active = false;
            }
        }
    }

    public void EnvelopeSweep()
    {
        if (!_active || _envSweepPace == 0)
        {
            return;
        }

        _envSweepCounter++;
        if (_envSweepCounter >= _envSweepPace)
        {
            if (_envDir && _volume > 0)
            {
                _volume--;
            }
            else if (!_envDir && _volume < 0xF)
            {
                _volume++;
            }

            _envSweepCounter = 0;
        }
    }
}
