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
using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif
#endregion

namespace Nine.Graphics
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
    /// <summary>
    /// A first person free camera.
    /// </summary>
    public class FreeCamera : ICamera, IUpdateable
    {
        Vector3 angle;
        Vector3 position;
        Vector2 mouseDown;

        public Viewport? Viewport { get; set; }
        public Vector3 Angle { get { return angle; } set { angle = value; } }
        public float Speed { get; set; }
        public float TurnSpeed { get; set; }
        public Vector3 Position
        {
            get 
            {
                Vector3 v = new Vector3();
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

        public GraphicsDevice GraphicsDevice { get; private set; }

        public FreeCamera(GraphicsDevice graphics) : this(graphics, Vector3.Zero, 30.0f, 20f) { }
        public FreeCamera(GraphicsDevice graphics, Vector3 position) : this(graphics, position, 30.0f, 20f) { }
        public FreeCamera(GraphicsDevice graphics, Vector3 position, float speed, float turnSpeed)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            this.Speed = speed;
            this.TurnSpeed = turnSpeed;
            Position = position;
            GraphicsDevice = graphics;
        }


        public void Update(TimeSpan elapsedTime)
        {
            // Assume screen size always greater then 100
            float delta = (float)elapsedTime.TotalSeconds;
            Vector3 forward;
            Vector3 left;

#if !SILVERLIGHT
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
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
#endif
            {
                KeyboardState keyboard = Keyboard.GetState();
                MouseState mouse = Mouse.GetState();

                float centerX = mouseDown.X;
                float centerY = mouseDown.Y;

                if (mouse.RightButton == ButtonState.Pressed)
                {
                    angle.X += MathHelper.ToRadians((mouse.Y - centerY) * TurnSpeed * 0.01f); // pitch
                    angle.Y += MathHelper.ToRadians((mouse.X - centerX) * TurnSpeed * 0.01f); // yaw
                }

                mouseDown.X = mouse.X;
                mouseDown.Y = mouse.Y;

                forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
                left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));

#if !SILVERLIGHT
                if (keyboard.IsKeyDown(Keys.LeftShift))
                    delta /= 3;
#endif
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

                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            }
        }
    }
}
