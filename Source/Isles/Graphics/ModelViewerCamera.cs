#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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

namespace Isles.Graphics
{
    public class ModelViewerCamera : ICamera
    {
        public Input Input { get; private set; }
        public Game Game { get; private set; }

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
            // TODO: Optimize this
            //       Update view and projection matrix only when parameter changed
            get { return Matrix.Multiply(world, Matrix.CreateLookAt(new Vector3(0, 0, Radius), Center, Up)); }
        }

        public Matrix Projection 
        {
            get 
            {
                return Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, Game.GraphicsDevice.Viewport.AspectRatio, MinRadius, MaxRadius * 2);
            }
        }


        public ModelViewerCamera(Game game) : this(game, 20, 1, 100, Vector3.Up) { }

        public ModelViewerCamera(Game game, float radius, float minRadius, float maxRadius, Vector3 up)
        {
            Game = game;


            Radius = radius;
            MinRadius = minRadius;
            MaxRadius = maxRadius;
            Up = up;
            Sensitivity = 1;


            Input = new Input(game);

            Input.MouseDown += new EventHandler<MouseEventArgs>(Input_RightButtonDown);
            Input.MouseMove += new EventHandler<MouseEventArgs>(Input_MouseMove);
            Input.Wheel += new EventHandler<MouseEventArgs>(Input_Wheel);

            game.Components.Add(Input);
        }


        void Input_RightButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButton.Right)
            {
                start = ScreenToArcBall(e.X, e.Y);

                worldStart = world;
            }
        }

        void Input_MouseMove(object sender, MouseEventArgs e)
        {
			if (e.IsRightButtonDown)
			{
                end = ScreenToArcBall(e.X, e.Y);

                // Coordinate system conversion:
                // 
                // Flash Vector3D uses the right handed coordinate system,
                // meaning positive z axis points outside the screen, given
                // that x points to the right and y points up. While our 
                // Matrix44 uses the left handed system, a conversion has to
                // be made here that negate the z value of cross product.
                Vector3 v = Vector3.Cross(start, end);
                v.Normalize();

                float angle = (float)(Math.Acos(Vector3.Dot(start, end)));

                if (angle != 0 && v.LengthSquared() > 0)
                {
                    rotate = Matrix.CreateFromAxisAngle(v, angle);

                    world = Matrix.Multiply(worldStart, rotate);
                }
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
        
		
		private Vector3 ScreenToArcBall(float x, float y)
		{
            Vector3 result;

			x = x * 2 / Game.GraphicsDevice.Viewport.Width - 1;
            y = -y * 2 / Game.GraphicsDevice.Viewport.Height + 1;
			
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
