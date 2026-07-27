using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace GameBoyEmulator.Core.Audio;

public class APU
{
    private const float CyclesPerSample = 4194304.0f / 44100.0f;
    private const float Charge = 0.996f;

    private byte _nr50;
    private byte _nr51;
    private byte _nr52;

    private int _oldDivApu;

    private float _sampleCycles;
    private float _capacitorL, _capacitorR;

    private bool _active;

    private bool _channel1a, _channel2a, _channel3a, _channel4a; // debug

    private Channel1 _channel1;
    private Channel2 _channel2;
    private Channel3 _channel3;
    private Channel4 _channel4;

    private APUSampleProvider _sampleProvider;
    private WasapiOut _out;
    
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
            _active = true;
            if ((_nr52 & 0x80) == 0)
            {
                _active = false;
                _nr50 = 0;
                _nr51 = 0;
                _capacitorL = 0;
                _capacitorR = 0;

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

    public APUSampleProvider SampleProvider { get => _sampleProvider; }

    public APU()
    {
        _channel1 = new();
        _channel2 = new();
        _channel3 = new();
        _channel4 = new();

        var format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        _sampleProvider = new APUSampleProvider(format, 70560);
        _out = new WasapiOut(AudioClientShareMode.Shared, 200);
        _out.Init(_sampleProvider);

        _channel1a = true;
        _channel2a = true;
        _channel3a = true;
        _channel4a = true;
    }

    //DEBUG
    public void ToggleChannel(int channel)
    {
        switch (channel)
        {
            case 1:
                _channel1a = !_channel1a;
                break;
            case 2:
                _channel2a = !_channel2a;
                break;
            case 3:
                _channel3a = !_channel3a;
                break;
            case 4:
                _channel4a = !_channel4a;
                break;
        }
    }

    public void Tick(int cycles, int divApuCounter)
    {
        if (!_active)
        {
            return;
        }

        _sampleCycles += cycles;

        if (divApuCounter != _oldDivApu)
        {
            if (divApuCounter % 2 == 0)
            {
                _channel1.LengthTimer();
                _channel2.LengthTimer();
                _channel3.LengthTimer();
                _channel4.LengthTimer();
            }

            if (divApuCounter % 4 == 0)
            {
                _channel1.FrequencySweep();
            }

            if (divApuCounter % 8 == 0)
            {
                _channel1.EnvelopeSweep();
                _channel2.EnvelopeSweep();
                _channel4.EnvelopeSweep();
            }

            _oldDivApu = divApuCounter;
        }

        _channel1.Tick(cycles);
        _channel2.Tick(cycles);
        _channel3.Tick(cycles);
        _channel4.Tick(cycles);

        if (_sampleCycles >= CyclesPerSample)
        {
            _sampleCycles -= CyclesPerSample;
            GenerateSample();
        }
    }

    public void StartAudio()
    {
        _out.Play();
    }

    private void GenerateSample()
    {
        float left = 0;
        float right = 0;

        if ((_nr51 & 0x80) != 0 && _channel4a)
        {
            left += _channel4.Output;
        }

        if ((_nr51 & 0x40) != 0 && _channel3a)
        {
            left += _channel3.Output;
        }

        if ((_nr51 & 0x20) != 0 && _channel2a)
        {
            left += _channel2.Output;
        }

        if ((_nr51 & 0x10) != 0 && _channel1a)
        {
            left += _channel1.Output;
        }

        if ((_nr51 & 0x08) != 0 && _channel4a)
        {
            right += _channel4.Output;
        }

        if ((_nr51 & 0x04) != 0 && _channel3a)
        {
            right += _channel3.Output;
        }

        if ((_nr51 & 0x02) != 0 && _channel2a)
        {
            right += _channel2.Output;
        }

        if ((_nr51 & 0x01) != 0 && _channel1a)
        {
            right += _channel1.Output;
        }

        float volRight = ((_nr50 & 0x07) + 1) / 8.0f;
        float volLeft = (((_nr50 & 0x70) >> 4) + 1) / 8.0f;

        left = left / 4 * volLeft;
        right = right / 4 * volRight;

        left = HighPass(left, ref _capacitorL);
        right = HighPass(right, ref _capacitorR);

        left *= 0.2f;
        right *= 0.2f;

        _sampleProvider.WriteSample(left);
        _sampleProvider.WriteSample(right);
    }

    private float HighPass(float input, ref float capacitor)
    {
        float output = input - capacitor;
        capacitor = input - output * Charge;
        return output;
    }
}
