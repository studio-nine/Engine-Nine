namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Input;
    using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
    
    class XnaInputSource : IInputSource, IUpdateable
    {
        /// <summary>
        /// Mouse state, set every frame in the Update method.
        /// </summary>
        MouseState mouseState = Mouse.GetState(), mouseStateLastFrame;

        /// <summary>
        /// Keyboard state, set every frame in the Update method.
        /// Note: KeyboardState is a class and not a struct,
        /// we have to initialize it here, else we might run into trouble when
        /// accessing any keyboardState data before BaseGame.Update() is called.
        /// We can also NOT use the last state because every time we call
        /// Keyboard.GetState() the old state is useless (see XNA help for more
        /// information, section Input). We store our own array of keys from
        /// the last frame for comparing stuff.
        /// </summary>
        KeyboardState keyboardState = Keyboard.GetState();

        /// <summary>
        /// Keys pressed last frame, for comparison if a key was just pressed.
        /// </summary>
        List<Keys> keysPressedLastFrame = new List<Keys>();

        /// <summary>
        /// Mouse wheel delta this frame. XNA does report only the total
        /// scroll value, but we usually need the current delta!
        /// </summary>
        /// <returns>0</returns>
        int mouseWheelDelta = 0;
        int mouseWheelValue = 0;

        public MouseState MouseState { get { return mouseState; } }
        public KeyboardState KeyboardState { get { return keyboardState; } }

        public void Update(TimeSpan elapsedTime)
        {
            UpdateMouse();
            UpdateKeyboard();
        }
        
        private void UpdateMouse()
        {
            // Handle mouse input variables
            mouseStateLastFrame = mouseState;
            mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            mouseWheelDelta = mouseState.ScrollWheelValue - mouseWheelValue;
            mouseWheelValue = mouseState.ScrollWheelValue;

            // Initialzie mouse event args
            MouseEventArgs mouseArgs = new MouseEventArgs();
            mouseArgs.MouseState = mouseState;
            mouseArgs.X = mouseState.X;
            mouseArgs.Y = mouseState.Y;
            mouseArgs.WheelDelta = mouseWheelDelta;
            mouseArgs.IsLeftButtonDown = (mouseState.LeftButton == ButtonState.Pressed);
            mouseArgs.IsRightButtonDown = (mouseState.RightButton == ButtonState.Pressed);
            mouseArgs.IsMiddleButtonDown = (mouseState.MiddleButton == ButtonState.Pressed);

            // Mouse wheel event
            if (mouseWheelDelta != 0 && MouseWheel != null)
                MouseWheel(this, mouseArgs);

            // Mouse Left Button Events
            if (mouseState.LeftButton == ButtonState.Pressed &&
                mouseStateLastFrame.LeftButton == ButtonState.Released && MouseDown != null)
            {
                mouseArgs.Button = MouseButtons.Left;                
                MouseDown(this, mouseArgs);
            }
            else if (mouseState.LeftButton == ButtonState.Released &&
                     mouseStateLastFrame.LeftButton == ButtonState.Pressed && MouseUp != null)
            {
                mouseArgs.Button = MouseButtons.Left;
                MouseUp(this, mouseArgs);
            }

            // Mouse Right Button Events
            if (mouseState.RightButton == ButtonState.Pressed &&
                mouseStateLastFrame.RightButton == ButtonState.Released && MouseDown != null)
            {
                mouseArgs.Button = MouseButtons.Right;
                MouseDown(this, mouseArgs);
            }
            else if (mouseState.RightButton == ButtonState.Released &&
                     mouseStateLastFrame.RightButton == ButtonState.Pressed && MouseUp != null)
            {
                mouseArgs.Button = MouseButtons.Right;
                MouseUp(this, mouseArgs);
            }

            // Mouse Middle Button Events
            if (mouseState.MiddleButton == ButtonState.Pressed &&
                mouseStateLastFrame.MiddleButton == ButtonState.Released && MouseDown != null)
            {
                mouseArgs.Button = MouseButtons.Middle;
                MouseDown(this, mouseArgs);
            }
            else if (mouseState.MiddleButton == ButtonState.Released &&
                     mouseStateLastFrame.MiddleButton == ButtonState.Pressed && MouseUp != null)
            {
                mouseArgs.Button = MouseButtons.Middle;
                MouseUp(this, mouseArgs);
            }

            // Mouse move event
            if ((mouseState.X != mouseStateLastFrame.X ||
                 mouseState.Y != mouseStateLastFrame.Y) && MouseMove != null)
            {
                MouseMove(this, mouseArgs);
            }
        }

        private void UpdateKeyboard()
        {
            // Handle keyboard input
            keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            var pressedKeys = keyboardState.GetPressedKeys();

            KeyboardEventArgs keyboardArgs = new KeyboardEventArgs();

            keyboardArgs.KeyboardState = keyboardState;

            // Key down events
            if (KeyDown != null)
            {
                foreach (Keys key in pressedKeys)
                {
                    if (!keysPressedLastFrame.Contains(key))
                    {
                        keyboardArgs.Key = key;
                        KeyDown(this, keyboardArgs);
                    }
                }
            }

            // Key up events
            if (KeyUp != null)
            {
                foreach (Keys key in keysPressedLastFrame)
                {
                    bool found = false;
                    foreach (Keys keyCurrent in pressedKeys)
                        if (keyCurrent == key)
                        {
                            found = true;
                            break;
                        }

                    if (!found)
                    {
                        keyboardArgs.Key = key;
                        KeyUp(this, keyboardArgs);
                    }
                }
            }

            keysPressedLastFrame.Clear();
            keysPressedLastFrame.AddRange(pressedKeys);
        }

        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseWheel;
        public event EventHandler<MouseEventArgs> MouseMove;
    }
}