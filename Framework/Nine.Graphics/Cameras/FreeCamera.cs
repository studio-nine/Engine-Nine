namespace Nine.Graphics.Cameras
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    /// <summary>
    /// A first person free camera.
    /// </summary>
    public class FreeCamera : Camera
    {
        public Vector3 Angle
        { 
            get { return angle; } 
            set { angle = value; }
        }
        private Vector3 angle;

        public Vector3 Position 
        { 
            get { return position; }
            set { position = value; } 
        }
        private Vector3 position;

        public float TurnSpeed { get; set; }
        public float Speed { get; set; }
        public float PrecisionModeSpeed { get; set; }

        public Keys ForwardKey { get; set; }
        public Keys BackwardKey { get; set; }
        public Keys LeftKey { get; set; }
        public Keys RightKey { get; set; }
        public Keys UpKey { get; set; }
        public Keys DownKey { get; set; }
        public Keys PrecisionModeKey { get; set; }

        private Vector2 mouseDown;

        public FreeCamera(GraphicsDevice graphics) : this(graphics, Vector3.Zero, 10.0f, 16f) { }
        public FreeCamera(GraphicsDevice graphics, Vector3 position) : this(graphics, position, 10.0f, 16f) { }
        public FreeCamera(GraphicsDevice graphics, Vector3 position, float speed, float turnSpeed) : base(graphics)
        {
            this.Speed = speed;
            this.PrecisionModeSpeed = speed / 5;
            this.TurnSpeed = turnSpeed;
            this.Position = position;
            this.ForwardKey = Keys.W;
            this.BackwardKey = Keys.S;
            this.LeftKey = Keys.A;
            this.RightKey = Keys.D;
            this.UpKey = Keys.X;
            this.DownKey = Keys.Z;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            // Assume screen size always greater then 100
            var delta = (float)elapsedTime.TotalSeconds;
            var move = Vector2.Zero;
            var speed = Speed;
            
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
                    angle.X += MathHelper.ToRadians((mouse.Y - centerY) * TurnSpeed * delta); // pitch
                    angle.Y += MathHelper.ToRadians((mouse.X - centerX) * TurnSpeed * delta); // yaw
                }

                mouseDown.X = mouse.X;
                mouseDown.Y = mouse.Y;

                if (keyboard.IsKeyDown(PrecisionModeKey))
                    speed = PrecisionModeSpeed;

                if (keyboard.IsKeyDown(ForwardKey))
                    move.X += speed * delta;
                if (keyboard.IsKeyDown(BackwardKey))
                    move.X -= speed * delta;
                if (keyboard.IsKeyDown(LeftKey))
                    move.Y += speed * delta;
                if (keyboard.IsKeyDown(RightKey))
                    move.Y -= Speed * delta;

                if (keyboard.IsKeyDown(DownKey))
                    position += Vector3.Down * speed * delta;
                if (keyboard.IsKeyDown(UpKey))
                    position += Vector3.Up * speed * delta;
            }

            Matrix.CreateFromYawPitchRoll(-angle.Y, -angle.X, -angle.Z, out transform);
            transform.M41 = position.X += transform.Forward.X * move.X + transform.Left.X * move.Y;
            transform.M42 = position.Y += transform.Forward.Y * move.X + transform.Left.Y * move.Y;
            transform.M43 = position.Z += transform.Forward.Z * move.X + transform.Left.Z * move.Y;
            
            NotifyTransformChanged();
        }
    }
}
