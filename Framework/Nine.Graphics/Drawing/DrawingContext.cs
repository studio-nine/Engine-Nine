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
        #region Properties
        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the graphics settings
        /// </summary>
        public Settings Settings { get; private set; }

        /// <summary>
        /// Gets the graphics statistics.
        /// </summary>
        public Statistics Statistics { get; private set; }

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
        public PassGroup MainPass { get; private set; }

        /// <summary>
        /// Gets the root pass of this drawing context composition chain.
        /// </summary>
        public PassGroup RootPass { get; private set; }

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
        public MatrixCollection Matrices
        {
            get { return matrices; }
        }
        internal MatrixCollection matrices;

        public TextureCollection Textures
        {
            get { return textures; }
        }
        internal TextureCollection textures;
        #endregion

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

        #region Fields
        private bool isDrawing = false;
        private FastList<Pass> activePasses = new FastList<Pass>();
        private FastList<IPostEffect> targetPasses = new FastList<IPostEffect>();
        
        private FastList<IDrawableObject> dynamicDrawables = new FastList<IDrawableObject>();
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of <c>DrawContext</c>.
        /// </summary>
        public DrawingContext(GraphicsDevice graphics, Settings settings)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (settings == null)
                throw new ArgumentNullException("settings");

            Settings = settings;
            GraphicsDevice = graphics;
            Statistics = new Statistics();
            ambientLight = new Versioned<Vector3>();
            DirectionalLights = new DirectionalLightCollection();
            Fog = new Versioned<IEffectFog>();
            Fog.Value = new FogProperty(Fog);
            matrices = new MatrixCollection();
            textures = new TextureCollection();
            MainPass = new PassGroup();
            MainPass.Passes.Add(new DrawingPass());
            RootPass = new PassGroup();
            RootPass.Passes.Add(MainPass);
        }

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, ISpatialQuery<IDrawableObject> scene, Matrix view, Matrix projection, Settings settings)
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
                if (RootPass == null)
                    return;

                dynamicDrawables.Clear();
                BoundingFrustum viewFrustum = ViewFrustum;
                scene.FindAll(ref viewFrustum, dynamicDrawables);

                activePasses.Clear();
                RootPass.GetActivePasses(activePasses);

                targetPasses.Resize(activePasses.Count);

                // Determines which pass should be rendered to a texture.
                int lastPass = 0;
                IPostEffect lastPostEffect = null;
                for (int i = activePasses.Count - 1; i >= 0; i--)
                {
                    var postEffect = activePasses[i] as IPostEffect;

                    if (lastPostEffect != null && (postEffect != null) || (i == 0))
                        targetPasses[i] = lastPostEffect;
                    else
                        targetPasses[i] = null;

                    if (postEffect != null)
                    {
                        lastPostEffect = postEffect;
                        if (lastPass == 0)
                            lastPass = i;
                    }
                }


                RenderTarget2D lastRenderTarget = null;
                RenderTarget2D intermediate = null;
                bool overrideViewFrustumLastPass = false;

                for (int i = 0; i < activePasses.Count; i++)
                {
                    var pass = activePasses[i];
                    var targetPass = targetPasses[i];
                    var overrideViewFrustum = false;

                    // Query the drawables in the current view frustum only when the view frustum changed
                    // or the pass overrides the frustum.
                    var passView = pass.View;
                    var passProjection = pass.Projection;

                    if (passView.HasValue)
                    {
                        View = passView.Value;
                        overrideViewFrustum = true;
                    }

                    if (passProjection.HasValue)
                    {
                        overrideViewFrustum = true;
                        Projection = passProjection.Value;
                    }

                    if (overrideViewFrustum || overrideViewFrustumLastPass)
                    {
                        dynamicDrawables.Clear();
                        BoundingFrustum frustum = matrices.ViewFrustum;
                        Scene.FindAll(ref frustum, dynamicDrawables);
                        overrideViewFrustumLastPass = overrideViewFrustum;
                    }

                    try
                    {
                        RenderTargetPool.Lock(lastRenderTarget);

                        if (targetPass != null)
                        {
                            intermediate = pass.PrepareRenderTarget(this, intermediate);
                            intermediate.Begin();
                            RenderTargetPool.Lock(intermediate);
                        }

                        var postEffect = pass as IPostEffect;
                        if (postEffect != null)
                            postEffect.InputTexture = lastRenderTarget;

                        // Clear the screen when we are drawing to the backbuffer.
                        if (i == lastPass)
                            GraphicsDevice.Clear(Settings.BackgroundColor);

                        pass.Draw(this, dynamicDrawables);
                    }
                    finally
                    {
                        RenderTargetPool.Unlock(lastRenderTarget);

                        if (targetPass != null)
                        {
                            intermediate.End();
                            RenderTargetPool.Unlock(intermediate);
                            lastRenderTarget = intermediate;
                        }
                    }
                }
            }
            finally
            {
                CurrentFrame++;
                isDrawing = false;
            }
        }
        #endregion
    }
}