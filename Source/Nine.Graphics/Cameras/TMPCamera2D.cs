namespace Nine.Graphics
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
    [Nine.Serialization.BinarySerializable]
    public class TMPCamera2D : Nine.Transformable, ICamera, IGraphicsObject
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
                var viewport = GraphicsDevice.Viewport;

                Position = new Vector2(viewport.Width / 2, viewport.Height / 2);

                Matrix.CreateOrthographic(
                    viewport.Width,
                    viewport.Height, 0, 1, out projection);

                return Matrix.CreateScale(1,-1,1) * projection;
            }
        }
        private Matrix projection;
        
        public float AspectRatio
        {
            get { return GraphicsDevice.Viewport.AspectRatio; }
        }

        #endregion

        public GraphicsDevice GraphicsDevice { get; private set; }

        #region Methods
        public TMPCamera2D(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
        }

        void IGraphicsObject.OnAdded(DrawingContext context)
        {
            if (context.camera == null)
                context.camera = this;
        }

        void IGraphicsObject.OnRemoved(DrawingContext context)
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
