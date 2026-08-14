namespace GameBoyEmulator.SaveState;

public class RewindStack(int capacity)
{
    private readonly SaveState[] _stack = new SaveState[capacity];
    private readonly int _capacity = capacity;
    private int _top;
    private int _count;

    public int Count { get => _count; }

    public void Push(SaveState saveState)
    {
        _stack[_top] = saveState;
        _top = (_top + 1) % _capacity;
        if (_count < _capacity)
        {
            _count++;
        }
    }

    public SaveState Pop()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("The stack is empty");
        }

        _top = (_top + _capacity - 1) % _capacity;
        if (_count > 0)
        {
            _count--;
        }
        return _stack[_top];
    }

    public SaveState Peek()
    {
        return _stack[(_top + _capacity - 1) % _capacity];
    }
}
