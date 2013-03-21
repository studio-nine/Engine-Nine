#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
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
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif
#endregion

namespace Nine
{
    /// <summary>
    /// Input source for silverlight on Windows Phone.
    /// </summary>
    public class SilverlightInputSource : IInputSource
    {
        System.Windows.UIElement control;

        /// <summary>
        /// Gets the state of the mouse.
        /// </summary>
        public MouseState MouseState
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the state of the keyboard.
        /// </summary>
        public KeyboardState KeyboardState
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SilverlightInputSource"/> class.
        /// </summary>
        /// <param name="control">The control.</param>
        public SilverlightInputSource(System.Windows.UIElement control)
        {
            this.control = control;
            control.KeyDown += new System.Windows.Input.KeyEventHandler(control_KeyDown);
            control.KeyUp += new System.Windows.Input.KeyEventHandler(control_KeyUp);
            control.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(control_MouseLeftButtonDown);
            control.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(control_MouseLeftButtonUp);
            control.MouseMove += new System.Windows.Input.MouseEventHandler(control_MouseMove);
        }

        private MouseEventArgs GetMouseEventArgs(System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(control);
            return new MouseEventArgs(MouseButtons.Left, (int)position.X, (int)position.Y, 0.0f)
            {
                IsLeftButtonDown = true,
                IsRightButtonDown = false,
                IsMiddleButtonDown = false,
            };
        }

        void control_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MouseMove != null)
                MouseMove(this, GetMouseEventArgs(e));
        }

        void control_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MouseDown != null)
                MouseDown(this, GetMouseEventArgs(e));
        }

        void control_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MouseDown != null)
                MouseDown(this, GetMouseEventArgs(e));
        }

        void control_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // FIXME: This is not correct. Avoid using it!!!
            if (KeyDown != null)
                KeyDown(this, new KeyboardEventArgs((Keys)e.Key));
        }

        void control_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // FIXME: This is not correct. Avoid using it!!!
            if (KeyUp != null)
                KeyUp(this, new KeyboardEventArgs((Keys)e.Key));
        }

        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseWheel;
        public event EventHandler<MouseEventArgs> MouseMove;
    }
}