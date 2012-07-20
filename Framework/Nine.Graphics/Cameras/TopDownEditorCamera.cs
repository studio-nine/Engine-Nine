namespace Nine.Graphics.Cameras
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines a camera used to edit worlds.
    /// </summary>
    public class TopDownEditorCamera : ICamera
    {
        public Input Input { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Viewport? Viewport { get; set; }

        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Radius { get; set; }
        public float MinRadius { get; set; }
        public float MaxRadius { get; set; }
        public float Sensitivity { get; set; }
        public float Speed { get; set; }

        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        private Vector3 target = Vector3.Zero;
        private Point startPoint = Point.Zero;


        public Matrix View
        {
            get
            {
                Vector3 eye = new Vector3();

                eye.Y = (float)Math.Sin(Pitch) * Radius;
                eye.Z = (float)Math.Cos(Pitch) * Radius;
                eye.X = (float)Math.Cos(Yaw) * eye.Z;
                eye.Z = (float)Math.Sin(Yaw) * eye.Z;

                eye += target;

                return Matrix.CreateLookAt(eye, target, Vector3.Up);
            }
        }

        public Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, MinRadius / 10, MaxRadius * 2);
            }
        }

        public TopDownEditorCamera(GraphicsDevice graphics) : this(graphics, 50, MathHelper.PiOver2 * 0.6f) { }

        public TopDownEditorCamera(GraphicsDevice graphics, float radius, float pitch)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;

            Sensitivity = 1.0f;
            Speed = 0.04f;

            Yaw = MathHelper.PiOver2;
            Radius = radius;
            MinRadius = 1f;
            MaxRadius = 500f;
            Pitch = pitch;

            Input = new Input();
            Input.MouseDown += new EventHandler<MouseEventArgs>(Input_MouseDown);
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

            float dx = -state.ThumbSticks.Right.X * Sensitivity;
            float dz = state.ThumbSticks.Right.Y * Sensitivity;
            
            target.X -= ((float)Math.Cos(Yaw) * dz + (float)Math.Sin(Yaw) * dx) * 0.1f;
            target.Z -= ((float)Math.Sin(Yaw) * dz - (float)Math.Cos(Yaw) * dx) * 0.1f;
#endif
        }


        void Input_Wheel(object sender, MouseEventArgs e)
        {
            Radius -= e.WheelDelta * (MaxRadius - MinRadius) * 0.0001f * Sensitivity;

            if (Radius < MinRadius)
                Radius = MinRadius;
            else if (Radius > MaxRadius)
                Radius = MaxRadius;
        }

        void Input_MouseDown(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
            if (e.Button == MouseButtons.Left)
#else
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
#endif
            {
                startPoint.X = e.X;
                startPoint.Y = e.Y;
            }
        }

        void Input_MouseMove(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
            if (e.IsButtonDown(MouseButtons.Left))
#else
            if (e.IsButtonDown(MouseButtons.Right))
#endif
            {
                float dx = e.X - startPoint.X;
                float dz = e.Y - startPoint.Y;

                startPoint.X = e.X;
                startPoint.Y = e.Y;

                target.X -= ((float)Math.Cos(Yaw) * dz + (float)Math.Sin(Yaw) * dx) * Speed;
                target.Z -= ((float)Math.Sin(Yaw) * dz - (float)Math.Cos(Yaw) * dx) * Speed;
            }
            else if (e.IsMiddleButtonDown)
            {
                float dx = e.X - startPoint.X;

                startPoint.X = e.X;

                Yaw += dx * 0.002f * MathHelper.Pi;
            }
        }
    }
}
