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

    private byte _div = 0;
    
    public byte DIV { get => _div; set => _div = 0; }
    public byte TIMA { get; set; }
    public byte TMA { get; set; }
    public byte TAC { get; set; }

    public void Tick(int cycles, MMU mmu)
    {
        UpdateDIV(cycles);
        UpdateTimer(mmu, cycles);
    }

    private void UpdateDIV(int cycles)
    {
        _divCycles += cycles;
        if (_divCycles >= DIVCycles)
        {
            DIV++;
            _divCycles -= DIVCycles;
        }
    }

    private void UpdateTimer(MMU mmu, int cycles)
    {
        if ((TAC & 0x4) == 0)
        {
            return;
        }

        _timerCycles += cycles;
        int clock = TAC & 0x3;
        while (_timerCycles >= _timerClocks[clock])
        {
            TIMA++;
            _timerCycles -= _timerClocks[clock];
        }
        if (TIMA == 0)
        {
            TIMA = TMA;
            mmu.IF |= 0x4;
        }
    }
}
