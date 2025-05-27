using Silk.NET.SDL;

namespace TheAdventure;

public unsafe class InputLogic
{
    private Sdl _sdl;
    private bool _clickDetected = false;
    private int _clickX = 0, _clickY = 0;

    public InputLogic(Sdl sdl)
    {
        _sdl = sdl;
    }

    public bool ProcessInput()
    {
        Event ev = new Event();
        while (_sdl.PollEvent(ref ev) != 0)
        {
            if (ev.Type == (uint)EventType.Quit)
            {
                return true;
            }

            if (ev.Type == (uint)EventType.Mousebuttondown &&
                ev.Button.Button == (byte)MouseButton.Primary)
            {
                _clickDetected = true;
                _clickX = ev.Button.X;
                _clickY = ev.Button.Y;
            }

            if (ev.Type == (uint)EventType.Windowevent &&
                ev.Window.Event == (byte)WindowEventID.TakeFocus)
            {
                _sdl.SetWindowInputFocus(_sdl.GetWindowFromID(ev.Window.WindowID));
            }
        }

        return false;
    }

    public (bool up, bool down, bool left, bool right) GetMovementKeys()
    {
        ReadOnlySpan<byte> keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);

        bool w = keyboardState[(int)KeyCode.W] != 0;
        bool s = keyboardState[(int)KeyCode.S] != 0;
        bool a = keyboardState[(int)KeyCode.A] != 0;
        bool d = keyboardState[(int)KeyCode.D] != 0;

        return (w, s, a, d);
    }

    public (bool clicked, int x, int y) GetMouseClick()
    {
        if (_clickDetected)
        {
            _clickDetected = false;
            return (true, _clickX, _clickY);
        }

        return (false, 0, 0);
    }
}
