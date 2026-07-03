using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Core;

public class DMA(MMU mmu)
{
    private const int MaxCycles = 640;
    private const int CyclesPerTransfer = 4;
    
    private ushort _address = 0;

    private int _cycles = 0;
    private int _transfers = 0;

    private bool _active = false;

    private MMU _mmu = mmu;

    public void StartTransfer(byte address)
    {
        _address = (ushort)(address << 8);
        _active = true;
        _cycles = 0;
        _transfers = 0;
    }

    public void Tick(int cycles)
    {
        if (!_active)
        {
            return;
        }

        _cycles += cycles;

        for (int i = _cycles - _transfers * 4; i > CyclesPerTransfer && _transfers * CyclesPerTransfer < MaxCycles; i -= 4)
        {
            byte transferedByte = _mmu.ReadByte((ushort)(_address + _transfers));
            _mmu.WriteByte((ushort)(0xFE00 + _transfers), transferedByte);
            _transfers++;
        }
        
        if (_cycles >= MaxCycles)
        {
            _active = false;
        }
    }
}
