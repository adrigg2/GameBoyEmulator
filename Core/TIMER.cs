using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Core;

public class TIMER
{
    private ushort _counter = 0;
    private byte _tima = 0;
    private byte _tma = 0;
    private byte _tac = 0;

    private int _timaResetCounter = -1;
    private int _timaIgnoreWritesCounter = 0;
    
    public byte DIV 
    {
        get => (byte)(_counter >> 8);
        set
        {
            int tacFreq = _tac & 0x3;
            int timerIncBitDisplace = ((tacFreq - 1) & 0x3) * 2 + 4;
            int oldTimerIncBit = (_counter >> timerIncBitDisplace) & 0x1;

            _counter = 0;

            int timerIncBit = (_counter >> timerIncBitDisplace) & 0x1;
            if ((_tac & 0x4) > 0 && oldTimerIncBit == 1 && timerIncBit == 0)
            {
                TimerTick();
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
                _tma = value;
            }
        }
    }
    public byte TAC
    {
        get => _tac;
        set
        {
            int tacFreq = _tac & 0x3;
            int oldTimerIncBit = (_counter >> ((tacFreq - 1) & 0x3) * 2 + 4) & 0x1;
            bool tacEnabledOld = (_tac & 0x4) > 0;

            _tac = value;

            tacFreq = _tac & 0x3;
            int timerIncBit = (_counter >> ((tacFreq - 1) & 0x3) * 2 + 4) & 0x1;

            bool timerTickByClockChange = (_tac & 0x4) > 0 && oldTimerIncBit == 1 && timerIncBit == 0;
            bool timerTickByTACDisable = tacEnabledOld && oldTimerIncBit == 1 && (_tac & 0x4) == 0;
            if (timerTickByClockChange || timerTickByTACDisable)
            {
                TimerTick();
            }
        }
    }

    public void Tick(int cycles, MMU mmu)
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
            }
            else if (_timaResetCounter == 0)
            {
                _timaResetCounter--;
                mmu.IF |= 0x4;
                _tima = _tma;
                _timaIgnoreWritesCounter = 4;
            }

            int tacFreq = _tac & 0x3;
            int timerIncBitDisplace = ((tacFreq - 1) & 0x3) * 2 + 4;
            int oldTimerIncBit = (_counter >> timerIncBitDisplace) & 0x1;

            _counter++;

            int timerIncBit = (_counter >> timerIncBitDisplace) & 0x1;
            if ((_tac & 0x4) > 0 && oldTimerIncBit == 1 && timerIncBit == 0)
            {
                TimerTick();
            }
        }
    }

    private void TimerTick()
    {
        byte oldTima = _tima;
        _tima++;

        if (oldTima > 0 && _tima == 0)
        {
            _timaResetCounter = 4;
        }
    }
}
