using GameBoyEmulator.SaveState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Core;

public class DMA
{
    private const int MaxCycles = 640;
    private const int CyclesPerTransfer = 4;
    
    private ushort _address = 0;

    private int _cycles = 0;
    private int _transfers = 0;

    private bool _active = false;

    public byte Address { get => (byte)(_address >> 8); set => StartTransfer(value); }

    public void StartTransfer(byte address)
    {
        if (address > 0xDF || _active)
        {
            return;
        }

        _address = (ushort)(address << 8);
        _active = true;
        _cycles = 0;
        _transfers = 0;
    }

    public void Tick(int cycles, MMU mmu)
    {
        if (!_active)
        {
            return;
        }

        _cycles += cycles;

        for (int i = _cycles - _transfers * 4; i > CyclesPerTransfer && _transfers * CyclesPerTransfer < MaxCycles; i -= 4)
        {
            byte transferedByte = mmu.ReadByte((ushort)(_address + _transfers));
            mmu.WriteByte((ushort)(0xFE00 + _transfers), transferedByte);
            _transfers++;
        }
        
        if (_cycles >= MaxCycles)
        {
            _active = false;
        }
    }

    public DMAState SaveState()
    {
        return new DMAState(_address, _cycles, _transfers, _active);
    }

    public void LoadState(DMAState state)
    {
        _address = state.Address;
        _cycles = state.Cycles;
        _transfers = state.Transfers;
        _active = state.Active;
    }
}
