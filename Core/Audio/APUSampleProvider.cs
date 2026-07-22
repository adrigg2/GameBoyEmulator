using NAudio.Wave;
using System.Collections.Concurrent;

namespace GameBoyEmulator.Core.Audio;

public class APUSampleProvider : ISampleProvider
{
    private readonly ConcurrentQueue<float> _ringBuffer;
    public WaveFormat WaveFormat { get; }

    public APUSampleProvider(WaveFormat waveFormat)
    {
        WaveFormat = waveFormat;
        _ringBuffer = new();
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int written = 0;
        while (written < count && _ringBuffer.TryDequeue(out float sample))
        {
            buffer[offset + written++] = sample;
        }

        while (written < count)
        {
            buffer[offset + written++] = 0f;
        }
        return count;
    }

    public void WriteSample(float sample)
    {
        _ringBuffer.Enqueue(sample);
    }
}
