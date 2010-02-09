#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion


namespace Isles
{
    public interface IInput
    {
        event EventHandler<KeyboardEventArgs> KeyDown;
        event EventHandler<KeyboardEventArgs> KeyUp;
        event EventHandler<MouseEventArgs> MouseDown;
        event EventHandler<MouseEventArgs> MouseUp;
        event EventHandler<MouseEventArgs> DoubleClick;
        event EventHandler<MouseEventArgs> Wheel;
        event EventHandler<MouseEventArgs> MouseMove;
    }

    public class KeyboardEventArgs : EventArgs 
    {
        public Keys Key { get; internal set; }

        public bool IsShiftPressed { get; internal set; }
        public bool IsCtrlPressed { get; internal set; }
        public bool IsAltPressed { get; internal set; }
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle,
    }

    public class MouseEventArgs : EventArgs
    {
        public MouseButton Button { get; internal set; }

        public int X { get; internal set; }
        public int Y { get; internal set; }

        public float WheelDelta { get; internal set; }
        public float WheelValue { get; internal set; }

        public bool IsLeftButtonDown { get; internal set; }
        public bool IsRightButtonDown { get; internal set; }
        public bool IsMiddleButtonDown { get; internal set; }
    }

    public class Input : GameComponent, IInput
    {
        #region Field
        /// <summary>
        /// Mouse state, set every frame in the Update method.
        /// </summary>
        MouseState mouseState = Mouse.GetState(), mouseStateLastFrame;

        /// <summary>
        /// Keyboard state, set every frame in the Update method.
        /// Note: KeyboardState is a class and not a struct,
        /// we have to initialize it here, else we might run into trouble when
        /// accessing any keyboardState data before BaseGame.Update() is called.
        /// We can also NOT use the last state because everytime we call
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
        /// <summary>
        /// Flag for simulating double click
        /// </summary>
        int doubleClickFlag = 0;
        double doubleClickTime = 0;
        
        /// <summary>
        /// Time interval for double click, measured in seconds
        /// </summary>
        private const float DoubleClickInterval = 0.25f;


        public MouseState MouseState { get { return mouseState; } }
        public KeyboardState KeyboardState { get { return keyboardState; } }
        #endregion


        #region Events
        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> DoubleClick;
        public event EventHandler<MouseEventArgs> Wheel;
        public event EventHandler<MouseEventArgs> MouseMove;
        #endregion


        #region Method
        /// <summary>
        /// Input class is designed to support a large number of instances.
        /// You can create as many instances as you wish, and add each
        /// instance to the game components collection. Don't call the
        /// update method manually.
        /// </summary>
        public Input(Game game) : base(game) 
        {
            // TODO: Optimize this
        }

        /// <summary>
        /// Mouse in box
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>Bool</returns>
        public bool MouseInBox(Rectangle rect)
        {
            return mouseState.X >= rect.X &&
                   mouseState.Y >= rect.Y &&
                   mouseState.X < rect.Right &&
                   mouseState.Y < rect.Bottom;
        }


        public static bool IsSpecialKey(Keys key)
        {
            // All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
            // With shift pressed this also results in this keys:
            // ~_|{}:"<>? !@#$%^&*().
            int keyNum = (int)key;
            if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) ||
                (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
                key == Keys.Space || // well, space ^^
                key == Keys.OemTilde || // `~
                key == Keys.OemMinus || // -_
                key == Keys.OemPipe || // \|
                key == Keys.OemOpenBrackets || // [{
                key == Keys.OemCloseBrackets || // ]}
                key == Keys.OemQuotes || // '"
                key == Keys.OemQuestion || // /?
                key == Keys.OemPlus ||
                key == Keys.OemSemicolon ||
                key == Keys.OemComma ||
                key == Keys.OemPeriod) // =+
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Key to char helper conversion method.
        /// Note: If the keys are mapped other than on a default QWERTY
        /// keyboard, this method will not work properly. Most keyboards
        /// will return the same for A-Z and 0-9, but the special keys
        /// might be different.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Char</returns>
        public static char KeyToChar(Keys key, bool shiftPressed)
        {
            // If key will not be found, just return space
            char ret = ' ';
            int keyNum = (int)key;
            if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
            {
                if (shiftPressed)
                    ret = key.ToString()[0];
                else
                    ret = key.ToString().ToLower()[0];
            }
            else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 &&
                shiftPressed == false)
            {
                ret = (char)((int)'0' + (keyNum - Keys.D0));
            }
            else if (key == Keys.D1 && shiftPressed)
                ret = '!';
            else if (key == Keys.D2 && shiftPressed)
                ret = '@';
            else if (key == Keys.D3 && shiftPressed)
                ret = '#';
            else if (key == Keys.D4 && shiftPressed)
                ret = '$';
            else if (key == Keys.D5 && shiftPressed)
                ret = '%';
            else if (key == Keys.D6 && shiftPressed)
                ret = '^';
            else if (key == Keys.D7 && shiftPressed)
                ret = '&';
            else if (key == Keys.D8 && shiftPressed)
                ret = '*';
            else if (key == Keys.D9 && shiftPressed)
                ret = '(';
            else if (key == Keys.D0 && shiftPressed)
                ret = ')';
            else if (key == Keys.OemTilde)
                ret = shiftPressed ? '~' : '`';
            else if (key == Keys.OemMinus)
                ret = shiftPressed ? '_' : '-';
            else if (key == Keys.OemPipe)
                ret = shiftPressed ? '|' : '\\';
            else if (key == Keys.OemOpenBrackets)
                ret = shiftPressed ? '{' : '[';
            else if (key == Keys.OemCloseBrackets)
                ret = shiftPressed ? '}' : ']';
            else if (key == Keys.OemSemicolon)
                ret = shiftPressed ? ':' : ';';
            else if (key == Keys.OemQuotes)
                ret = shiftPressed ? '"' : '\'';
            else if (key == Keys.OemComma)
                ret = shiftPressed ? '<' : ',';
            else if (key == Keys.OemPeriod)
                ret = shiftPressed ? '>' : '.';
            else if (key == Keys.OemQuestion)
                ret = shiftPressed ? '?' : '/';
            else if (key == Keys.OemPlus)
                ret = shiftPressed ? '+' : '=';

            // Return result
            return ret;
        }

        /// <summary>
        /// Handle keyboard input helper method to catch keyboard input
        /// for an input text. Only used to enter the player name in the game.
        /// </summary>
        /// <param name="inputText">Input text</param>
        public void CatchKeyboardInput(ref string inputText, int maxChars)
        {
            // Is a shift key pressed (we have to check both, left and right)
            bool isShiftPressed =
                keyboardState.IsKeyDown(Keys.LeftShift) ||
                keyboardState.IsKeyDown(Keys.RightShift);

            // Go through all pressed keys
            foreach (Keys pressedKey in keyboardState.GetPressedKeys())
                // Only process if it was not pressed last frame
                if (keysPressedLastFrame.Contains(pressedKey) == false)
                {
                    // No special key?
                    if (IsSpecialKey(pressedKey) == false &&
                        // Max. allow 32 chars
                        inputText.Length < maxChars)
                    {
                        // Then add the letter to our inputText.
                        // Check also the shift state!
                        inputText += KeyToChar(pressedKey, isShiftPressed);
                    }
                    else if (pressedKey == Keys.Back &&
                        inputText.Length > 0)
                    {
                        // Remove 1 character at end
                        inputText = inputText.Substring(0, inputText.Length - 1);
                    }
                }
        }
        #endregion


        #region Update
        /// <summary>
        /// Update, called from BaseGame.Update().
        /// Will catch all new states for keyboard, mouse and the gamepad.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Make sure we are called by xna component system
            if (Assembly.GetCallingAssembly() != typeof(Game).Assembly)
                throw new InvalidOperationException("Do not call Input.Update directly. Use Game.Components.Add instead.");

            if (!Game.IsActive)
                return;

            // Handle mouse input variables
            mouseStateLastFrame = mouseState;
            mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            mouseWheelDelta = mouseState.ScrollWheelValue - mouseWheelValue;
            mouseWheelValue = mouseState.ScrollWheelValue;

            // Initialzie mouse event args
            MouseEventArgs mouseArgs = new MouseEventArgs();
            mouseArgs.X = mouseState.X;
            mouseArgs.Y = mouseState.Y;
            mouseArgs.WheelDelta = mouseWheelDelta;
            mouseArgs.WheelValue = mouseWheelValue;
            mouseArgs.IsLeftButtonDown = (mouseState.LeftButton == ButtonState.Pressed);
            mouseArgs.IsRightButtonDown = (mouseState.RightButton == ButtonState.Pressed);
            mouseArgs.IsMiddleButtonDown = (mouseState.MiddleButton == ButtonState.Pressed);

            // Mouse wheel event
            if (mouseWheelDelta != 0 && Wheel != null)
                Wheel(this, mouseArgs);

            // Mouse Left Button Events
            if (mouseState.LeftButton == ButtonState.Pressed &&
                mouseStateLastFrame.LeftButton == ButtonState.Released)
            {
                if (doubleClickFlag == 0)
                {
                    doubleClickFlag = 1;
                    doubleClickTime = gameTime.TotalGameTime.TotalSeconds;

                    mouseArgs.Button = MouseButton.Left;

                    if (MouseDown != null)
                        MouseDown(this, mouseArgs);
                }
                else if (doubleClickFlag == 1)
                {
                    if ((gameTime.TotalGameTime.TotalSeconds - doubleClickTime) < DoubleClickInterval)
                    {
                        doubleClickFlag = 0;
                        
                        if (DoubleClick != null)
                            DoubleClick(this, mouseArgs);
                    }
                    else
                    {
                        doubleClickTime = gameTime.TotalGameTime.TotalSeconds;

                        mouseArgs.Button = MouseButton.Left;

                        if (MouseDown != null)
                            MouseDown(this, mouseArgs);
                    }
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released &&
                     mouseStateLastFrame.LeftButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButton.Left;

                if (MouseUp != null)
                    MouseUp(this, mouseArgs);
            }

            // Mouse Right Button Events
            if (mouseState.RightButton == ButtonState.Pressed &&
                mouseStateLastFrame.RightButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButton.Right;

                if (MouseDown != null)
                    MouseDown(this, mouseArgs); 
            }
            else if (mouseState.RightButton == ButtonState.Released &&
                     mouseStateLastFrame.RightButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButton.Right;

                if (MouseUp != null)
                    MouseUp(this, mouseArgs);
            }

            // Mouse Middle Button Events
            if (mouseState.MiddleButton == ButtonState.Pressed &&
                mouseStateLastFrame.MiddleButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButton.Middle;

                if (MouseDown != null)
                    MouseDown(this, mouseArgs);
            }
            else if (mouseState.MiddleButton == ButtonState.Released &&
                     mouseStateLastFrame.MiddleButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButton.Middle;

                if (MouseUp != null)
                    MouseUp(this, mouseArgs);
            }

            // Mouse move event
            if (MouseMove != null && (
                mouseState.X != mouseStateLastFrame.X ||
                mouseState.Y != mouseStateLastFrame.Y))
                MouseMove(this, mouseArgs);


            // Handle keyboard input
            keysPressedLastFrame = new List<Keys>(keyboardState.GetPressedKeys());
            keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            KeyboardEventArgs keyboardArgs = new KeyboardEventArgs();

            keyboardArgs.IsAltPressed = keyboardState.IsKeyDown(Keys.LeftAlt) |
                                        keyboardState.IsKeyDown(Keys.RightAlt);
            keyboardArgs.IsCtrlPressed = keyboardState.IsKeyDown(Keys.LeftControl) |
                                        keyboardState.IsKeyDown(Keys.RightControl);
            keyboardArgs.IsShiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) |
                                        keyboardState.IsKeyDown(Keys.RightShift);

            // Key down events
            if (KeyDown != null)
            {
                foreach (Keys key in keyboardState.GetPressedKeys())
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
                    foreach (Keys keyCurrent in keyboardState.GetPressedKeys())
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


            // Update component
            base.Update(gameTime);
        }
        #endregion
    }
}