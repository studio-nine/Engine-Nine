namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Input.Touch;
    using Keys = Microsoft.Xna.Framework.Input.Keys;


    /// <summary>
    /// A basic class that pushes input events to the consumer.
    /// </summary>
    /// <remarks>
    /// Before you create <c>Input</c> object, make sure a valid
    /// <c>InputComponent</c> object is created.
    /// 
    /// You can create as many <c>Input</c> instances any where
    /// without worrying much about performance and memory leak.
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
        /// Gets the InputComponent associated with this instance.
        /// </summary>
        internal InputComponent Component;

        /// <summary>
        /// Creates a new instance of Input.
        /// </summary>
        public Input() : this(InputComponent.Current) { }

        /// <summary>
        /// Creates a new instance of Input.
        /// </summary>
        public Input(InputComponent component)
        {
            if (Nine.Serialization.ContentProperties.IsContentBuild)
                return;

            if (component == null)
                throw new InvalidOperationException(
                    "InputComponent must not be null, do you forget to add an InputComponent to the game's component collection?");
            
            Component = component;
            Enabled = true;

            component.inputs.Add(new WeakReference(this));
        }

        /// <summary>
        /// Gets or sets whether this instance will raise input events.
        /// </summary>
        public bool Enabled 
        {
            get { return enabled; }
            set 
            {
                enabled = value; 
                Component.PlayerIndexChanged = true;
#if WINDOWS_PHONE
                if (EnabledGestures != GestureType.None)
                    Component.EnabledGesturesChanged = true; 
#endif
            }
        }

        private bool enabled;

#if WINDOWS_PHONE
        /// <summary>
        /// Gets or sets enabled gestures handles by this input.
        /// </summary>
        /// <remarks>
        /// You can choose to handle gestures either using the method provided by <c>Input</c>
        /// or manually detect it using <c>TouchPanel</c>. But these two methods don't work
        /// together with each other. By setting this <c>Input.EnabledGestures</c> property,
        /// you indicate that you are going to handle gestures using the event model provided
        /// by Engine Nine across your whole application, in which case you may fail to read
        /// gestures using <c>TouchPanel.ReadGesture</c> method.
        /// </remarks>
        public GestureType EnabledGestures
        {
            get { return enabledGestures; }
            set { enabledGestures = value; Component.EnabledGesturesChanged = true; }
        }

        private GestureType enabledGestures = GestureType.None;
#endif
        
        /// <summary>
        /// Gets or sets the player index to which this <c>Input</c> class will respond to.
        /// A value of null represents this instance will respond to all player inputs.
        /// </summary>
        public PlayerIndex? PlayerIndex
        {
            get { return playerIndex; }
            set { playerIndex = value; Component.PlayerIndexChanged = true; }
        }

        private PlayerIndex? playerIndex;

        /// <summary>
        /// Gets the current gamePad state.
        /// </summary>
        public GamePadState GamePadState { get { return PlayerIndex != null ? Component.gamePadStates[(int)PlayerIndex] : new GamePadState(); } }

        /// <summary>
        /// Gets the current keyboard state.
        /// </summary>
        public KeyboardState KeyboardState { get { return Component.InputSource.KeyboardState; } }

        /// <summary>
        /// Gets the current mouse state.
        /// </summary>
        public MouseState MouseState { get { return Component.InputSource.MouseState; } }

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
        /// Occurs when the mouse scrolled.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseWheel;

        /// <summary>
        /// Occurs when the mouse moved.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseMove;

        /// <summary>
        /// Occurs when the game update itself.
        /// </summary>
        public event EventHandler<EventArgs> Update;
        
        /// <summary>
        /// Occurs when a gamepad used by the current <c>PlayerIndex</c> has just been pressed.
        /// </summary>
        public event EventHandler<GamePadEventArgs> ButtonDown;

        /// <summary>
        /// Occurs when a gamepad used by the current <c>PlayerIndex</c> has just been released.
        /// </summary>
        public event EventHandler<GamePadEventArgs> ButtonUp;

        /// <summary>
        /// Occurs when a new gesture has been sampled.
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureSampled;

        internal bool HasGestureSampled { get { return GestureSampled != null; } }

        internal protected virtual void OnButtonDown(GamePadEventArgs e)
        {
            if (ButtonDown != null)
                ButtonDown(this, e);
        }

        internal protected virtual void OnButtonUp(GamePadEventArgs e)
        {
            if (ButtonUp != null)
                ButtonUp(this, e);
        }

        internal protected virtual void OnGestureSampled(GestureEventArgs e)
        {
            if (GestureSampled != null)
                GestureSampled(this, e);
        }

        internal protected virtual void OnKeyUp(KeyboardEventArgs e)
        {
            if (KeyUp != null)
                KeyUp(this, e);
        }

        internal protected virtual void OnKeyDown(KeyboardEventArgs e)
        {
            if (KeyDown != null)
                KeyDown(this, e);
        }

        internal protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null)
                MouseMove(this, e);
        }

        internal protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null)
                MouseUp(this, e);
        }

        internal protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null)
                MouseDown(this, e);
        }

        internal protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            if (MouseWheel != null)
                MouseWheel(this, e);
        }

        internal protected virtual void OnUpdate()
        {
            if (Update != null)
                Update(this, EventArgs.Empty);
        }
        #endregion

        #region Extras
        /// <summary>
        /// All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
        /// With shift pressed this also results in this keys:
        /// </summary>
        internal static bool IsSpecialKey(Keys key)
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
        internal static char KeyToChar(Keys key, bool shiftPressed)
        {
            // TODO: Use another way to get input to allow multi-language keyboards.

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

        List<Keys> keysPressedLastFrame;

        /// <summary>
        /// Handle keyboard input helper method to catch keyboard input
        /// for an input text. Only used to enter the player name in the game.
        /// </summary>
        internal void CatchKeyboardInput(ref string inputText, int maxChars)
        {
            if (!Enabled)
                return;

            if (keysPressedLastFrame == null)
                keysPressedLastFrame = new List<Keys>();            

            // Is a shift key pressed (we have to check both, left and right)
            bool isShiftPressed =
                KeyboardState.IsKeyDown(Keys.LeftShift) ||
                KeyboardState.IsKeyDown(Keys.RightShift);

            // Go through all pressed keys
            var pressedKeys = KeyboardState.GetPressedKeys();
            foreach (Keys pressedKey in pressedKeys)
            {
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

            keysPressedLastFrame.Clear();
            keysPressedLastFrame.AddRange(pressedKeys);
        }

        // TODO: Make this more respond changes better
        internal void EditString(ref string input, Keys key, bool multiline, int selectedIndex, int maxChars)
        {
            if (selectedIndex > input.Length)
                throw new ArgumentOutOfRangeException("selectedIndex");

            if (input.Length >= maxChars)
                return;

            bool isShiftPressed =
                KeyboardState.IsKeyDown(Keys.LeftShift) ||
                KeyboardState.IsKeyDown(Keys.RightShift);

            if (IsSpecialKey(key) == false && input.Length < maxChars)
            {
                input = input.Insert(selectedIndex, KeyToChar(key, isShiftPressed).ToString());
            }
            else if (multiline && key == Keys.Enter)
            {
                input = input.Insert(selectedIndex, Environment.NewLine);
            }
            else if (key == Keys.Back && input.Length > 0)
            {
                input = input.Remove(selectedIndex - 1);
            }
        }

        #endregion
    }
}