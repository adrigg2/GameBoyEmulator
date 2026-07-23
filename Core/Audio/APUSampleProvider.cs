using NAudio.Wave;
using System.Collections.Concurrent;

namespace GameBoyEmulator.Core.Audio;

public class APUSampleProvider : ISampleProvider
{
    private readonly RingBuffer _ringBuffer;
    public WaveFormat WaveFormat { get; }

    public int SampleCount { get => _ringBuffer.Count; }

    public APUSampleProvider(WaveFormat waveFormat, int capacity)
    {
        WaveFormat = waveFormat;
        _ringBuffer = new(capacity);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int written = _ringBuffer.Read(buffer, offset, count);

        while (written < count)
        {
            buffer[offset + written++] = 0f;
        }
        return count;
    }

    public void WriteSample(float sample)
    {
        _ringBuffer.Write(sample);
    }
}
