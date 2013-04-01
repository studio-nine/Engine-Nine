namespace Nine.Graphics.UI.Input
{
    using System;

    public interface IInputElement
    {
        bool IsEnabled { get; }
        bool Focusable { get; set; }

        bool Focus();

        event EventHandler<KeyboardEventArgs> KeyDown;
        event EventHandler<KeyboardEventArgs> KeyUp;
        event EventHandler<MouseEventArgs> MouseEnter;
        event EventHandler<MouseEventArgs> MouseLeave;
        event EventHandler<MouseEventArgs> MouseMove;
        event EventHandler<MouseEventArgs> MouseWheel;
    }
}
