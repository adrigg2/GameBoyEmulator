namespace GameBoyEmulator.Core.Audio;

public class Channel3
{
    private byte _nr30;
    private byte _nr31;
    private byte _nr32;
    private byte _nr33;
    private byte _nr34;

    private readonly byte[] _waveRam;

    private int _lengthTimer;
    private int _periodDiv;
    private int _volume;
    private int _waveIndex;
    private int _waveBuffer;

    private float _output;

    private bool _active;
    private bool _dacActive;

    public byte NR30
    {
        get => _nr30;
        set
        {
            _nr30 = value;
            if ((_nr30 & 0x80) == 0)
            {
                _dacActive = false;
                _active = false;
            }
            else
            {
                _dacActive = true;
            }
        }
    }
    public byte NR31
    {
        set
        {
            _nr31 = value;
            _lengthTimer = _nr31;
        }
    }
    public byte NR32 { get => _nr32; set => _nr32 = value; }
    public byte NR33 { set => _nr33 = value; }
    public byte NR34
    {
        get => (byte)(_nr34 & 0x40);
        set
        {
            _nr34 = value;
            if ((_nr34 & 0x80) != 0)
            {
                Active = true;

                if (_lengthTimer >= 256)
                {
                    _lengthTimer = _nr31;
                }

                _periodDiv = _nr33 | ((_nr34 & 0x07) << 8);
                _volume = (_nr32 & 0x60) >> 5;
                _waveIndex = 0;
            }
        }
    }

    public bool Active { get => _active; private set => _active = value && _dacActive; }

    public float Output { get => _output; }

    public Channel3()
    {
        _waveRam = new byte[0x10];
    }

    public void WriteWaveRam(ushort address, byte value)
    {
        _waveRam[address & 0x000F] = value;
    }

    public byte ReadWaveRam(ushort address)
    {
        return _waveRam[address & 0x000F];
    }

    public void ClearRegisters()
    {
        _nr30 = 0;
        _nr32 = 0;
        _nr33 = 0;
        _nr34 = 0;
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

        _periodDiv += cycles / 2;
        if (_periodDiv > 0x7FF)
        {
            cycles = _periodDiv - 0x800;
            _periodDiv = _nr33 | ((_nr34 & 0x07) << 8);
            _periodDiv += cycles;
            _waveIndex = ++_waveIndex % 32;
            byte waveByte = _waveRam[_waveIndex / 2];
            _waveBuffer = _waveIndex % 2 == 0 ? waveByte >> 4 : waveByte & 0x0F;
        }

        if (_volume != 0)
        {
            int digitalSignal = _waveBuffer >> (_volume - 1);
            _output = (-2.0f * digitalSignal / 15.0f) + 1.0f;
        }
        else
        {
            _output = 1;
        }
    }

    public void LengthTimer()
    {
        if (!Active)
        {
            return;
        }

        int timerActive = _nr34 & 0x40;
        if (timerActive != 0)
        {
            _lengthTimer++;
            if (_lengthTimer >= 256)
            {
                Active = false;
            }
        }
    }
}
