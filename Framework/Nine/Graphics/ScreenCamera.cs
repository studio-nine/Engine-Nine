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
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines a 2D orthographic screen camera.
    /// </summary>
    public class ScreenCamera : ICamera
    {
        public Input Input { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        public float Zoom { get; set; }
        public float MinZoom { get; set; }
        public float MaxZoom { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float Sensitivity { get; set; }
        
        private Point startPoint = Point.Zero;
        
        public Matrix View
        {
            get { return Matrix.CreateTranslation(-X, -Y, -Z); }
        }

        public Matrix Projection 
        {
            get
            {
                return Matrix.CreateOrthographic(
                    Zoom * GraphicsDevice.Viewport.Width, Zoom * GraphicsDevice.Viewport.Height, NearClip, FarClip);
            }
        }


        public ScreenCamera(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
            
            Zoom = 1;
            MinZoom = 0.01f;
            MaxZoom = 10f;
            Sensitivity = 1;
            NearClip = 0;
            FarClip = 1000;

            Input = new Input();

            Input.MouseDown += new EventHandler<MouseEventArgs>(Input_ButtonDown);
            Input.MouseMove += new EventHandler<MouseEventArgs>(Input_MouseMove);
            Input.MouseWheel += new EventHandler<MouseEventArgs>(Input_Wheel);
        }

        void Input_ButtonDown(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
            if (e.Button == MouseButtons.Left)
#else
            if (e.Button == MouseButtons.Right)
#endif
            {
                startPoint.X = e.X;
                startPoint.Y = e.Y;
            }
        }

        void Input_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.IsRightButtonDown)
            {
                X -= e.X - startPoint.X;
                Y += e.Y - startPoint.Y;

                startPoint.X = e.X;
                startPoint.Y = e.Y;
            }
        }


        void Input_Wheel(object sender, MouseEventArgs e)
        {   
            Zoom -= e.WheelDelta * (MaxZoom - MinZoom) * 0.0001f * Sensitivity;

            if (Zoom < MinZoom)
                Zoom = MinZoom;
            else if (Zoom > MaxZoom)
                Zoom = MaxZoom;
        }
    }
}
