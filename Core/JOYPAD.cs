using System.Windows.Input;

namespace GameBoyEmulator.Core;

public class JOYPAD
{
    private byte _pad = 0x0F;
    private byte _buttons = 0x0F;
    private byte _oldPad = 0x0F;
    private byte _oldButtons = 0x0F;
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

        if (_oldButtons != _buttons) { Console.WriteLine($"{_joyp:X2}"); }
        if (_oldPad != _pad) { Console.WriteLine($"{_joyp:X2}"); }
        _oldPad = _pad;
        _oldButtons = _buttons;
    }

    public void HandleKeyDown(Key key)
    {
        switch (key)
        {
            case Key.W: // UP
                _pad = (byte)(_pad & ~0x4);
                break;
            case Key.A: // LEFT
                _pad = (byte)(_pad & ~0x2);
                break;
            case Key.S: // DOWN
                _pad = (byte)(_pad & ~0x8);
                break;
            case Key.D: // RIGHT
                _pad = (byte)(_pad & ~0x1);
                break;
            case Key.U: // A
                _buttons = (byte)(_buttons & ~0x1);
                break;
            case Key.I: // B
                _buttons = (byte)(_buttons & ~0x2);
                break;
            case Key.O: // START
                _buttons = (byte)(_buttons & ~0x8);
                break;
            case Key.L: // SELECT
                _buttons = (byte)(_buttons & ~0x4);
                break;
        }
    }

    public void HandleKeyUp(Key key)
    {
        switch (key)
        {
            case Key.W: // UP
                _pad |= 0x4;
                break;
            case Key.A: // LEFT
                _pad |= 0x2;
                break;
            case Key.S: // DOWN
                _pad |= 0x8;
                break;
            case Key.D: // RIGHT
                _pad |= 0x1;
                break;
            case Key.U: // A
                _buttons |= 0x1;
                break;
            case Key.I: // B
                _buttons |= 0x2;
                break;
            case Key.O: // START
                _buttons |= 0x8;
                break;
            case Key.L: // SELECT
                _buttons |= 0x4;
                break;
        }
    }
}
