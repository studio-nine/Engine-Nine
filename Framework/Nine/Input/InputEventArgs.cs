namespace Nine
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
    
    #region KeyboardEventArgs
    /// <summary>
    /// Event args use for keyboard events.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class KeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the key been pressed or released.
        /// </summary>
        public Keys Key { get; internal set; }

        /// <summary>
        /// Gets the current keyboard state.
        /// </summary>
        public KeyboardState KeyboardState { get; internal set; }

        /// <summary>
        /// Gets or sets whether this event has been handled.
        /// Handled events will stop propagation to the next input container.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a new instance of KeyboardEventArgs.
        /// </summary>
        internal KeyboardEventArgs() { }

        /// <summary>
        /// Creates a new instance of KeyboardEventArgs.
        /// </summary>
        internal KeyboardEventArgs(Keys key)
        {
            Key = key;
            KeyboardState = Keyboard.GetState();
        }
    }
    #endregion

    #region MouseEventArgs
    /// <summary>
    /// Defines the three mouse buttons.
    /// </summary>
    public enum MouseButtons
    {
        /// <summary>
        /// Defines the mouse left button.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Defines the mouse right button.
        /// </summary>
        Right = 1 << 1,

        /// <summary>
        /// Defines the mouse middle button.
        /// </summary>
        Middle = 1 << 2,
    }

    /// <summary>
    /// EventArgs use for mouse events.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the mouse button been pressed or released.
        /// </summary>
        public MouseButtons Button { get; internal set; }

        /// <summary>
        /// Gets the X position of the mouse in game window client space.
        /// </summary>
        public int X { get; internal set; }

        /// <summary>
        /// Gets the Y position of the mouse in game window client space.
        /// </summary>
        public int Y { get; internal set; }

        /// <summary>
        /// Gets the position of the mouse in game window client space.
        /// </summary>
        public Point Position { get { return new Point(X, Y); } }

        /// <summary>
        /// Gets the delta amount of mouse wheel.
        /// </summary>
        public float WheelDelta { get; internal set; }

        /// <summary>
        /// Gets the current mouse state.
        /// </summary>
        public MouseState MouseState { get; internal set; }

        /// <summary>
        /// Gets or sets whether this event has been handled.
        /// Handled events will stop propagation to the next input container.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets whether the specified button is pressed.
        /// </summary>
        public bool IsButtonDown(MouseButtons button)
        {
            if (button == MouseButtons.Left)
                return IsLeftButtonDown;
            if (button == MouseButtons.Middle)
                return IsMiddleButtonDown;
            return IsRightButtonDown;
        }

        /// <summary>
        /// Gets whether the specified button is released.
        /// </summary>
        public bool IsButtonUp(MouseButtons button)
        {
            return !IsButtonDown(button);
        }

        internal bool IsLeftButtonDown;
        internal bool IsRightButtonDown;
        internal bool IsMiddleButtonDown;

        /// <summary>
        /// Creates a new instance of MouseEventArgs.
        /// </summary>
        internal MouseEventArgs() { }

        /// <summary>
        /// Creates a new instance of MouseEventArgs.
        /// </summary>
        internal MouseEventArgs(MouseButtons button, int x, int y, float wheelDelta)
        {
            Button = button;
            X = x;
            Y = y;
            WheelDelta = wheelDelta;

            MouseState = Mouse.GetState();

            IsLeftButtonDown = MouseState.LeftButton == ButtonState.Pressed;
            IsRightButtonDown = MouseState.RightButton == ButtonState.Pressed;
            IsMiddleButtonDown = MouseState.MiddleButton == ButtonState.Pressed;
        }
    }
    #endregion
    
#if !SILVERLIGHT
    #region GamePadEventArgs
    /// <summary>
    /// EventArgs use for gamepad events.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GamePadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the gamepad button been pressed or released.
        /// </summary>
        public Buttons Button { get; internal set; }

        /// <summary>
        /// Gets which player has triggered the event.
        /// </summary>
        public PlayerIndex PlayerIndex { get; internal set; }

        /// <summary>
        /// Gets the current gamepad state.
        /// </summary>
        public GamePadState GamePadState { get; internal set; }

        /// <summary>
        /// Gets or sets whether this event has been handled.
        /// Handled events will stop propagation to the next input container.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a new instance of GamePadEventArgs.
        /// </summary>
        internal GamePadEventArgs() { }
    }
    #endregion

    #region GestureEventArgs
    /// <summary>
    /// EventArgs use for touch gesture events.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GestureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the gesture type.
        /// </summary>
        public GestureType GestureType { get; internal set; }

        /// <summary>
        /// Gets the detailed gesture sample.
        /// </summary>
        public GestureSample GestureSample { get; internal set; }

        /// <summary>
        /// Gets or sets whether this event has been handled.
        /// Handled events will stop propagation to the next input container.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a new instance of GamePadEventArgs.
        /// </summary>
        internal GestureEventArgs() { }
    }
    #endregion
#endif
}