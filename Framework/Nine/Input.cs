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
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
#if WINDOWS
using System.Windows.Forms;
using FormKeys = System.Windows.Forms.Keys;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using FormMouseButtons = System.Windows.Forms.MouseButtons;
using MouseButtons = Nine.MouseButtons;
using FormButtonState = System.Windows.Forms.ButtonState;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Nine
{
    #region KeyboardEventArgs
    /// <summary>
    /// Event args use for keyboard events.
    /// </summary>
    public class KeyboardEventArgs : EventArgs 
    {
        /// <summary>
        /// Gets the key been pressed or released.
        /// </summary>
        public Keys Key { get; internal set; }

        /// <summary>
        /// Gets whether shift is been pressed during event.
        /// </summary>
        public bool IsShiftPressed { get; internal set; }

        /// <summary>
        /// Gets whether ctrl is been pressed during event.
        /// </summary>
        public bool IsCtrlPressed { get; internal set; }

        /// <summary>
        /// Gets whether alt is been pressed during event.
        /// </summary>
        public bool IsAltPressed { get; internal set; }

        /// <summary>
        /// Creates a new instance of KeyboardEventArgs.
        /// </summary>
        public KeyboardEventArgs() { }

        /// <summary>
        /// Creates a new instance of KeyboardEventArgs.
        /// </summary>
        public KeyboardEventArgs(Keys key)
        {
            Key = key;

            KeyboardState keyboardState = Keyboard.GetState();

            IsAltPressed = keyboardState.IsKeyDown(Keys.LeftAlt) | keyboardState.IsKeyDown(Keys.RightAlt);
            IsCtrlPressed = keyboardState.IsKeyDown(Keys.LeftControl) | keyboardState.IsKeyDown(Keys.RightControl);
            IsShiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) | keyboardState.IsKeyDown(Keys.RightShift);
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
        Left,

        /// <summary>
        /// Defines the mouse right button.
        /// </summary>
        Right,

        /// <summary>
        /// Defines the mouse middle button.
        /// </summary>
        Middle,
    }

    /// <summary>
    /// Event args use for mouse events.
    /// </summary>
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
        /// Gets the delta amount of mouse wheel.
        /// </summary>
        public float WheelDelta { get; internal set; }

        /// <summary>
        /// Gets whether left mouse button is been pressed during event.
        /// </summary>
        public bool IsLeftButtonDown { get; internal set; }

        /// <summary>
        /// Gets whether right mouse button is been pressed during event.
        /// </summary>
        public bool IsRightButtonDown { get; internal set; }

        /// <summary>
        /// Gets whether middle mouse button is been pressed during event.
        /// </summary>
        public bool IsMiddleButtonDown { get; internal set; }

        /// <summary>
        /// Creates a new instance of MouseEventArgs.
        /// </summary>
        public MouseEventArgs() { }

        /// <summary>
        /// Creates a new instance of MouseEventArgs.
        /// </summary>
        public MouseEventArgs(MouseButtons button, int x, int y, float wheelDelta)
        {
            Button = button;
            X = x;
            Y = y;
            WheelDelta = wheelDelta;

            IsLeftButtonDown = Mouse.GetState().LeftButton == ButtonState.Pressed;
            IsRightButtonDown = Mouse.GetState().RightButton == ButtonState.Pressed;
            IsMiddleButtonDown = Mouse.GetState().MiddleButton == ButtonState.Pressed;
        }
    }
    #endregion

    #region Input
    /// <summary>
    /// A basic class that pushes input events to the consumer.
    /// </summary>
    /// <remarks>
    /// Before you create <c>Input</c> object, make sure a valid
    /// <c>InputComponent</c> object is created.
    /// 
    /// You can create as many <c>Input</c> instances any where
    /// without worrying much about performance and memoy leak.
    /// 
    /// However, <b>you need to explicitly keep an reference on Input instances</b>,
    /// otherwise the garbage collector may collect the instance and
    /// no events will be raised after that.
    /// </remarks>
    /// 
    /// <example>
    /// The following code will explode a potential bug
    /// when the application runs for certain period and the garbage
    /// collector starts to collect unused resource, by then, the
    /// input events will no longer be raised any more:
    /// <code>
    /// class EventListener
    /// {
    ///     public EventListener()
    ///     {
    ///         Input input = new Input();
    ///         input.MouseDown += new EventHandler(input_MouseDown);
    ///     }
    /// 
    ///     void input_MouseDown(object sender, MouseEventArgs e)
    ///     {
    ///         // This method won't be called after garbage collection.
    ///     }
    /// }
    /// </code>
    /// </example>
    /// 
    /// <example>
    /// To solve this problem, promote input from local variable to member by
    /// explicitly keeping a reference to it:
    /// <code>
    /// class EventListener
    /// {
    ///     Input input;
    ///     
    ///     public EventListener()
    ///     {
    ///         input = new Input();
    ///         input.MouseDown += new EventHandler(input_MouseDown);
    ///     }
    /// 
    ///     void input_MouseDown(object sender, MouseEventArgs e)
    ///     {
    ///         // This method works fine.
    ///     }
    /// }
    /// </code>
    /// </example>
    public class Input
    {
        /// <summary>
        /// Creates a new instance of Input.
        /// </summary>
        public Input() : this(InputComponent.Current) { }

        /// <summary>
        /// Creates a new instance of Input.
        /// </summary>
        public Input(InputComponent component)
        {
            if (component == null)
                throw new InvalidOperationException(
                    "InputComponent must not be null, do you forget to add an InputComponent to the game's component collection?");
            
            Component = component;
            Enabled = true;

            component.inputs.Add(new WeakReference(this));
        }

        /// <summary>
        /// Gets the InputComponent associated with this instance.
        /// </summary>
        public InputComponent Component { get; private set; }

        /// <summary>
        /// Gets or sets whether this instance will raise input events.
        /// </summary>
        public bool Enabled { get; set; }

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
        /// Occurs when the mouse scrolled.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseWheel;

        /// <summary>
        /// Occurs when the mouse moved.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseMove;

        #region Raise Events
        internal protected virtual void OnKeyUp(KeyboardEventArgs keyboardArgs)
        {
            if (KeyUp != null)
                KeyUp(this, keyboardArgs);
        }

        internal protected virtual void OnKeyDown(KeyboardEventArgs keyboardArgs)
        {
            if (KeyDown != null)
                KeyDown(this, keyboardArgs);
        }

        internal protected virtual void OnMouseMove(MouseEventArgs mouseArgs)
        {
            if (MouseMove != null)
                MouseMove(this, mouseArgs);
        }

        internal protected virtual void OnMouseUp(MouseEventArgs mouseArgs)
        {
            if (MouseUp != null)
                MouseUp(this, mouseArgs);
        }

        internal protected virtual void OnMouseDown(MouseEventArgs mouseArgs)
        {
            if (MouseDown != null)
                MouseDown(this, mouseArgs);
        }

        internal protected virtual void OnMouseWheel(MouseEventArgs mouseArgs)
        {
            if (MouseWheel != null)
                MouseWheel(this, mouseArgs);
        }
        #endregion
    }
    #endregion

    #region InputComponent
    /// <summary>
    /// An input component that manages a set of <c>Input</c> instances based on push model.
    /// </summary>
    public class InputComponent : GameComponent, IUpdateObject
    {
        #region Field
        internal List<WeakReference> inputs = new List<WeakReference>();

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
        /// Time interval for double click, measured in seconds
        /// </summary>
        private const float DoubleClickInterval = 0.25f;

        /// <summary>
        /// Gets the current MouseState.
        /// </summary>
        public MouseState MouseState { get { return mouseState; } }

        /// <summary>
        /// Gets the curren keyboard state.
        /// </summary>
        public KeyboardState KeyboardState { get { return keyboardState; } }

        /// <summary>
        /// Gets or sets the InputComponent for current context.
        /// </summary>
        public static InputComponent Current { get; set; }
        #endregion

        #region Method
        /// <summary>
        /// Creates a new instance of InputComponent.
        /// </summary>
        public InputComponent() : base((IServiceProvider)null) 
        {
            Current = this;
        }

        /// <summary>
        /// Creates a new instance of InputComponent using the input system of windows forms.
        /// </summary>
        /// <param name="handle">Handle of the game window</param>
        public InputComponent(IntPtr handle) : base((IServiceProvider)null)
        {
            Current = this;
#if WINDOWS
            control = Form.FromHandle(handle);
            control.MouseDown += new MouseEventHandler(control_MouseDown);
            control.MouseUp += new MouseEventHandler(control_MouseUp);
            control.MouseMove += new MouseEventHandler(control_MouseMove);
            control.KeyDown += new KeyEventHandler(control_KeyDown);
            control.KeyUp += new KeyEventHandler(control_KeyUp);
            control.MouseWheel += new MouseEventHandler(control_MouseWheel);
#endif
        }

        /// <summary>
        /// Gets whether the cursor is inside the specfied rectangle.
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

        /// <summary>
        /// All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
        /// With shift pressed this also results in this keys:
        /// </summary>
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
        /// Will catch all new states for keyboard, mouse and the gamepad.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
#if WINDOWS
            if (control != null)
                return;
#endif
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
            mouseArgs.IsLeftButtonDown = (mouseState.LeftButton == ButtonState.Pressed);
            mouseArgs.IsRightButtonDown = (mouseState.RightButton == ButtonState.Pressed);
            mouseArgs.IsMiddleButtonDown = (mouseState.MiddleButton == ButtonState.Pressed);

            // Mouse wheel event
            if (mouseWheelDelta != 0)
                Wheel(this, mouseArgs);

            // Mouse Left Button Events
            if (mouseState.LeftButton == ButtonState.Pressed &&
                mouseStateLastFrame.LeftButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButtons.Left;

                MouseDown(this, mouseArgs);
            }
            else if (mouseState.LeftButton == ButtonState.Released &&
                     mouseStateLastFrame.LeftButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButtons.Left;

                MouseUp(this, mouseArgs);
            }

            // Mouse Right Button Events
            if (mouseState.RightButton == ButtonState.Pressed &&
                mouseStateLastFrame.RightButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButtons.Right;

                MouseDown(this, mouseArgs); 
            }
            else if (mouseState.RightButton == ButtonState.Released &&
                     mouseStateLastFrame.RightButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButtons.Right;

                MouseUp(this, mouseArgs);
            }

            // Mouse Middle Button Events
            if (mouseState.MiddleButton == ButtonState.Pressed &&
                mouseStateLastFrame.MiddleButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButtons.Middle;

                MouseDown(this, mouseArgs);
            }
            else if (mouseState.MiddleButton == ButtonState.Released &&
                     mouseStateLastFrame.MiddleButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButtons.Middle;

                MouseUp(this, mouseArgs);
            }

            // Mouse move event
            if (mouseState.X != mouseStateLastFrame.X ||
                mouseState.Y != mouseStateLastFrame.Y)
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
            foreach (Keys key in keyboardState.GetPressedKeys())
            {
                if (!keysPressedLastFrame.Contains(key))
                {
                    keyboardArgs.Key = key;
                    KeyDown(this, keyboardArgs);
                }
            }

            // Key up events
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

            // Update component
            base.Update(gameTime);
        }

#if WINDOWS
        bool hasDown = false;

        Control control = null;

        void control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Wheel(this, new MouseEventArgs(MouseButtons.Middle, e.X, e.Y, e.Delta));
        }

        void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!hasDown)
                return;

            MouseMove(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
        }

        void control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hasDown = false;

            MouseUp(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
        }

        void control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hasDown = true;

            MouseDown(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
        }

        void control_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            KeyDown(this, new KeyboardEventArgs((Keys)e.KeyValue));
        }

        void control_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            KeyUp(this, new KeyboardEventArgs((Keys)e.KeyValue));
        }

        MouseButtons ConvertButton(System.Windows.Forms.MouseButtons button)
        {
            if (button == System.Windows.Forms.MouseButtons.Right)
                return MouseButtons.Right;

            if (button == System.Windows.Forms.MouseButtons.Middle)
                return MouseButtons.Middle;

            return MouseButtons.Left;
        }
#endif
        #endregion

        #region Raise Events
        private void KeyUp(InputComponent inputComponent, KeyboardEventArgs keyboardArgs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].IsAlive)
                {
                    Input input = (Input)inputs[i].Target;

                    if (input.Enabled)
                        input.OnKeyUp(keyboardArgs);
                }
                else
                {
                    inputs.RemoveAt(i);
                    i--;
                }
            }
        }

        private void KeyDown(InputComponent inputComponent, KeyboardEventArgs keyboardArgs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].IsAlive)
                {
                    Input input = (Input)inputs[i].Target;

                    if (input.Enabled)
                        input.OnKeyDown(keyboardArgs);
                }
                else
                {
                    inputs.RemoveAt(i);
                    i--;
                }
            }
        }

        private void MouseMove(InputComponent inputComponent, MouseEventArgs mouseArgs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].IsAlive)
                {
                    Input input = (Input)inputs[i].Target;

                    if (input.Enabled)
                        input.OnMouseMove(mouseArgs);
                }
                else
                {
                    inputs.RemoveAt(i);
                    i--;
                }
            }
        }

        private void MouseUp(InputComponent inputComponent, MouseEventArgs mouseArgs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].IsAlive)
                {
                    Input input = (Input)inputs[i].Target;

                    if (input.Enabled)
                        input.OnMouseUp(mouseArgs);
                }
                else
                {
                    inputs.RemoveAt(i);
                    i--;
                }
            }
        }

        private void MouseDown(InputComponent inputComponent, MouseEventArgs mouseArgs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].IsAlive)
                {
                    Input input = (Input)inputs[i].Target;

                    if (input.Enabled)
                        input.OnMouseDown(mouseArgs);
                }
                else
                {
                    inputs.RemoveAt(i);
                    i--;
                }
            }
        }

        private void Wheel(InputComponent inputComponent, MouseEventArgs mouseArgs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].IsAlive)
                {
                    Input input = (Input)inputs[i].Target;

                    if (input.Enabled)
                        input.OnMouseWheel(mouseArgs);
                }
                else
                {
                    inputs.RemoveAt(i);
                    i--;
                }
            }
        }
        #endregion
    }
    #endregion
}