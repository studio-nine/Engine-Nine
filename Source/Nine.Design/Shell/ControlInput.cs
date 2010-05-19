#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Nine.Design.Shell
{
    public class ControlInput : IInput
    {
        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseMove;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> Wheel;

        public Control Control { get; private set; }

        public ControlInput(Control control)
        {
            Control = control;

            control.MouseDown += new MouseEventHandler(control_MouseDown);
            control.MouseUp += new MouseEventHandler(control_MouseUp);
            control.MouseMove += new MouseEventHandler(control_MouseMove);
            control.MouseWheel += new MouseEventHandler(control_MouseWheel);            
        }

        void control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Wheel != null)
            {
                Wheel(this, new MouseEventArgs(MouseButtons.Middle, e.X, e.Y, e.Delta));
            }
        }

        void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!hasDown)
                return;

            if (MouseMove != null)
            {
                MouseMove(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
            }
        }

        void control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hasDown = false;

            if (MouseUp != null)
            {
                MouseUp(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
            }
        }

        void control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hasDown = true;

            if (MouseDown != null)
            {
                MouseDown(this, new MouseEventArgs(ConvertButton(e.Button), e.X, e.Y, e.Delta));
            }
        }

        MouseButtons ConvertButton(System.Windows.Forms.MouseButtons button)
        {
            if (button == System.Windows.Forms.MouseButtons.Right)
                return MouseButtons.Right;

            if (button == System.Windows.Forms.MouseButtons.Middle)
                return MouseButtons.Middle;

            return MouseButtons.Left;
        }

        bool hasDown = false;
    }
}
