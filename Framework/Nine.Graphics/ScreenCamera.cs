#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines the coordinate system used by ScreenCamera.
    /// </summary>
    public enum ScreenCameraCoordinate
    {
        /// <summary>
        /// Represents a right handed coordinate system with X axis pointing right,
        /// Y axis pointing up and Z axis pointing outside the screen.
        /// The origin of the coordinate system is the center of the screen.
        /// </summary>
        ThreeDimension,

        /// <summary>
        /// Represents a left handed coordinate system with X axis pointing right,
        /// Y axis pointing down and Z axis pointing outside the screen.
        /// The origin of the coordinate system is the top left corner of the screen.
        /// </summary>
        TwoDimension,
    }

    /// <summary>
    /// Defines a 2D orthographic screen camera.
    /// </summary>
    public class ScreenCamera : ICamera
    {
        public Input Input { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Viewport? Viewport { get; set; }

        public ScreenCameraCoordinate CoordinateType { get; set; }

        public float Zoom { get; set; }
        public float MinZoom { get; set; }
        public float MaxZoom { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float Sensitivity { get; set; }

        private Vector2 startMouse = Vector2.Zero;
        private Vector2 startPosition = Vector2.Zero;

        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        public Matrix View
        {
            get { return Matrix.CreateTranslation(-PositionOffset.X - X, -PositionOffset.Y - Y, -Z); }
        }

        public Matrix Projection
        {
            get
            {
                float scale = 0.5f / Zoom;
                if (CoordinateType == ScreenCameraCoordinate.TwoDimension)
                {
                    return Matrix.CreateOrthographicOffCenter(
                        -GraphicsDevice.Viewport.Width * scale, GraphicsDevice.Viewport.Width * scale,
                        GraphicsDevice.Viewport.Height * scale, -GraphicsDevice.Viewport.Height * scale, NearClip, FarClip);
                }
                else
                {
                    return Matrix.CreateOrthographicOffCenter(
                        -GraphicsDevice.Viewport.Width * scale, GraphicsDevice.Viewport.Width * scale,
                        -GraphicsDevice.Viewport.Height * scale, GraphicsDevice.Viewport.Height * scale, NearClip, FarClip);
                }
            }
        }

        private Vector2 PositionOffset
        {
            get
            {
                return CoordinateType == ScreenCameraCoordinate.TwoDimension ?
                    new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2) : Vector2.Zero;
            }
        }
        
        public ScreenCamera(GraphicsDevice graphics) : this(graphics, ScreenCameraCoordinate.ThreeDimension)
        {
        }

        public ScreenCamera(GraphicsDevice graphics, ScreenCameraCoordinate coordinateType)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
            CoordinateType = coordinateType;

            Z = 1000;
            Zoom = 1;
            MinZoom = 0.01f;
            MaxZoom = 10f;
            Sensitivity = 1;
            NearClip = 0;
            FarClip = 10000;

            Input = new Input();

            Input.MouseDown += new EventHandler<MouseEventArgs>(Input_ButtonDown);
            Input.MouseMove += new EventHandler<MouseEventArgs>(Input_MouseMove);
            Input.MouseWheel += new EventHandler<MouseEventArgs>(Input_Wheel);
        }

        private void Input_ButtonDown(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
            if (e.Button == MouseButtons.Left)
#else
            if (e.Button == MouseButtons.Right)
#endif
            {
                startMouse.X = e.X;
                startMouse.Y = e.Y;
                startPosition.X = X;
                startPosition.Y = Y;
            }
        }

        private void Input_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.IsButtonDown(MouseButtons.Right))
            {
                X = startPosition.X - (e.X - startMouse.X) / Zoom;
                if (CoordinateType == ScreenCameraCoordinate.TwoDimension)
                    Y = startPosition.Y - (e.Y - startMouse.Y) / Zoom;
                else
                    Y = startPosition.Y + (e.Y - startMouse.Y) / Zoom;
            }
        }


        private void Input_Wheel(object sender, MouseEventArgs e)
        {
            float zoom = NormalizeZoom(this.Zoom);
            float maxZoom = NormalizeZoom(this.MaxZoom);
            float minZoom = NormalizeZoom(this.MinZoom);

            zoom += e.WheelDelta * (maxZoom - minZoom) * 0.0001f * Sensitivity;
            zoom = MathHelper.Clamp(zoom, minZoom, maxZoom);
            this.Zoom = DenormalizeZoom(zoom);
        }

        private float NormalizeZoom(float zoom)
        {
            System.Diagnostics.Debug.Assert(zoom > 0);

            if (zoom >= 1)
                return zoom - 1;
            return 1 - 1.0f / zoom;
        }

        private float DenormalizeZoom(float zoom)
        {
            if (zoom >= 0)
                return zoom = 1 + zoom;
            return zoom = 1.0f / (1 - zoom);
        }
    }
}
