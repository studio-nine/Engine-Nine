#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.ParticleEffects;
using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// A drawing context contains commonly used global parameters for rendering.
    /// </summary>
    public class DrawingContext
    {
        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the graphics settings
        /// </summary>
        public DrawingSettings Settings { get; private set; }

        /// <summary>
        /// Gets the graphics statistics.
        /// </summary>
        public DrawingStatistics Statistics { get; private set; }

        /// <summary>
        /// Gets the elapsed time since last update.
        /// </summary>
        public TimeSpan ElapsedTime { get; internal set; }

        /// <summary>
        /// Gets the total seconds since the beginning of the draw context.
        /// </summary>
        public TimeSpan TotalTime { get; internal set; }

        /// <summary>
        /// Gets the scene to be renderted with this drawing context.
        /// </summary>
        public ISpatialQuery<IDrawableObject> Scene { get; private set; }

        /// <summary>
        /// Gets the main pass that is used to render the scene.
        /// </summary>
        public DrawingPassGroup MainPass { get; private set; }

        /// <summary>
        /// Gets the root pass of this drawing context composition chain.
        /// </summary>
        public DrawingPassChain RootPass { get; private set; }

        /// <summary>
        /// Gets the number of elapsed frames since the beginning of the draw context.
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        /// Gets the view matrix for this drawing operation.
        /// </summary>
        public Matrix View
        {
            get { return matrices.view; }
            set { matrices.View = value; }
        }

        /// <summary>
        /// Gets the projection matrix for this drawing operation.
        /// </summary>
        public Matrix Projection
        {
            get { return matrices.projection; }
            set { matrices.Projection = value; }
        }

        /// <summary>
        /// Gets the eye position.
        /// </summary>
        public Vector3 EyePosition
        {
            get { return matrices.eyePosition; }
        }
        
        /// <summary>
        /// Gets the view frustum for this drawing operation.
        /// </summary>
        public BoundingFrustum ViewFrustum
        {
            get { return matrices.ViewFrustum; }
        }

        /// <summary>
        /// Gets commonly used matrices.
        /// </summary>
        public DrawingContextMatrixCollection Matrices
        {
            get { return matrices; }
        }
        internal DrawingContextMatrixCollection matrices;

        public DrawingContextTextureCollection Textures
        {
            get { return textures; }
        }
        internal DrawingContextTextureCollection textures;

        private bool isDrawing = false;

        #region Accelerated Global Properties
        /// <summary>
        /// Provides an optimization hint to opt-out parameters that are not 
        /// changed since last drawing operation.
        /// </summary>
        internal Material PreviousMaterial;

        /// <summary>
        /// SetVertexBuffer is not doing a good job filtering out duplicated vertex buffer due to
        /// multiple vertex buffer binding. Doing it manually here.
        /// </summary>
        /// <remarks>
        /// You should always use this SetVertexBuffer instead of GraphicsDevice.SetVertexBuffer.
        /// If you try to bind to multiple vertex buffers, use GraphicsDevice.SetVertexBuffer and
        /// call context.SetVertexBuffer(null, 0);
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            if (VertexBuffer != vertexBuffer || VertexOffset != vertexOffset)
            {
                GraphicsDevice.SetVertexBuffer(vertexBuffer, vertexOffset);
                VertexBuffer = vertexBuffer;
                VertexOffset = vertexOffset;
            }
        }

        private int VertexOffset;
        private VertexBuffer VertexBuffer;

        /// <summary>
        /// Gets the global ambient light color of this <see cref="DrawingContext"/>.
        /// </summary>
        public Versioned<Vector3> AmbientLight
        {
            get { return ambientLight; }
        }
        internal Versioned<Vector3> ambientLight;

        /// <summary>
        /// Gets a global sorted collection of directional lights of this <see cref="DrawingContext"/>.
        /// </summary>
        public DirectionalLightCollection DirectionalLights { get; private set; }

        /// <summary>
        /// Gets the default or main directional light of this <see cref="DrawingContext"/>.
        /// </summary>
        public DirectionalLight DirectionalLight
        {
            get { return DirectionalLights[0] ?? DirectionalLight.Empty; }
        }

        /// <summary>
        /// Gets the global fog settings of this <see cref="DrawingContext"/>.
        /// </summary>
        public Versioned<IEffectFog> Fog { get; private set; }
        #endregion
                
        /// <summary>
        /// Initializes a new instance of <c>DrawContext</c>.
        /// </summary>
        public DrawingContext(GraphicsDevice graphics, DrawingSettings settings)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (settings == null)
                throw new ArgumentNullException("settings");

            Settings = settings;
            GraphicsDevice = graphics;
            Statistics = new DrawingStatistics();
            ambientLight = new Versioned<Vector3>();
            DirectionalLights = new DirectionalLightCollection();
            Fog = new Versioned<IEffectFog>();
            Fog.Value = new FogProperty(Fog);
            matrices = new DrawingContextMatrixCollection();
            textures = new DrawingContextTextureCollection();
            MainPass = new DrawingPassGroup();
            MainPass.Passes.Add(new BasicDrawingPass());
            RootPass = new DrawingPassChain();
            RootPass.Passes.Add(MainPass);
        }

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, ISpatialQuery<IDrawableObject> scene, Matrix view, Matrix projection, DrawingSettings settings)
        {
            if (isDrawing)
                throw new InvalidOperationException("Cannot trigger another drawing of the scene while it's still been drawn");

            Scene = scene;
            View = view;
            Projection = projection;
            isDrawing = true;
            VertexOffset = 0;
            VertexBuffer = null;
            PreviousMaterial = null;
            ElapsedTime = elapsedTime;
            TotalTime += elapsedTime;

            try
            {   
                if (RootPass != null)
                {
                    DrawingPass.DynamicDrawables.Clear();
                    BoundingFrustum viewFrustum = ViewFrustum;
                    scene.FindAll(ref viewFrustum, DrawingPass.DynamicDrawables);

                    RootPass.Draw(this, DrawingPass.DynamicDrawables.Elements, 0, DrawingPass.DynamicDrawables.Count);
                }
            }
            finally
            {
                CurrentFrame++;
                isDrawing = false;
            }
        }
    }
}