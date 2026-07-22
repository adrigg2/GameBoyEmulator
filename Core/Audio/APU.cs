using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Printing;

namespace GameBoyEmulator.Core.Audio;

public class APU
{
    private const float CyclesPerSample = 4194304.0f / 44100.0f;

    private byte _nr50;
    private byte _nr51;
    private byte _nr52;

    private float _sampleCycles;

    private bool _active;

    private Channel1 _channel1;
    private Channel2 _channel2;
    private Channel3 _channel3;
    private Channel4 _channel4;

    private APUSampleProvider _waveProvider;
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

        var format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        _waveProvider = new APUSampleProvider(format);
        _out = new WasapiOut(AudioClientShareMode.Shared, 50);
        _out.Init(_waveProvider);
        _out.Play();
    }

    public void Tick(int cycles, int divApuCounter)
    {
        if (!_active)
        {
            return;
        }

        _sampleCycles += cycles;

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

    private void GenerateSample()
    {
        float left = 0;
        float right = 0;

        if ((_nr51 & 0x80) != 0)
        {
            left += _channel4.Output;
        }

        if ((_nr51 & 0x40) != 0)
        {
            left += _channel3.Output;
        }

        if ((_nr51 & 0x20) != 0)
        {
            left += _channel2.Output;
        }

        if ((_nr51 & 0x10) != 0)
        {
            left += _channel1.Output;
        }

        if ((_nr51 & 0x08) != 0)
        {
            right += _channel4.Output;
        }

        if ((_nr51 & 0x04) != 0)
        {
            right += _channel3.Output;
        }

        if ((_nr51 & 0x02) != 0)
        {
            right += _channel2.Output;
        }

        if ((_nr51 & 0x01) != 0)
        {
            right += _channel1.Output;
        }

        float volRight = ((_nr50 & 0x07) + 1) / 8.0f;
        float volLeft = (((_nr50 & 0x70) >> 4) + 1) / 8.0f;

        left = (left / 4) * volLeft;
        right = (right / 4) * volRight;

        _waveProvider.WriteSample(left);
        _waveProvider.WriteSample(right);
    }
}
