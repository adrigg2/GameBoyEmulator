namespace GameBoyEmulator.Core.Audio;

public class RingBuffer(int capacity)
{
    private readonly float[] _buffer = new float[capacity];
    private readonly int _capacity = capacity;
    private int _head;
    private int _tail;
    private int _count;
    private readonly object _lock = new();

    public int Count { get { lock (_lock) return _count; } }

    public void Write(float f)
    {
        lock (_lock)
        {
            if (_count == _capacity)
            {
                _head = (_head + 1) % _capacity;
                _count--;
            }

            _buffer[_tail] = f;
            _tail = (_tail + 1) % _capacity;
            _count++;
        }
    }

    public int Read(float[] destination, int offset, int count)
    {
        lock (_lock)
        {
            int toRead = Math.Min(_count, count);
            for (int i = 0; i < toRead; i++)
            {
                destination[offset + i] = _buffer[_head];
                _head = (_head + 1) % _capacity;
            }
            _count -= toRead;
            return toRead;
        }
    }

    public void Clear()
    {
        _count = 0;
        _head = 0;
        _tail = 0;
    }
}
