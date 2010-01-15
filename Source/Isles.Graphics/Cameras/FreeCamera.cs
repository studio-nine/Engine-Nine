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
using Microsoft.Xna.Framework.Input;
#endregion


namespace Isles.Graphics.Cameras
{
    /*
     * Created by Javier Cant Ferrero.
     * MVP Windows-DirectX 2007/2008
     * DotnetClub Sevilla
     * Date 17/02/2007
     * Web www.codeplex.com/XNACommunity
     * Email javiuniversidad@gmail.com
     * blog: mirageproject.blogspot.com
     */
    public class FreeCamera : ICamera
    {
        Vector3 angle;
        Vector3 position;

        public Vector3 Angle { get { return angle; } set { angle = value; } }
        public float Speed { get; set; }
        public float TurnSpeed { get; set; }
        public Vector3 Position
        {
            get 
            {
                Vector3 v;
                v.X = position.X;
                v.Y = -position.Z;
                v.Z = position.Y;
                return v;
            }
            set 
            {
                position.X = value.X;
                position.Y = value.Z;
                position.Z = -value.Y;
            }
        }


        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }


        public FreeCamera() : this(Vector3.Zero, 1.0f, 90f) { }

        public FreeCamera(Vector3 position, float speed, float turnSpeed)
        {
            this.Speed = speed;
            this.TurnSpeed = turnSpeed;
            this.position = position;
                
            Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, 4.0f / 3.0f, 1, 1000);
        }


        public void Update(GameTime gameTime)
        {
            // Assume screen size always greater then 100
            int center = 100;
            float delta = (float)gameTime.TotalGameTime.TotalSeconds;
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
            Vector3 forward;
            Vector3 left;

            if (gamePad.IsConnected)
            {
                angle.X -= gamePad.ThumbSticks.Right.Y * TurnSpeed * 0.001f;
                angle.Y += gamePad.ThumbSticks.Right.X * TurnSpeed * 0.001f;

                forward = Vector3.Normalize(new Vector3((float)Math.Sin(-Angle.Y), (float)Math.Sin(Angle.X), (float)Math.Cos(-Angle.Y)));
                left = Vector3.Normalize(new Vector3((float)Math.Cos(Angle.Y), 0f, (float)Math.Sin(Angle.Y)));

                position -= forward * Speed * gamePad.ThumbSticks.Left.Y * delta;
                position += left * Speed * gamePad.ThumbSticks.Left.X * delta;

                View = Matrix.CreateTranslation(-position);
                View *= Matrix.CreateRotationX(-MathHelper.PiOver2); 
                View *= Matrix.CreateRotationZ(Angle.Z);
                View *= Matrix.CreateRotationY(Angle.Y);
                View *= Matrix.CreateRotationX(Angle.X);
            }
            else
            {
                KeyboardState keyboard = Keyboard.GetState();
                MouseState mouse = Mouse.GetState();

                int centerX = center;
                int centerY = center;

                Mouse.SetPosition(centerX, centerY);

                angle.X += MathHelper.ToRadians((mouse.Y - centerY) * TurnSpeed * 0.01f); // pitch
                angle.Y += MathHelper.ToRadians((mouse.X - centerX) * TurnSpeed * 0.01f); // yaw

                forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
                left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));

                if (keyboard.IsKeyDown(Keys.W))
                    position -= forward * Speed * delta;

                if (keyboard.IsKeyDown(Keys.S))
                    position += forward * Speed * delta;

                if (keyboard.IsKeyDown(Keys.A))
                    position -= left * Speed * delta;

                if (keyboard.IsKeyDown(Keys.D))
                    position += left * Speed * delta;

                if (keyboard.IsKeyDown(Keys.Z))
                    position += Vector3.Down * Speed * delta;

                if (keyboard.IsKeyDown(Keys.X))
                    position += Vector3.Up * Speed * delta;

                View = Matrix.CreateRotationX(-MathHelper.PiOver2);
                View *= Matrix.CreateTranslation(-position);
                View *= Matrix.CreateRotationZ(Angle.Z);
                View *= Matrix.CreateRotationY(Angle.Y);
                View *= Matrix.CreateRotationX(Angle.X);
            }
        }
    }
}
