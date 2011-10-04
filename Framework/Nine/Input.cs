#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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
using System.Runtime.InteropServices;
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
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Nine
{
    #region Input
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
    public class Input : IInputSource
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
                if (EnabledGestures != GestureType.None)
                    Component.EnabledGesturesChanged = true; 
            }
        }

        private bool enabled;

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

        /// <summary>
        /// Gets the current keyboard state.
        /// </summary>
        public KeyboardState KeyboardState { get { return Component.keyboardState; } }

        /// <summary>
        /// Gets the current mouse state.
        /// </summary>
        public MouseState MouseState { get { return Component.mouseState; } }

        /// <summary>
        /// Gets the current gamePad state.
        /// </summary>
        public GamePadState GamePadState { get { return PlayerIndex != null ? Component.gamePadStates[(int)PlayerIndex] : new GamePadState(); } }

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

        /// <summary>
        /// Occurs when the game update itself.
        /// </summary>
        public event EventHandler<EventArgs> Update;

        internal bool HasGestureSampled { get { return GestureSampled != null; } }

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
        internal void CatchKeyboardInput(ref string inputText, int maxChars)
        {
            if (!Enabled)
                return;

            // Is a shift key pressed (we have to check both, left and right)
            bool isShiftPressed =
                KeyboardState.IsKeyDown(Keys.LeftShift) ||
                KeyboardState.IsKeyDown(Keys.RightShift);

            // Go through all pressed keys
            foreach (Keys pressedKey in KeyboardState.GetPressedKeys())
                // Only process if it was not pressed last frame
                if (Component.keysPressedLastFrame.Contains(pressedKey) == false)
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
    }
    #endregion

    #region InputRaiseStrategy

    public enum InputRaiseStrategy
    {
        Tunneling,
        Bubbling
    }

    #endregion

    #region InputComponent
    /// <summary>
    /// An input component that manages a set of <c>Input</c> instances based on push model.
    /// </summary>
    public class InputComponent : IUpdateable
    {
        #region Field

        internal List<WeakReference> inputs = new List<WeakReference>();

        /// <summary>
        /// Mouse state, set every frame in the Update method.
        /// </summary>
        internal MouseState mouseState = Mouse.GetState(), mouseStateLastFrame;

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
        internal KeyboardState keyboardState = Keyboard.GetState();

        /// <summary>
        /// Keys pressed last frame, for comparison if a key was just pressed.
        /// </summary>
        internal List<Keys> keysPressedLastFrame = new List<Keys>();
        
        internal GamePadState[] gamePadStates = new GamePadState[4];
        internal GamePadState[] gamePadStatesLastFrame = new GamePadState[4];
        private bool[] playerIndexEnabled = new bool[4] { true, true, true, true, };

        /// <summary>
        /// Mouse wheel delta this frame. XNA does report only the total
        /// scroll value, but we usually need the current delta!
        /// </summary>
        /// <returns>0</returns>
        int mouseWheelDelta = 0;
        int mouseWheelValue = 0;

        internal bool PlayerIndexChanged = false;
        internal bool EnabledGesturesChanged = false;
        private GestureType enabledGestures = GestureType.None;

        /// <summary>
        /// Gets or sets the InputComponent for current context.
        /// </summary>
        public static InputComponent Current { get; set; }
        
        /// <summary>
        /// Time interval for double click, measured in seconds
        /// </summary>
        private const float DoubleClickInterval = 0.25f;

        public InputRaiseStrategy RaiseStrategy { get; set; }

#if WINDOWS
        private bool leftDown = false;
        private bool rightDown = false;
        private bool middleDown = false;
        private Control control = null;
        internal bool HostingEnabled = false;        
#endif

        private static readonly Buttons[] AllGamePadButtons = new Buttons[] 
        {
#if WINDOWS_PHONE
            Buttons.Back,
#else
            Buttons.A, Buttons.B, Buttons.Back, Buttons.BigButton, Buttons.DPadDown,
            Buttons.DPadLeft, Buttons.DPadRight, Buttons.DPadUp, Buttons.LeftShoulder,
            Buttons.LeftStick, Buttons.LeftThumbstickDown, Buttons.LeftThumbstickLeft,
            Buttons.LeftThumbstickRight, Buttons.LeftThumbstickUp, Buttons.LeftTrigger,
            Buttons.RightShoulder, Buttons.RightStick, Buttons.RightThumbstickDown,
            Buttons.RightThumbstickLeft, Buttons.RightThumbstickRight, Buttons.RightThumbstickUp,
            Buttons.RightTrigger, Buttons.Start, Buttons.X, Buttons.Y,
#endif
        };
        #endregion

        #region Method
        /// <summary>
        /// Creates a new instance of InputComponent.
        /// </summary>
        public InputComponent()
        {
            Current = this;
            RaiseStrategy = InputRaiseStrategy.Tunneling;
        }

        /// <summary>
        /// Creates a new instance of InputComponent using the input system of windows forms.
        /// </summary>
        /// <param name="handle">Handle of the game window</param>
        public InputComponent(IntPtr handle)
        {
            Current = this;
            RaiseStrategy = InputRaiseStrategy.Tunneling;
#if WINDOWS
            Mouse.WindowHandle = handle;
            control = Form.FromHandle(handle);
            control.PreviewKeyDown += new PreviewKeyDownEventHandler(control_PreviewKeyDown);
            control.MouseDown += new MouseEventHandler(control_MouseDown);
            control.MouseUp += new MouseEventHandler(control_MouseUp);
            control.MouseCaptureChanged += new EventHandler(control_MouseCaptureChanged);
            control.MouseMove += new MouseEventHandler(control_MouseMove);
            control.KeyDown += new KeyEventHandler(control_KeyDown);
            control.KeyUp += new KeyEventHandler(control_KeyUp);
            control.MouseWheel += new MouseEventHandler(control_MouseWheel);
#endif
        }
        #endregion

        #region Update
        /// <summary>
        /// Will catch all new states for keyboard, mouse and the gamepad.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            Update();
#if WINDOWS
            if (control != null || HostingEnabled)
                return;
#endif      
            UpdateMouse();
            UpdateKeyboard();
#if XBOX
            UpdateGamePad();
#endif

#if WINDOWS_PHONE
            UpdateTouchGestures();
#endif
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
            if (mouseWheelDelta != 0)
                Wheel(mouseArgs);

            // Mouse Left Button Events
            if (mouseState.LeftButton == ButtonState.Pressed &&
                mouseStateLastFrame.LeftButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButtons.Left;

                MouseDown(mouseArgs);
            }
            else if (mouseState.LeftButton == ButtonState.Released &&
                     mouseStateLastFrame.LeftButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButtons.Left;

                MouseUp(mouseArgs);
            }

            // Mouse Right Button Events
            if (mouseState.RightButton == ButtonState.Pressed &&
                mouseStateLastFrame.RightButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButtons.Right;

                MouseDown(mouseArgs);
            }
            else if (mouseState.RightButton == ButtonState.Released &&
                     mouseStateLastFrame.RightButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButtons.Right;

                MouseUp(mouseArgs);
            }

            // Mouse Middle Button Events
            if (mouseState.MiddleButton == ButtonState.Pressed &&
                mouseStateLastFrame.MiddleButton == ButtonState.Released)
            {
                mouseArgs.Button = MouseButtons.Middle;

                MouseDown(mouseArgs);
            }
            else if (mouseState.MiddleButton == ButtonState.Released &&
                     mouseStateLastFrame.MiddleButton == ButtonState.Pressed)
            {
                mouseArgs.Button = MouseButtons.Middle;

                MouseUp(mouseArgs);
            }

            // Mouse move event
            if (mouseState.X != mouseStateLastFrame.X ||
                mouseState.Y != mouseStateLastFrame.Y)
                MouseMove(mouseArgs);
        }
        
        private void UpdateKeyboard()
        {
            // Handle keyboard input
            keysPressedLastFrame.Clear();
            keysPressedLastFrame.AddRange(keyboardState.GetPressedKeys());
            keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            KeyboardEventArgs keyboardArgs = new KeyboardEventArgs();

            keyboardArgs.KeyboardState = keyboardState;

            // Key down events
            foreach (Keys key in keyboardState.GetPressedKeys())
            {
                if (!keysPressedLastFrame.Contains(key))
                {
                    keyboardArgs.Key = key;
                    KeyDown(keyboardArgs);
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
                    KeyUp(keyboardArgs);
                }
            }
        }

        private void UpdateGamePad()
        {
            if (PlayerIndexChanged)
            {
                PlayerIndexChanged = false;
                Array.Clear(playerIndexEnabled, 0, 4);
                ForEach(input => 
                {
                    if (input.PlayerIndex.HasValue)
                        playerIndexEnabled[(int)input.PlayerIndex.Value] = true;
                    else
                        for (int i = 0; i < 4; i++)
                            playerIndexEnabled[i] = true;
                    return false;
                });
            }

            for (int i = 0; i < gamePadStates.Length; i++)
            {
                if (playerIndexEnabled[i])
                {
                    gamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                    if (gamePadStates[i].IsConnected)
                    {
                        foreach (Buttons button in AllGamePadButtons)
                        {
                            RaiseButtonEvent(button, i);
                        }
                    }
                    gamePadStatesLastFrame[i] = gamePadStates[i];
                }
            }
        }
        
        private void UpdateTouchGestures()
        {
            if (EnabledGesturesChanged)
            {
                EnabledGesturesChanged = false;
                enabledGestures = GestureType.None;
                ForEach(input => 
                {
                    if (input.Enabled && input.HasGestureSampled)
                        enabledGestures |= input.EnabledGestures; 
                    return false; 
                });
                TouchPanel.EnabledGestures = enabledGestures;
            }

            if (enabledGestures != GestureType.None && TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                GestureSampled(new GestureEventArgs() { GestureSample = gesture, GestureType = gesture.GestureType });
            }
        }

        private void RaiseButtonEvent(Buttons button, int i)
        {
            if (gamePadStatesLastFrame[i].IsButtonUp(button) && gamePadStates[i].IsButtonDown(button))
                ButtonDown(new GamePadEventArgs() { Button = button, GamePadState = gamePadStates[i], PlayerIndex = (PlayerIndex)i });
            else if (gamePadStatesLastFrame[i].IsButtonDown(button) && gamePadStates[i].IsButtonUp(button))
                ButtonUp(new GamePadEventArgs() { Button = button, GamePadState = gamePadStates[i], PlayerIndex = (PlayerIndex)i });
        }

#if WINDOWS
        void control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Wheel(new MouseEventArgs(MouseButtons.Middle, e.X, e.Y, e.Delta));
        }

        void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseEventArgs args = new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta);

            args.IsLeftButtonDown = leftDown;
            args.IsRightButtonDown = rightDown;
            args.IsMiddleButtonDown = middleDown;

            MouseMove(args);
        }

        void control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            control.Capture = true;

            if (e.Button == FormMouseButtons.Left)
                leftDown = true;
            else if (e.Button == FormMouseButtons.Right)
                rightDown = true;
            else if (e.Button == FormMouseButtons.Middle)
                middleDown = true;

            MouseDown(new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
        }

        void control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            control.Capture = false;

            if (e.Button == FormMouseButtons.Left)
                leftDown = false;
            else if (e.Button == FormMouseButtons.Right)
                rightDown = false;
            else if (e.Button == FormMouseButtons.Middle)
                middleDown = false;

            MouseUp(new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
        }

        void control_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (!control.Capture)
            {
                leftDown = false;
                rightDown = false;
                middleDown = false;
            }
        }
        
        void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        void control_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // FIXME: This is not correct. Avoid using it!!!
            KeyDown(new KeyboardEventArgs((Keys)e.KeyValue));
        }

        void control_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // FIXME: This is not correct. Avoid using it!!!
            KeyUp(new KeyboardEventArgs((Keys)e.KeyValue));
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
        private void ForEach(Predicate<Input> action)
        {
            if (RaiseStrategy == InputRaiseStrategy.Tunneling)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    var input = inputs[i].Target as Input;
                    if (input != null)
                    {
                        if (action(input))
                            break;
                    }
                    else
                    {
                        inputs.RemoveAt(i);
                        i--;
                    }
                }
            }
            else
            {
                for (int i = inputs.Count - 1; i >= 0; i--)
                {
                    var input = inputs[i].Target as Input;
                    if (input != null)
                    {
                        if (action(input))
                            break;
                    }
                    else
                    {
                        inputs.RemoveAt(i);
                        i++;
                    }
                }
            }
        }

        internal void KeyUp(KeyboardEventArgs keyboardArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled)
                    input.OnKeyUp(keyboardArgs);
                return keyboardArgs.Handled;
            });
        }

        internal void KeyDown(KeyboardEventArgs keyboardArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled)
                    input.OnKeyDown(keyboardArgs);
                return keyboardArgs.Handled;
            });
        }

        internal void ButtonUp(GamePadEventArgs gamePadArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled && (input.PlayerIndex == null || input.PlayerIndex.Value == gamePadArgs.PlayerIndex))
                    input.OnButtonUp(gamePadArgs);
                return gamePadArgs.Handled;
            });
        }

        internal void ButtonDown(GamePadEventArgs gamePadArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled && (input.PlayerIndex == null || input.PlayerIndex.Value == gamePadArgs.PlayerIndex))
                    input.OnButtonDown(gamePadArgs);
                return gamePadArgs.Handled;
            });
        }

        internal void MouseMove(MouseEventArgs mouseArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled)
                    input.OnMouseMove(mouseArgs);
                return mouseArgs.Handled;
            });
        }

        internal void MouseUp(MouseEventArgs mouseArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled)
                    input.OnMouseUp(mouseArgs);
                return mouseArgs.Handled;
            });
        }

        internal void MouseDown(MouseEventArgs mouseArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled)
                    input.OnMouseDown(mouseArgs);
                return mouseArgs.Handled;
            });
        }

        internal void Wheel(MouseEventArgs mouseArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled)
                    input.OnMouseWheel(mouseArgs);
                return mouseArgs.Handled;
            });
        }

        internal void GestureSampled(GestureEventArgs gestureArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled && (input.EnabledGestures & gestureArgs.GestureType) != 0)
                    input.OnGestureSampled(gestureArgs);
                return gestureArgs.Handled;
            });
        }

        private void Update()
        {
            ForEach(input =>
            {
                if (input.Enabled)
                    input.OnUpdate();
                return false;
            });
        }
        #endregion
    }
    #endregion

}