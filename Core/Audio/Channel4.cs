using GameBoyEmulator.SaveState.Components.APU;

namespace GameBoyEmulator.Core.Audio;

public class Channel4
{
    private byte _nr41;
    private byte _nr42;
    private byte _nr43;
    private byte _nr44;

    private ushort _lsfr;

    private int _lengthTimer;
    private int _volume;
    private int _envSweepPace;
    private int _envSweepCounter;
    private int _channelFrequency;
    private int _channelCycles;

    private float _output;

    private bool _active;
    private bool _dacActive;
    private bool _envDir;
    private bool _envFinished;

    public byte NR41
    {
        set
        {
            _nr41 = value;
            _lengthTimer = _nr41 & 0x3F;
        }
    }
    public byte NR42
    {
        get => _nr42;
        set
        {
            _nr42 = value;

            if ((_nr42 & 0xF8) == 0)
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
    public byte NR43 { get => _nr43; set => _nr43 = value; }
    public byte NR44
    {
        get => (byte)(_nr44 & 0x40);
        set
        {
            _nr44 = value;
            if ((_nr44 & 0x80) != 0)
            {
                Active = true;

                if (_lengthTimer >= 64)
                {
                    _lengthTimer = _nr41 & 0x3F;
                }

                ResetFrequency();
                
                _volume = (_nr42 & 0xF0) >> 4;
                _envSweepPace = _nr42 & 0x7;
                _envDir = (_nr42 & 0x8) > 0;
                _envSweepCounter = 0;
                _envFinished = false;
                _lsfr = 0;
            }
        }
    }

    public bool Active { get => _active; private set => _active = value && _dacActive; }

    public float Output { get => _output; }

    public void ClearRegisters()
    {
        _nr41 &= 0x3F;
        _nr42 = 0;
        _nr43 = 0;
        _nr44 = 0;
    }

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

        if (_channelFrequency == 0)
        {
            return;
        }

        _channelCycles += cycles;
        if (_channelCycles >= _channelFrequency)
        {
            _channelCycles -= _channelFrequency;
            ResetFrequency();
            int digitalSignal = UpdateLsfr() * _volume;
            _output = (-2.0f * digitalSignal / 15.0f) + 1.0f;
        }
    }

    public void LengthTimer()
    {
        if (!Active)
        {
            return;
        }

        int timerActive = _nr44 & 0x40;
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

    public Channel4State SaveState()
    {
        return new Channel4State(
            _nr41,
            _nr42,
            _nr43,
            _nr44,
            _lsfr,
            _lengthTimer,
            _volume,
            _envSweepPace,
            _envSweepCounter,
            _channelFrequency,
            _channelCycles,
            _output,
            _active,
            _dacActive,
            _envDir,
            _envFinished
            );
    }

    public void LoadState(Channel4State state)
    {
        _nr41 = state.NR41;
        _nr42 = state.NR42;
        _nr43 = state.NR43;
        _nr44 = state.NR44;
        _lsfr = state.LSFR;
        _lengthTimer = state.LengthTimer;
        _volume = state.Volume;
        _envSweepPace = state.EnvSweepPace;
        _envSweepCounter = state.EnvSweepCounter;
        _channelFrequency = state.ChannelFrequency;
        _channelCycles = state.ChannelCycles;
        _output = state.Output;
        _active = state.Active;
        _dacActive = state.DacActive;
        _envDir = state.EnvDir;
        _envFinished = state.EnvFinished;
    }

    private void ResetFrequency()
    {
        int clockDiv = (_nr43 & 0x07) * 16;
        if (clockDiv == 0)
        {
            clockDiv = 8;
        }

        int clockShift = (_nr43 & 0xF0) >> 4;
        if (clockShift >= 14)
        {
            _channelFrequency = 0;
            return;
        }

        _channelFrequency = clockDiv << clockShift;
    }

    private int UpdateLsfr()
    {
        int bitToWrite = (~((_lsfr & 0x1) ^ ((_lsfr >> 1) & 0x1))) & 0x1;
        _lsfr = (ushort)(_lsfr & ~0x8000);
        _lsfr = (ushort)(_lsfr | (bitToWrite << 15));

        if ((_nr43 & 0x08) != 0)
        {
            _lsfr = (ushort)(_lsfr & ~0x80);
            _lsfr = (ushort)(_lsfr | (bitToWrite << 7));
        }

        _lsfr >>= 1;

        return _lsfr & 0x1;
    }
}
