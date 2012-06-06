#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Graphics.ObjectModel
{
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
        private bool needUpdateProjectionMatrix;

        /// <summary>
        /// Gets or sets the distance to the camera near clip plane.
        /// </summary>
        public float NearPlane
        {
            get { return nearPlaneDistance; }
            set { nearPlaneDistance = value; needUpdateProjectionMatrix = true; }
        }

        /// <summary>
        /// Gets or sets the distance to the camera far clip plane.
        /// </summary>
        public float FarPlane
        {
            get { return farPlaneDistance; }
            set { farPlaneDistance = value; needUpdateProjectionMatrix = true; }
        }

        /// <summary>
        /// Gets or sets camera aspect ratio.
        /// </summary>
        public float AspectRatio
        {
            get
            {
                if (viewport.HasValue)
                    return viewport.Value.AspectRatio;
                if (GraphicsDevice != null)
                    return (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;
                
                // Use this default aspect ratio if no viewport is specified.
                return 4.0f / 3;
            }
        }

        /// <summary>
        /// Gets or sets camera field of view in radians.
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; needUpdateProjectionMatrix = true; }
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
                    needUpdateProjectionMatrix = true;
                }
            }

            if (needUpdateProjectionMatrix)
            {
                Matrix.CreatePerspectiveFieldOfView(fieldOfView, AspectRatio, nearPlaneDistance, farPlaneDistance, out projectionMatrix);
                needUpdateProjectionMatrix = false;
            }
        }
        #endregion

        #region Viewport
        private Viewport? viewport;

        /// <summary>
        /// Gets or sets the viewport of this camera.
        /// </summary>
        public Viewport? Viewport
        {
            get { return viewport; }
            set { viewport = value; needUpdateProjectionMatrix = true; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether this <see cref="Camera"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// You have to explicitly set the viewport property if no graphics device is specified.
        /// </summary>
        public Camera() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// If a valid graphics device is specified, the camera will automatically adjust 
        /// its aspect ratio based on the default viewport settings of the graphics device.
        /// </summary>
        public Camera(GraphicsDevice graphics)
        {
            Enabled = true;
            GraphicsDevice = graphics;
            needUpdateProjectionMatrix = true;
        }
        #endregion
    }
}