namespace Nine
{
    using System;
    using System.Windows.Forms;
    using Microsoft.Xna.Framework.Input;
    using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
    using FormMouseButtons = System.Windows.Forms.MouseButtons;
    using Keys = Microsoft.Xna.Framework.Input.Keys;
    using System.Runtime.InteropServices;
    
    class WindowsInputSource : IInputSource, IUpdateable
    {
        MouseState mouseState;
        KeyboardState keyboardState;
        
        bool mouseStateNeedsUpdate = true;
        bool keyboardStateNeedsUpdate = true;

        public MouseState MouseState
        {
            get
            {
                if (mouseStateNeedsUpdate)
                {
                    mouseStateNeedsUpdate = false;
                    mouseState = Mouse.GetState();
                    mouseState = new MouseState(mouseState.X, mouseState.Y, mouseState.ScrollWheelValue,
                                                leftDown ? ButtonState.Pressed : ButtonState.Released,
                                                middleDown ? ButtonState.Pressed : ButtonState.Released,
                                                rightDown ? ButtonState.Pressed : ButtonState.Released,
                                                mouseState.XButton1, mouseState.XButton2);
                }
                return mouseState;
            }
        }

        public KeyboardState KeyboardState
        {
            get
            {
                if (keyboardStateNeedsUpdate)
                {
                    keyboardStateNeedsUpdate = false;
                    keyboardState = Keyboard.GetState();
                }
                return keyboardState;
            }
        }

        public WindowsInputSource(IntPtr handle)
        {
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
        }

        void control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (MouseWheel != null)
                MouseWheel(this, new MouseEventArgs(MouseButtons.Middle, e.X, e.Y, e.Delta));
        }

        void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseEventArgs args = new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta);

            args.IsLeftButtonDown = leftDown;
            args.IsRightButtonDown = rightDown;
            args.IsMiddleButtonDown = middleDown;

            if (MouseMove != null)
                MouseMove(this, args);
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

            if (MouseDown != null)
                MouseDown(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
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

            if (MouseUp != null)
                MouseUp(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
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
            if (KeyDown != null)
                KeyDown(this, new KeyboardEventArgs(ConvertKeys(e.KeyCode)));
        }

        void control_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (KeyUp != null)
                KeyUp(this, new KeyboardEventArgs(ConvertKeys(e.KeyCode)));
        }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); 

        Keys ConvertKeys(System.Windows.Forms.Keys key)
        {
            // NOTE: this will not work when both Left & Right control keys are pressed.
            if (key == System.Windows.Forms.Keys.ControlKey)
            {
                if (GetAsyncKeyState(System.Windows.Forms.Keys.LControlKey) < 0)
                    return Keys.LeftControl;
                return Keys.RightControl;
            }

            if (key == System.Windows.Forms.Keys.ShiftKey)
            {
                if (GetAsyncKeyState(System.Windows.Forms.Keys.LShiftKey) < 0)
                    return Keys.LeftShift;
                return Keys.RightShift;
            }

            if (key == System.Windows.Forms.Keys.Menu)
            {
                if (GetAsyncKeyState(System.Windows.Forms.Keys.LMenu) < 0)
                    return Keys.LeftAlt;
                return Keys.RightAlt;
            }

            return (Keys)key;
        }
        
        MouseButtons ConvertButton(System.Windows.Forms.MouseButtons button)
        {
            if (button == System.Windows.Forms.MouseButtons.Right)
                return MouseButtons.Right;

            if (button == System.Windows.Forms.MouseButtons.Middle)
                return MouseButtons.Middle;

            return MouseButtons.Left;
        }

        public void Update(float elapsedTime)
        {
            mouseStateNeedsUpdate = true;
            keyboardStateNeedsUpdate = true;
        }

        private bool leftDown = false;
        private bool rightDown = false;
        private bool middleDown = false;
        private Control control = null;

        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseWheel;
        public event EventHandler<MouseEventArgs> MouseMove;
    }
}