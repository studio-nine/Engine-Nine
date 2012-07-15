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

namespace Nine.Graphics.Cameras
{
    /// <summary>
    /// A first person free camera.
    /// </summary>
    public class FreeCamera : ICamera, IUpdateable
    {
        Vector2 mouseDown;

        public Vector3 Angle
        { 
            get { return angle; } 
            set { angle = value; }
        }
        Vector3 angle;

        public Vector3 Position 
        { 
            get { return position; }
            set { position = value; } 
        }
        Vector3 position;

        public Viewport? Viewport { get; set; }
        public float Speed { get; set; }
        public float TurnSpeed { get; set; }

        public Matrix View 
        {
            get { return view; }
        }
        Matrix view;

        public Matrix Projection
        {
            get { return projection; }
        }
        Matrix projection;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public FreeCamera(GraphicsDevice graphics) : this(graphics, Vector3.Zero, 30.0f, 20f) { }
        public FreeCamera(GraphicsDevice graphics, Vector3 position) : this(graphics, position, 30.0f, 20f) { }
        public FreeCamera(GraphicsDevice graphics, Vector3 position, float speed, float turnSpeed)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            this.Speed = speed;
            this.TurnSpeed = turnSpeed;
            this.Position = position;
            this.GraphicsDevice = graphics;
        }

        public void Update(TimeSpan elapsedTime)
        {
            // Assume screen size always greater then 100
            float delta = (float)elapsedTime.TotalSeconds;
            Vector2 move = Vector2.Zero;

#if !SILVERLIGHT
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
            if (gamePad.IsConnected)
            {
                angle.X -= gamePad.ThumbSticks.Right.Y * TurnSpeed * 0.001f;
                angle.Y += gamePad.ThumbSticks.Right.X * TurnSpeed * 0.001f;

                move.X = gamePad.ThumbSticks.Left.Y * Speed * delta;
                move.Y = gamePad.ThumbSticks.Left.X * Speed * delta;
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

#if !SILVERLIGHT
                if (keyboard.IsKeyDown(Keys.LeftShift))
                    delta /= 3;
#endif
                if (keyboard.IsKeyDown(Keys.W))
                    move.X += Speed * delta;

                if (keyboard.IsKeyDown(Keys.S))
                    move.X -= Speed * delta;

                if (keyboard.IsKeyDown(Keys.A))
                    move.Y += Speed * delta;

                if (keyboard.IsKeyDown(Keys.D))
                    move.Y -= Speed * delta;

                if (keyboard.IsKeyDown(Keys.Z))
                    position += Vector3.Down * Speed * delta;

                if (keyboard.IsKeyDown(Keys.X))
                    position += Vector3.Up * Speed * delta;
            }

            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000, out projection);
            Matrix.CreateFromYawPitchRoll(-angle.Y, -angle.X, -angle.Z, out view);
            view.M41 = position.X += view.Forward.X * move.X + view.Left.X * move.Y;
            view.M42 = position.Y += view.Forward.Y * move.X + view.Left.Y * move.Y;
            view.M43 = position.Z += view.Forward.Z * move.X + view.Left.Z * move.Y;
            Matrix.Invert(ref view, out view);
        }
    }
}
