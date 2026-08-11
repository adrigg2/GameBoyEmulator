using GameBoyEmulator.SaveState.Components;
using GameBoyEmulator.SaveState.Components.APU;

namespace GameBoyEmulator.SaveState;

public record SaveState(CPUState CPU, DMAState DMA, JOYPADState JOYPAD, MMUState MMU, PPUState PPU, TIMERState TIMER, APUState APU);
