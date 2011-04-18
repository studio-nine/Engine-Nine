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
using Microsoft.Xna.Framework.Input;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Interface for game camera
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Gets the camera view matrix
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// Gets the camera projection matrix
        /// </summary>
        Matrix Projection { get; }
    }

    /// <summary>
    /// Defines a camera used to view models.
    /// </summary>
    public class ModelViewerCamera : ICamera
    {
        public Input Input { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        public float Radius { get; set; }
        public float MinRadius { get; set; }
        public float MaxRadius { get; set; }
        public Vector3 Center { get; set; }
        public Vector3 Up { get; set; }
        public float Sensitivity { get; set; }

        private Vector3 start = Vector3.Zero;
        private Vector3 end = Vector3.Zero;
        private Matrix rotate = Matrix.Identity;
        private Matrix world = Matrix.Identity;
        private Matrix worldStart = Matrix.Identity;


        public Matrix View
        {
            get 
            {
                return Matrix.CreateTranslation(-Center) * world * 
                       Matrix.CreateLookAt(Vector3.UnitZ * Radius, Vector3.Zero, Up); 
            }
        }

        public Matrix Projection 
        {
            get 
            {
                return Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, MinRadius, MaxRadius * 10);
            }
        }


        public ModelViewerCamera(GraphicsDevice graphics) : this(graphics, 20, 1, 100, Vector3.Up) { }

        public ModelViewerCamera(GraphicsDevice graphics, float radius, float minRadius, float maxRadius, Vector3 up)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
            
            Radius = radius;
            MinRadius = minRadius;
            MaxRadius = maxRadius;
            Up = up;
            Sensitivity = 1;

            Input = new Input();

            Input.MouseDown += new EventHandler<MouseEventArgs>(Input_ButtonDown);
            Input.MouseMove += new EventHandler<MouseEventArgs>(Input_MouseMove);
            Input.MouseWheel += new EventHandler<MouseEventArgs>(Input_Wheel);
            Input.Update += new EventHandler<EventArgs>(Input_Update);
        }

        void Input_Update(object sender, EventArgs e)
        {
#if XBOX
            GamePadState state = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            if (state.Buttons.LeftShoulder == ButtonState.Pressed)
                Radius -= (MaxRadius - MinRadius) * 0.005f * Sensitivity;
            if (state.Buttons.RightShoulder == ButtonState.Pressed)
                Radius += (MaxRadius - MinRadius) * 0.005f * Sensitivity;

            if (Radius < MinRadius)
                Radius = MinRadius;
            else if (Radius > MaxRadius)
                Radius = MaxRadius;

            BeginRotation(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            EndRotation(GraphicsDevice.Viewport.Width / 2 + state.ThumbSticks.Right.X * 5 * Sensitivity,
                        GraphicsDevice.Viewport.Height / 2 - state.ThumbSticks.Right.Y * 5 * Sensitivity);
#endif
        }

        void Input_ButtonDown(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
            if (e.Button == MouseButtons.Left)
#else
            if (e.Button == MouseButtons.Right)
#endif
            {
                BeginRotation(e.X, e.Y);
            }
        }

        void Input_MouseMove(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
			if (e.IsLeftButtonDown)
#else
            if (e.IsRightButtonDown)
#endif
			{
                EndRotation(e.X, e.Y);
            }
        }


        void Input_Wheel(object sender, MouseEventArgs e)
        {   
            Radius -= e.WheelDelta * (MaxRadius - MinRadius) * 0.0001f * Sensitivity;

            if (Radius < MinRadius)
                Radius = MinRadius;
            else if (Radius > MaxRadius)
                Radius = MaxRadius;
        }
        
        private void BeginRotation(float x, float y)
        {
            start = ScreenToArcBall(x, y);
            worldStart = world;
        }

        private void EndRotation(float x, float y)
        {
            end = ScreenToArcBall(x, y);

            Vector3 v = Vector3.Cross(start, end);
            v.Normalize();

            float angle = (float)(Math.Acos(Vector3.Dot(start, end)));

            if (angle != 0 && v.LengthSquared() > 0)
            {
                rotate = Matrix.CreateFromAxisAngle(v, angle);

                world = Matrix.Multiply(worldStart, rotate);
            }
        }
		
		private Vector3 ScreenToArcBall(float x, float y)
		{
            Vector3 result = new Vector3();

			x = x * 2 / GraphicsDevice.Viewport.Width - 1;
            y = -y * 2 / GraphicsDevice.Viewport.Height + 1;
			
			float mag = x * x + y * y;
			
			if (mag > 1)
			{
				mag = (float)(1 / Math.Sqrt(mag));
				
				result.X = x * mag;
				result.Y = y * mag;
				result.Z = 0;
			}
			else
			{
				result.X = x;
				result.Y = y;
				result.Z = (float)(Math.Sqrt(1 - mag));
			}

            return result;
		}
    }
}
