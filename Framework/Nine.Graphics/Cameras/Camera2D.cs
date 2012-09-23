namespace Nine.Graphics.Cameras
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a 2D orthographic camera that uses a right handed 
    /// coordinate system with X axis pointing right, Y axis pointing
    /// up and Z axis pointing outside the screen. The origin
    /// of the coordinate system is the center of the screen.
    /// </summary>
    [ContentSerializable]
    public class Camera2D : Nine.Transformable, ICamera, ISceneObject
    {
        #region View
        public Matrix View
        {
            get
            {
                if (viewMatrixNeedsUpdate)
                {
                    var matrix = AbsoluteTransform;
                    view.M41 = -matrix.M41;
                    view.M42 = -matrix.M42;
                    viewMatrixNeedsUpdate = false;
                }
                return view;
            }
        }        
        private bool viewMatrixNeedsUpdate;
        private Matrix view = Matrix.Identity;

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                transform.M41 = value.X;
                transform.M42 = value.Y;
                NotifyTransformChanged();
            }
        }
        private Vector2 position;

        protected override void OnTransformChanged()
        {
            viewMatrixNeedsUpdate = true;
        }
        #endregion

        #region Projection
        public Matrix Projection
        {
            get
            {
                var scale = 0.5f / Zoom;
                var viewport = GraphicsDevice.Viewport;

                Matrix.CreateOrthographicOffCenter(
                    -viewport.Width * scale, viewport.Width * scale,
                    -viewport.Height * scale, viewport.Height * scale, 0, 1, out projection);
                
                return projection;
            }
        }
        private Matrix projection;
        
        public float AspectRatio
        {
            get { return GraphicsDevice.Viewport.AspectRatio; }
        }

        public float Zoom { get; set; }
        #endregion

        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

        public float MinZoom { get; set; }
        public float MaxZoom { get; set; }
        public float WheelSpeed { get; set; }
        
        public bool InputEnabled
        {
            get { return input.Enabled; }
            set { input.Enabled = value; }
        }

        private Input input;
        private Vector2 startMouse = Vector2.Zero;
        private Vector2 startPosition = Vector2.Zero;
        #endregion

        #region Methods
        public Camera2D(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;           
        
            Zoom = 1;
            MinZoom = 0.01f;
            MaxZoom = 10f;
            WheelSpeed = 1;

            input = new Input();

            input.MouseDown += new EventHandler<MouseEventArgs>(Input_ButtonDown);
            input.MouseMove += new EventHandler<MouseEventArgs>(Input_MouseMove);
            input.MouseWheel += new EventHandler<MouseEventArgs>(Input_Wheel);
        }

        private void Input_ButtonDown(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
            if (e.Button == MouseButtons.Left)
#else
            if (e.Button == MouseButtons.Right)
#endif
            {
                startMouse.X = e.X;
                startMouse.Y = e.Y;
                startPosition.X = position.X;
                startPosition.Y = position.Y;
            }
        }

        private void Input_MouseMove(object sender, MouseEventArgs e)
        {
#if WINDOWS_PHONE
            if (e.IsButtonDown(MouseButtons.Left))
#else
            if (e.IsButtonDown(MouseButtons.Right))
#endif
            {
                Position = new Vector2(startPosition.X - (e.X - startMouse.X) / Zoom,
                                       startPosition.Y + (e.Y - startMouse.Y) / Zoom);
            }
        }

        private void Input_Wheel(object sender, MouseEventArgs e)
        {
            float zoom = NormalizeZoom(this.Zoom);
            float maxZoom = NormalizeZoom(this.MaxZoom);
            float minZoom = NormalizeZoom(this.MinZoom);

            zoom += e.WheelDelta * (maxZoom - minZoom) * 0.0001f * WheelSpeed;
            zoom = MathHelper.Clamp(zoom, minZoom, maxZoom);
            this.Zoom = DenormalizeZoom(zoom);            
        }

        private float NormalizeZoom(float zoom)
        {
            if (zoom >= 1)
                return zoom - 1;
            return 1 - 1.0f / zoom;
        }

        private float DenormalizeZoom(float zoom)
        {
            if (zoom >= 0)
                return zoom = 1 + zoom;
            return zoom = 1.0f / (1 - zoom);
        }

        void ISceneObject.OnAdded(DrawingContext context)
        {
            if (context.camera == null)
                context.camera = this;
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {
            if (context.camera == this)
                context.camera = null;
        }

        bool ICamera.TryGetViewFrustum(out Matrix view, out Matrix projection)
        {
            view = this.View;
            projection = this.Projection;
            return true;
        }
        #endregion
    }
}
