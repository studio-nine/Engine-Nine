namespace Nine.Graphics
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    /// <summary>
    /// A first person free camera.
    /// </summary>
    public class FreeCamera : Camera, Nine.IUpdateable
    {
        public Vector3 Angle
        { 
            get { return angle; }
            set { angle = value; UpdateTransform(Vector2.Zero); }
        }
        private Vector3 angle;

        public Vector3 Position 
        { 
            get { return position; }
            set { position = value; UpdateTransform(Vector2.Zero); } 
        }
        private Vector3 position;

        public bool InputEnabled { get; set; }

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
        private bool mouseDownHasValue;

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

            UpdateTransform(Vector2.Zero);
        }

        public void Update(float elapsedTime)
        {
            if (InputEnabled)
            {
                // Assume screen size always greater then 100
                var move = Vector2.Zero;
                var speed = Speed;

                GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
                if (gamePad.IsConnected)
                {
                    angle.X -= gamePad.ThumbSticks.Right.Y * TurnSpeed * 0.001f;
                    angle.Y += gamePad.ThumbSticks.Right.X * TurnSpeed * 0.001f;

                    move.X = gamePad.ThumbSticks.Left.Y * Speed * elapsedTime;
                    move.Y = gamePad.ThumbSticks.Left.X * Speed * elapsedTime;
                }
                else
                {
                    KeyboardState keyboard = Keyboard.GetState();
                    MouseState mouse = Mouse.GetState();

                    float centerX = mouseDown.X;
                    float centerY = mouseDown.Y;

#if WINDOWS_PHONE
                if (mouse.LeftButton == ButtonState.Pressed)
#else
                    if (mouse.RightButton == ButtonState.Pressed)
#endif
                    {
                        if (mouseDownHasValue)
                        {
                            angle.X += MathHelper.ToRadians((mouse.Y - centerY) * TurnSpeed * elapsedTime) / AspectRatio; // pitch
                            angle.Y += MathHelper.ToRadians((mouse.X - centerX) * TurnSpeed * elapsedTime); // yaw
                        }
                        mouseDown.X = mouse.X;
                        mouseDown.Y = mouse.Y;
                        mouseDownHasValue = true;
                    }
                    else
                    {
                        mouseDownHasValue = false;
                    }

                    if (keyboard.IsKeyDown(PrecisionModeKey))
                        speed = PrecisionModeSpeed;

                    if (keyboard.IsKeyDown(ForwardKey))
                        move.X += speed * elapsedTime;
                    if (keyboard.IsKeyDown(BackwardKey))
                        move.X -= speed * elapsedTime;
                    if (keyboard.IsKeyDown(LeftKey))
                        move.Y += speed * elapsedTime;
                    if (keyboard.IsKeyDown(RightKey))
                        move.Y -= Speed * elapsedTime;

                    if (keyboard.IsKeyDown(DownKey))
                        position += Vector3.Down * speed * elapsedTime;
                    if (keyboard.IsKeyDown(UpKey))
                        position += Vector3.Up * speed * elapsedTime;
                }
                UpdateTransform(move);
            }
        }

        private void UpdateTransform(Vector2 move)
        {
            Matrix.CreateFromYawPitchRoll(-angle.Y, -angle.X, -angle.Z, out transform);
            transform.M41 = position.X += transform.Forward.X * move.X + transform.Left.X * move.Y;
            transform.M42 = position.Y += transform.Forward.Y * move.X + transform.Left.Y * move.Y;
            transform.M43 = position.Z += transform.Forward.Z * move.X + transform.Left.Z * move.Y;

            NotifyTransformChanged();
        }
    }
}
