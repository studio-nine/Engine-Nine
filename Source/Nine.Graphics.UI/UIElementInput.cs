namespace Nine.Graphics.UI
{
    using Microsoft.Xna.Framework;
    using System;

    public abstract partial class UIElement
    {
        #region Events

        /// <summary>
        /// Occurs when a key is been pressed.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyDown;

        /// <summary>
        /// Occurs when a key is been released.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyUp;

        /// <summary>
        /// Occurs when a mouse button is been pressed.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseDown;

        /// <summary>
        /// Occurs when a mouse button is been released.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseUp;

        /// <summary>
        /// Occurs when the mouse moved.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseMove;

        /// <summary>
        /// Occurs when the mouse scrolled.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseWheel;

        /// <summary>
        /// Occurs when a gamepad used by the current <c>PlayerIndex</c> has just been pressed.
        /// </summary>
        public event EventHandler<GamePadEventArgs> ButtonDown;

        /// <summary>
        /// Occurs when a gamepad used by the current <c>PlayerIndex</c> has just been released.
        /// </summary>
        public event EventHandler<GamePadEventArgs> ButtonUp;

        #endregion

        #region Invoke Input Methods

        internal void InvokeKeyDown(object sender, KeyboardEventArgs e)
        {
            OnKeyDown(e);
            if (KeyDown != null)
                KeyDown(sender, e);
        }
        internal void InvokeKeyUp(object sender, KeyboardEventArgs e)
        {
            OnKeyUp(e);
            if (KeyUp != null)
                KeyUp(sender, e);
        }
        internal void InvokeMouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
            if (MouseMove != null)
                MouseMove(sender, e);
        }
        internal void InvokeOnMouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
            if (MouseUp != null)
                MouseUp(sender, e);
        }
        internal void InvokeMouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
            if (MouseDown != null)
                MouseDown(sender, e);
        }
        internal void InvokeMouseWheel(object sender, MouseEventArgs e)
        {
            OnMouseWheel(e);
            if (MouseWheel != null)
                MouseWheel(sender, e);
        }
        internal void InvokeButtonDown(object sender, GamePadEventArgs e)
        {
            OnButtonDown(e);
            if (ButtonDown != null)
                ButtonDown(sender, e);
        }
        internal void InvokeButtonUp(object sender, GamePadEventArgs e)
        {
            OnButtonUp(e);
            if (ButtonUp != null)
                ButtonUp(sender, e);
        }

        #endregion

        protected virtual void OnKeyDown(KeyboardEventArgs e) { }
        protected virtual void OnKeyUp(KeyboardEventArgs e) { }
        protected virtual void OnMouseMove(MouseEventArgs e) { }
        protected virtual void OnMouseUp(MouseEventArgs e) { }
        protected virtual void OnMouseDown(MouseEventArgs e) { }
        protected virtual void OnMouseWheel(MouseEventArgs e) { }
        protected virtual void OnButtonDown(GamePadEventArgs e) { }
        protected virtual void OnButtonUp(GamePadEventArgs e) { }

        public bool HitTest(Vector2 point)
        {
            return AbsoluteRenderTransform.Contains(point.X, point.Y) == ContainmentType.Contains;
        }
    }
}
