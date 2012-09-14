namespace Nine.Graphics.Cameras
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines a camera used to view models.
    /// </summary>
    public class ModelViewerCamera : Camera
    {
        public float Radius { get; set; }
        public float MinRadius { get; set; }
        public float MaxRadius { get; set; }
        public float WheelSpeed { get; set; }

        public Vector3 Center 
        {
            get { return center; }
            set { center = value; }
        }

        public bool MouseWheelEnabled { get; set; }
        public MouseButtons RotateButton { get; set; }

        private Input input;
        private Vector3 center;
        private Vector3 start = Vector3.Zero;
        private Vector3 end = Vector3.Zero;
        private Matrix rotation = Matrix.Identity;
        private Matrix rotationStart = Matrix.Identity;

        public ModelViewerCamera(GraphicsDevice graphics) : this(graphics, 20, 1, 100) { }
        public ModelViewerCamera(GraphicsDevice graphics, float radius, float minRadius, float maxRadius) : base(graphics)
        {
#if WINDOWS_PHONE
            RotateButton = MouseButtons.Left;
#else
            RotateButton = MouseButtons.Right;
#endif
            Radius = radius;
            MinRadius = minRadius;
            MaxRadius = maxRadius;
            WheelSpeed = 1;
            MouseWheelEnabled = true;

            input = new Input();

            input.MouseDown += new EventHandler<MouseEventArgs>(Input_ButtonDown);
            input.MouseMove += new EventHandler<MouseEventArgs>(Input_MouseMove);
            input.MouseWheel += new EventHandler<MouseEventArgs>(Input_Wheel);
            input.Update += new EventHandler<EventArgs>(Input_Update);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            transform = Matrix.Identity;
            transform.M43 = -Radius;

            Matrix.Multiply(ref rotation, ref transform, out transform);
            Matrix.Invert(ref transform, out transform);

            transform.M41 += center.X;
            transform.M42 += center.Y;
            transform.M43 += center.Z;

            NotifyTransformChanged();
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
            if (!Enabled)
                return;

            if (e.Button == RotateButton)
            {
                BeginRotation(e.X, e.Y);
            }
        }

        void Input_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Enabled)
                return;

            if (e.IsButtonDown(RotateButton))
            {
                EndRotation(e.X, e.Y);
            }
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
        
        private void BeginRotation(float x, float y)
        {
            start = ScreenToArcBall(x, y);
            rotationStart = rotation;
        }

        private void EndRotation(float x, float y)
        {
            end = ScreenToArcBall(x, y);

            Vector3 v;
            Vector3.Cross(ref start, ref end, out v);
            v.Normalize();

            float dot;
            Vector3.Dot(ref start, ref end, out dot);
            float angle = (float)(Math.Acos(dot));

            if (angle != 0 && v.LengthSquared() > 0)
            {
                Matrix rotate;
                Matrix.CreateFromAxisAngle(ref v, angle, out rotate);
                Matrix.Multiply(ref rotationStart, ref rotate, out rotation);
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
