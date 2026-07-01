using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Core;

public class TIMER
{
    private const int DIVCycles = 256;

    private int _divCycles = 0;
    private int _timerCycles = 0;

    private int[] _timerClocks = [1024, 16, 64, 256];

    public void Tick(int cycles, MMU mmu)
    {
        UpdateDIV(mmu, cycles);
        UpdateTimer(mmu, cycles);
    }

    private void UpdateDIV(MMU mmu, int cycles)
    {
        _divCycles += cycles;
        if (_divCycles >= DIVCycles)
        {
            mmu.DIV++;
            _divCycles -= DIVCycles;
        }
    }

    private void UpdateTimer(MMU mmu, int cycles)
    {
        byte TAC = mmu.TAC;
        if ((TAC & 0x4) == 0)
        {
            return;
        }

        _timerCycles += cycles;
        int clock = TAC & 0x3;
        while (_timerCycles >= _timerClocks[clock])
        {
            mmu.TIMA++;
            _timerCycles -= _timerClocks[clock];
        }
        if (mmu.TIMA == 0)
        {
            mmu.TIMA = mmu.TMA;
            mmu.IF |= 0x4;
        }
    }
}
