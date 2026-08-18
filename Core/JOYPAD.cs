using GameBoyEmulator.SaveState.Components;
using System.Windows.Input;

namespace GameBoyEmulator.Core;

public class JOYPAD
{
    private byte _pad = 0x0F;
    private byte _buttons = 0x0F;
    private byte _joyp = 0x3F;
    
    public byte JOYP { get => _joyp; set => _joyp = (byte)((value & 0xF0) | (_joyp & 0x0F)); }

    public void Update(MMU mmu)
    {
        if ((_joyp & 0x30) == 0x30)
        {
            _joyp = 0x3F;
        }
        else if ((_joyp & 0x20) == 0)
        {
            _joyp = (byte)((_joyp & 0xF0) | _buttons);
            if (_buttons != 0x0F)
            {
                mmu.IF |= 0x10;
            }
        }
        else if ((_joyp & 0x10) == 0)
        {
            _joyp = (byte)((_joyp & 0xF0) | _pad);
            if (_pad != 0x0F)
            {
                mmu.IF |= 0x10;
            }
        }
    }

    public void HandleKeyDown(Key key)
    {
        if (key == Settings.DPadUp)
        {
            _pad = (byte)(_pad & ~0x4);
        }
        else if (key == Settings.DPadLeft)
        {
            _pad = (byte)(_pad & ~0x2);
        }
        else if (key == Settings.DPadRight)
        {
            _pad = (byte)(_pad & ~0x8);
        }
        else if (key == Settings.DPadDown)
        {
            _pad = (byte)(_pad & ~0x1);
        }
        else if (key == Settings.ButtonA)
        {
            _buttons = (byte)(_buttons & ~0x1);
        }
        else if (key == Settings.ButtonB)
        {
            _buttons = (byte)(_buttons & ~0x2);
        }
        else if (key == Settings.ButtonStart)
        {
            _buttons = (byte)(_buttons & ~0x8);
        }
        else if (key == Settings.ButtonSelect)
        {
            _buttons = (byte)(_buttons & ~0x4);
        }
        else if (key == Settings.AllButtons)
        {
            _buttons = 0;
        }
    }

    public void HandleKeyUp(Key key)
    {
        if (key == Settings.DPadUp)
        {
            _pad |= 0x4;
        }
        else if (key == Settings.DPadLeft)
        {
            _pad |= 0x2;
        }
        else if (key == Settings.DPadRight)
        {
            _pad |= 0x8;
        }
        else if (key == Settings.DPadDown)
        {
            _pad |= 0x1;
        }
        else if (key == Settings.ButtonA)
        {
            _buttons |= 0x1;
        }
        else if (key == Settings.ButtonB)
        {
            _buttons |= 0x2;
        }
        else if (key == Settings.ButtonStart)
        {
            _buttons |= 0x8;
        }
        else if (key == Settings.ButtonSelect)
        {
            _buttons |= 0x4;
        }
        else if (key == Settings.AllButtons)
        {
            _buttons = 0xF;
        }
    }

    public JOYPADState SaveState()
    {
        return new JOYPADState(JOYP);
    }

    public void LoadState(JOYPADState state)
    {
        JOYP = state.JOYP;
    }
}
