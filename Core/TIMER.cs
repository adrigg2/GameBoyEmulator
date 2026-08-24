using GameBoyEmulator.SaveState.Components;

namespace GameBoyEmulator.Core;

public class TIMER
{
    private ushort _counter = 0;
    private byte _tima = 0;
    private byte _tma = 0;
    private byte _tac = 0;

    private int _timaResetCounter = -1;
    private int _timaIgnoreWritesCounter = 0;
    private int _divApu = 0;

    public byte DIV
    {
        get => (byte)(_counter >> 8);
        set
        {
            int timerIncBitDisplace = GetTimerBit();
            int oldTimerIncBit = (_counter >> timerIncBitDisplace) & 0x1;
            int oldTimerApuBit = (_counter >> 12) & 0x1;

            _counter = 0;

            int timerIncBit = (_counter >> timerIncBitDisplace) & 0x1;
            int timerApuBit = (_counter >> 12) & 0x1;
            if ((_tac & 0x4) > 0 && oldTimerIncBit == 1 && timerIncBit == 0)
            {
                TimerTick();
            }

            if (oldTimerApuBit == 1 && timerApuBit == 0)
            {
                _divApu++;
            }
        }
    }
    public byte TIMA
    {
        get => _tima;
        set
        {
            if (_timaIgnoreWritesCounter <= 0)
            {
                _tima = value;
            }
            _timaResetCounter = -1;
        }
    }
    public byte TMA
    {
        get => _tma;
        set
        {
            if (_timaIgnoreWritesCounter > 0)
            {
                _tima = value;
            }
            _tma = value;
        }
    }
    public byte TAC
    {
        get => _tac;
        set
        {
            int oldTimerIncBit = (_counter >> GetTimerBit()) & 0x1;
            bool tacEnabledOld = (_tac & 0x4) > 0;

            _tac = value;

            int timerIncBit = (_counter >> GetTimerBit()) & 0x1;

            bool timerTickByClockChange = (_tac & 0x4) > 0 && oldTimerIncBit == 1 && timerIncBit == 0;
            bool timerTickByTACDisable = tacEnabledOld && oldTimerIncBit == 1 && (_tac & 0x4) == 0;
            if (timerTickByClockChange || timerTickByTACDisable)
            {
                TimerTick();
            }
        }
    }

    public int Tick(int cycles, MMU mmu)
    {
        for (int i = 0; i < cycles; i++)
        {
            if (_timaIgnoreWritesCounter > 0)
            {
                _timaIgnoreWritesCounter--;
            }

            if (_timaResetCounter > 0)
            {
                _timaResetCounter--;
                if (_timaResetCounter == 0)
                {
                    _timaResetCounter--;
                    mmu.IF |= 0x4;
                    _tima = _tma;
                    _timaIgnoreWritesCounter = 4;
                }
            }

            int timerIncBitDisplace = GetTimerBit();
            int oldTimerIncBit = (_counter >> timerIncBitDisplace) & 0x1;
            int oldTimerApuBit = (_counter >> 12) & 0x1;

            _counter++;

            int timerIncBit = (_counter >> timerIncBitDisplace) & 0x1;
            int timerApuBit = (_counter >> 12) & 0x1;
            if ((_tac & 0x4) > 0 && oldTimerIncBit == 1 && timerIncBit == 0)
            {
                TimerTick();
            }

            if (oldTimerApuBit == 1 && timerApuBit == 0)
            {
                _divApu++;
            }
        }

        return _divApu;
    }

    public TIMERState SaveState()
    {
        return new TIMERState(
            _timaResetCounter,
            _timaIgnoreWritesCounter,
            _divApu,
            _counter,
            _tima,
            _tma,
            _tac
            );
    }

    public void LoadState(TIMERState state)
    {
        _timaResetCounter = state.TIMAResetCounter;
        _timaIgnoreWritesCounter = state.TIMAIgnoreWritesCounter;
        _divApu = state.DivAPU;
        _counter = state.Counter;
        _tima = state.TIMA;
        _tma = state.TMA;
        _tac = state.TAC;
    }

    private void TimerTick()
    {
        if (_tima++ == 0xFF)
        {
            _timaResetCounter = 4;
        }
    }

    private int GetTimerBit()
    {
        int tacFreq = _tac & 0x3;
        return ((tacFreq - 1) & 0x3) * 2 + 3;
    }
}
