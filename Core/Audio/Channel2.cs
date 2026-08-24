using GameBoyEmulator.SaveState.Components.APU;

namespace GameBoyEmulator.Core.Audio;

public class Channel2
{
    private byte _nr21;
    private byte _nr22;
    private byte _nr23;
    private byte _nr24;

    private readonly byte[] _dutyCycles = [
        0b00000001, // 12.5%
        0b10000001, // 25%
        0b10000111, // 50%
        0b01111110, // 75%
        ];

    private int _lengthTimer;
    private int _periodDiv;
    private int _sampleIndex;
    private int _volume;
    private int _envSweepPace;
    private int _envSweepCounter;

    private float _output;

    private bool _active;
    private bool _dacActive;
    private bool _envDir;
    private bool _envFinished;

    public byte NR21
    {
        get => (byte)(_nr21 & 0xC0);
        set
        {
            _nr21 = value;
            _lengthTimer = _nr21 & 0x3F;
        }
    }
    public byte NR22
    {
        get => _nr22;
        set
        {
            _nr22 = value;

            if ((_nr22 & 0xF8) == 0)
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
    public byte NR23 { set => _nr23 = value; }
    public byte NR24
    {
        get => (byte)(_nr24 & 0x40);
        set
        {
            _nr24 = value;
            if ((_nr24 & 0x80) != 0)
            {
                Active = true;

                if (_lengthTimer >= 64)
                {
                    _lengthTimer = _nr21 & 0x3F;
                }

                _periodDiv = _nr23 | ((_nr24 & 0x07) << 8);
                _volume = (_nr22 & 0xF0) >> 4;
                _envSweepPace = _nr22 & 0x7;
                _envDir = (_nr22 & 0x8) > 0;
                _envSweepCounter = 0;
                _envFinished = false;
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
            _periodDiv = _nr23 | ((_nr24 & 0x07) << 8);
            _periodDiv += cycles;
            _sampleIndex = ++_sampleIndex % 8;
        }

        int dutyCycle = (_nr21 & 0xC0) >> 6;
        int digitalSignal = ((_dutyCycles[dutyCycle] >> _sampleIndex) & 0x1) * _volume;
        _output = (-2.0f * digitalSignal / 15.0f) + 1.0f;
    }

    public void ClearRegisters()
    {
        Active = false;
        _nr21 &= 0x3F;
        _nr22 = 0;
        _nr23 = 0;
        _nr24 = 0;
    }

    public void LengthTimer()
    {
        if (!Active)
        {
            return;
        }

        int timerActive = _nr24 & 0x40;
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
            else if (_envDir && _volume < 0xF)
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

    public Channel2State SaveState()
    {
        return new Channel2State(
            _nr21,
            _nr22,
            _nr23,
            _nr24,
            _lengthTimer,
            _periodDiv,
            _sampleIndex,
            _volume,
            _envSweepPace,
            _envSweepCounter,
            _output,
            _active,
            _dacActive,
            _envDir,
            _envFinished
            );
    }

    public void LoadState(Channel2State state)
    {
        _nr21 = state.NR21;
        _nr22 = state.NR22;
        _nr23 = state.NR23;
        _nr24 = state.NR24;
        _lengthTimer = state.LengthTimer;
        _periodDiv = state.PeriodDiv;
        _sampleIndex = state.SampleIndex;
        _volume = state.Volume;
        _envSweepPace = state.EnvSweepPace;
        _envSweepCounter = state.EnvSweepCounter;
        _output = state.Output;
        _active = state.Active;
        _dacActive = state.DacActive;
        _envDir = state.EnvDir;
        _envFinished = state.EnvFinished;
    }
}
