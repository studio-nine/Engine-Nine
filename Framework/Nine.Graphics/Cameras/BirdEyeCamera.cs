namespace Nine.Graphics.Cameras
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines a camera that emulates the bird eye view.
    /// </summary>
    public class BirdEyeCamera : Camera
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Radius { get; set; }
        public float MinRadius { get; set; }
        public float MaxRadius { get; set; }
        public float WheelSpeed { get; set; }
        public float Speed { get; set; }

        public MouseButtons RotateButton { get; set; }
        public MouseButtons TranslateButton { get; set; }

        public Vector3 LookAt
        {
            get { return lookAt; }
            set { lookAt = value; }
        }

        private Input input;
        private Vector3 lookAt = Vector3.Zero;
        private Point startPoint = Point.Zero;

        public BirdEyeCamera(GraphicsDevice graphics) : this(graphics, 50, MathHelper.PiOver2 * 0.6f) { }
        public BirdEyeCamera(GraphicsDevice graphics, float radius, float pitch) : base(graphics)
        {
#if WINDOWS_PHONE
            TranslateButton = MouseButtons.Left;
#else
            RotateButton = MouseButtons.Middle;
            TranslateButton = MouseButtons.Right;
#endif

            WheelSpeed = 1.0f;
            Speed = 1.0f;

            Yaw = MathHelper.PiOver2;
            Radius = radius;
            MinRadius = 1f;
            MaxRadius = 500f;
            Pitch = pitch;

            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(Input_MouseDown);
            input.MouseMove += new EventHandler<MouseEventArgs>(Input_MouseMove);
            input.MouseWheel += new EventHandler<MouseEventArgs>(Input_Wheel);
            input.Update += new EventHandler<EventArgs>(Input_Update);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var forward = new Vector3();
            forward.Y = -(float)Math.Sin(Pitch) * Radius;
            forward.Z = -(float)Math.Cos(Pitch) * Radius;
            forward.X = -(float)Math.Cos(Yaw) * forward.Z;
            forward.Z = -(float)Math.Sin(Yaw) * forward.Z;

            var up = Vector3.Up;
            var eye = lookAt - forward;
            Matrix.CreateWorld(ref eye, ref forward, ref up, out transform);
            NotifyTransformChanged();
        }

        void Input_Update(object sender, EventArgs e)
        {
            if (!Enabled)
                return;

#if XBOX
            GamePadState state = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            if (state.Buttons.LeftShoulder == ButtonState.Pressed)
                Radius -= (MaxRadius - MinRadius) * 0.005f * Speed;
            if (state.Buttons.RightShoulder == ButtonState.Pressed)
                Radius += (MaxRadius - MinRadius) * 0.005f * Speed;

            if (Radius < MinRadius)
                Radius = MinRadius;
            else if (Radius > MaxRadius)
                Radius = MaxRadius;

            float dx = -state.ThumbSticks.Right.X * Speed * 0.04f;
            float dz = state.ThumbSticks.Right.Y * Speed * 0.04f;
            
            lookAt.X -= ((float)Math.Cos(Yaw) * dz + (float)Math.Sin(Yaw) * dx) * 0.1f;
            lookAt.Z -= ((float)Math.Sin(Yaw) * dz - (float)Math.Cos(Yaw) * dx) * 0.1f;
#endif
        }


        void Input_Wheel(object sender, MouseEventArgs e)
        {
            if (!Enabled)
                return;

            Radius -= e.WheelDelta * (MaxRadius - MinRadius) * 0.0001f * WheelSpeed;

            if (Radius < MinRadius)
                Radius = MinRadius;
            else if (Radius > MaxRadius)
                Radius = MaxRadius;
        }

        void Input_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Enabled)
                return;

            if (e.Button == RotateButton || e.Button == TranslateButton)
            {
                startPoint.X = e.X;
                startPoint.Y = e.Y;
            }
        }

        void Input_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Enabled)
                return;

            if (e.IsButtonDown(TranslateButton))
            {
                float dx = e.X - startPoint.X;
                float dz = e.Y - startPoint.Y;

                startPoint.X = e.X;
                startPoint.Y = e.Y;

                lookAt.X += ((float)Math.Cos(Yaw) * dz + (float)Math.Sin(Yaw) * dx) * Speed * 0.04f;
                lookAt.Z += ((float)Math.Sin(Yaw) * dz - (float)Math.Cos(Yaw) * dx) * Speed * 0.04f;
            }
            else if (e.IsButtonDown(RotateButton))
            {
                float dx = e.X - startPoint.X;

                startPoint.X = e.X;

                Yaw += dx * 0.002f * MathHelper.Pi;
            }
        }
    }
}
