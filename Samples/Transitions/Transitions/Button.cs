#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Nine;

#endregion

namespace Transitions
{
    /// <summary>
    /// The is a simple Button class that contains fields and events that a typical
    /// Gui system should provide.
    /// </summary>
    public class Button
    {
        bool insideLastFrame = false;

        public float X { get; set; }
        public float Y { get; set; }
        public string Text { get; set; }
        public Color Color { get; set; }
        public Rectangle Bounds { get; set; }
        public bool Enabled { get; set; }
        public object Tag { get; set; }

        public event EventHandler<MouseEventArgs> MouseOver;
        public event EventHandler<MouseEventArgs> MouseOut;
        public event EventHandler<MouseEventArgs> Click;


        public Button(Input input)
        {
            Enabled = true;
            Color = Color.White;

            input.MouseDown += new EventHandler<MouseEventArgs>(input_MouseDown);
            input.MouseMove += new EventHandler<MouseEventArgs>(input_MouseMove);
        }

        void input_MouseDown(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                Rectangle rectange = Bounds;

                rectange.Offset((int)X, (int)Y);

                if (rectange.Contains(e.X, e.Y) && Click != null)
                {
                    Click(this, e);
                }
            }
        }

        void input_MouseMove(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                Rectangle rectange = Bounds;

                rectange.Offset((int)X, (int)Y);

                bool inside = rectange.Contains(e.X, e.Y);

                if (inside && !insideLastFrame && MouseOver != null)
                {
                    MouseOver(this, e);
                }

                if (insideLastFrame && !inside && MouseOut != null)
                {
                    MouseOut(this, e);
                }

                insideLastFrame = inside;
            }
        }
    }
}
