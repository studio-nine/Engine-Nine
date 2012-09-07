namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
    using Keys = System.Windows.Input.Key;
#else
    using Keys = Microsoft.Xna.Framework.Input.Keys;
    using Microsoft.Xna.Framework.Input.Touch;
#endif

    public enum InputRaiseMode
    {
        Tunneling,
        Bubbling
    }

    /// <summary>
    /// An input component that manages a set of <c>Input</c> instances based on push model.
    /// </summary>
    public class InputComponent : IUpdateable
    {
        #region Fields
        /// <summary>
        /// Gets or sets the InputComponent for current context.
        /// </summary>
        public static InputComponent Current { get; set; }

        /// <summary>
        /// Gets or sets the raise mode.
        /// </summary>
        public InputRaiseMode RaiseMode { get; set; }

        internal List<WeakReference> inputs = new List<WeakReference>();

        internal IInputSource InputSource;

#if !SILVERLIGHT
        internal GamePadState[] gamePadStates = new GamePadState[4];
        GamePadState[] gamePadStatesLastFrame = new GamePadState[4];
        bool[] playerIndexEnabled = new bool[4] { true, true, true, true, };

        internal bool PlayerIndexChanged = false;

#if WINDOWS_PHONE
        internal bool EnabledGesturesChanged = false;
        GestureType enabledGestures = GestureType.None;
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
#endif
        #endregion

        #region Method
        /// <summary>
        /// Creates a new instance of InputComponent.
        /// </summary>
        public InputComponent()
        {
            Current = this;
            InputSource = new XnaInputSource();
            InitializeEvents();
        }

        /// <summary>
        /// Creates a new instance of InputComponent using the input system of windows forms.

        /// </summary>
        /// <param name="handle">Handle of the game window</param>
        public InputComponent(IntPtr handle)
        {
            Current = this;
#if WINDOWS
            InputSource = new WindowsInputSource(handle);
#else
            InputSource = new XnaInputSource();
#endif
            InitializeEvents();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputComponent"/> class.
        /// </summary>
        public InputComponent(IInputSource inputSource)
        {
            if (inputSource == null)
                throw new ArgumentNullException("inputSource");

            Current = this;
            this.InputSource = inputSource;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            InputSource.MouseDown += (sender, e) => { MouseDown(e); };
            InputSource.MouseUp += (sender, e) => { MouseUp(e); };
            InputSource.KeyDown += (sender, e) => { KeyDown(e); };
            InputSource.KeyUp += (sender, e) => { KeyUp(e); };
            InputSource.MouseWheel += (sender, e) => { Wheel(e); };
            InputSource.MouseMove += (sender, e) => { MouseMove(e); };
        }
        #endregion

        #region Update
        /// <summary>
        /// Will catch all new states for keyboard, mouse and the gamepad.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            if (InputSource is IUpdateable)
                ((IUpdateable)InputSource).Update(elapsedTime);

            Update();
            
#if XBOX
            UpdateGamePad();
#elif WINDOWS_PHONE
            UpdateTouchGestures();
#endif
        }

#if !SILVERLIGHT
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
                        for (int i = 0; i < 4; ++i)
                            playerIndexEnabled[i] = true;
                    return false;
                });
            }

            for (int i = 0; i < gamePadStates.Length; ++i)
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

        private void RaiseButtonEvent(Buttons button, int i)
        {
            if (gamePadStatesLastFrame[i].IsButtonUp(button) && gamePadStates[i].IsButtonDown(button))
                ButtonDown(new GamePadEventArgs() { Button = button, GamePadState = gamePadStates[i], PlayerIndex = (PlayerIndex)i });
            else if (gamePadStatesLastFrame[i].IsButtonDown(button) && gamePadStates[i].IsButtonUp(button))
                ButtonUp(new GamePadEventArgs() { Button = button, GamePadState = gamePadStates[i], PlayerIndex = (PlayerIndex)i });
        }
#endif

#if WINDOWS_PHONE
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

        internal void GestureSampled(GestureEventArgs gestureArgs)
        {
            ForEach(input =>
            {
                if (input.Enabled && (input.EnabledGestures & gestureArgs.GestureType) != 0)
                    input.OnGestureSampled(gestureArgs);
                return gestureArgs.Handled;
            });
        }
#endif
        #endregion

        #region Raise Events
        private void ForEach(Predicate<Input> action)
        {
            if (RaiseMode == InputRaiseMode.Tunneling)
            {
                for (int i = 0; i < inputs.Count; ++i)
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

#if !SILVERLIGHT
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
#endif

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
}