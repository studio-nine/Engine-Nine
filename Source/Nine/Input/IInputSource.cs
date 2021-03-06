namespace Nine
{
    using System;
    using Microsoft.Xna.Framework.Input;
    
    /// <summary>
    /// Defines the source that triggers the input.
    /// </summary>
    public interface IInputSource
    {
        /// <summary>
        /// Gets the state of the mouse.
        /// </summary>
        MouseState MouseState { get; }

        /// <summary>
        /// Gets the state of the keyboard.
        /// </summary>
        KeyboardState KeyboardState { get; }

        /// <summary>
        /// Occurs when a key is been pressed.
        /// </summary>
        event EventHandler<KeyboardEventArgs> KeyDown;

        /// <summary>
        /// Occurs when a key is been released.
        /// </summary>
        event EventHandler<KeyboardEventArgs> KeyUp;

        /// <summary>
        /// Occurs when a mouse button is been pressed.
        /// </summary>
        event EventHandler<MouseEventArgs> MouseDown;

        /// <summary>
        /// Occurs when a mouse button is been released.
        /// </summary>
        event EventHandler<MouseEventArgs> MouseUp;

        /// <summary>
        /// Occurs when the mouse scrolled.
        /// </summary>
        event EventHandler<MouseEventArgs> MouseWheel;

        /// <summary>
        /// Occurs when the mouse moved.
        /// </summary>
        event EventHandler<MouseEventArgs> MouseMove;
    }
}