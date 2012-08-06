namespace Nine.Graphics.Cameras
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;

    /// <summary>
    /// Defines a camera that can be attacked to a <see cref="Transformable"/>.
    /// </summary>
    [ContentSerializable]
    public class Camera : Transformable, ICamera
    {
        #region View
        private Matrix view = Matrix.Identity;

        /// <summary>
        /// Gets the camera view matrix
        /// </summary>
        public Matrix View
        {
            get { return view; }
        }

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            view = Matrix.Invert(AbsoluteTransform);
        }
        #endregion

        #region Projection
        private int previousDefaultViewportWidth;
        private int previousDefaultViewportHeight;
        private float fieldOfView = MathHelper.PiOver4;
        private float nearPlaneDistance = 1;
        private float farPlaneDistance = 1000;
        private Matrix projectionMatrix;
        private bool projectionMatrixNeedsUpdate;

        /// <summary>
        /// Gets or sets the distance to the camera near clip plane.
        /// </summary>
        public float NearPlane
        {
            get { return nearPlaneDistance; }
            set { nearPlaneDistance = value; projectionMatrixNeedsUpdate = true; }
        }

        /// <summary>
        /// Gets or sets the distance to the camera far clip plane.
        /// </summary>
        public float FarPlane
        {
            get { return farPlaneDistance; }
            set { farPlaneDistance = value; projectionMatrixNeedsUpdate = true; }
        }

        /// <summary>
        /// Gets or sets camera aspect ratio.
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return (viewport.HasValue ? viewport.Value.AspectRatio : GraphicsDevice.Viewport.AspectRatio)
                     * (viewportScale.Max.X - viewportScale.Min.X)
                     / (viewportScale.Max.Y - viewportScale.Min.Y);
            }
        }

        /// <summary>
        /// Gets or sets camera field of view in radians.
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; projectionMatrixNeedsUpdate = true; }
        }

        /// <summary>
        /// Gets the camera projection matrix
        /// </summary>
        public Matrix Projection
        {
            get { UpdateProjectionMatrix(); return projectionMatrix; }
        }

        /// <summary>
        /// Performs update of ProjectionMatrix.
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            // Check whether GraphicsDevice.Viewport has changed
            if (!viewport.HasValue)
            {
                if (GraphicsDevice.Viewport.Width != previousDefaultViewportWidth ||
                    GraphicsDevice.Viewport.Height != previousDefaultViewportHeight)
                {
                    previousDefaultViewportWidth = GraphicsDevice.Viewport.Width;
                    previousDefaultViewportHeight = GraphicsDevice.Viewport.Height;
                    projectionMatrixNeedsUpdate = true;
                }
            }

            if (projectionMatrixNeedsUpdate)
            {
                Matrix.CreatePerspectiveFieldOfView(fieldOfView, AspectRatio, nearPlaneDistance, farPlaneDistance, out projectionMatrix);
                projectionMatrixNeedsUpdate = false;
            }
        }
        #endregion

        #region Viewport
        /// <summary>
        /// Gets or sets the viewport of this camera.
        /// </summary>
        public Viewport? Viewport
        {
            get { return viewport; }
            set { viewport = value; projectionMatrixNeedsUpdate = true; }
        }
        private Viewport? viewport;

        /// <summary>
        /// Gets or sets the scale factor that is applied to the viewport.
        /// </summary>
        public BoundingRectangle ViewportScale
        {
            get { return viewportScale; }
            set { viewportScale = value; projectionMatrixNeedsUpdate = true; }
        }
        private BoundingRectangle viewportScale = new BoundingRectangle(Vector2.Zero, Vector2.One);
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether this <see cref="Camera"/> is enabled.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// If a valid graphics device is specified, the camera will automatically adjust 
        /// its aspect ratio based on the default viewport settings of the graphics device.
        /// </summary>
        public Camera(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Enabled = true;
            GraphicsDevice = graphics;            
            projectionMatrixNeedsUpdate = true;
        }
        #endregion
    }
}