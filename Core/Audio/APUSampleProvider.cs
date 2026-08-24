using NAudio.Wave;

namespace GameBoyEmulator.Core.Audio;

public class APUSampleProvider(WaveFormat waveFormat, int capacity) : ISampleProvider
{
    private readonly RingBuffer _ringBuffer = new(capacity);
    public WaveFormat WaveFormat { get; } = waveFormat;

    public int SampleCount { get => _ringBuffer.Count; }

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

    public void Clear()
    {
        _ringBuffer.Clear();
    }
}
