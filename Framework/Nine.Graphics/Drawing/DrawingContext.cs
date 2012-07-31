namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.ObjectModel;
    using Nine.Graphics.PostEffects;
    using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;

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
        public Settings Settings
        {
            get { return settings; }
            set { settings = (value ?? new Settings()); }
        }
        private Settings settings;

        /// <summary>
        /// Gets the graphics statistics.
        /// </summary>
        public Statistics Statistics { get; private set; }

        /// <summary>
        /// Gets the elapsed time since last update.
        /// </summary>
        public TimeSpan ElapsedTime { get; internal set; }

        /// <summary>
        /// Gets the elapsed time from last frame in seconds.
        /// </summary>
        internal float ElapsedSeconds;

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
        /// Gets a collection of global textures.
        /// </summary>
        public TextureCollection Textures
        {
            get { return textures; }
        }
        internal TextureCollection textures;
        #endregion

        #region Matrices
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
        #endregion

        #region Lights
        /// <summary>
        /// Gets the global ambient light color of this <see cref="DrawingContext"/>.
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; ambientLightColorVersion++; }
        }
        internal Vector3 ambientLightColor;
        internal int ambientLightColorVersion;


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
        #endregion

        #region Fog
        /// <summary>
        /// Gets the global fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; fogVersion++; }
        }
        internal Vector3 fogColor = MaterialConstants.FogColor;

        /// <summary>
        /// Gets or sets the fog end.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }
            set { fogEnd = value; fogVersion++; }
        }
        internal float fogEnd = MaterialConstants.FogEnd;

        /// <summary>
        /// Gets or sets the fog start.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }
            set { fogStart = value; fogVersion++; }
        }
        internal float fogStart = MaterialConstants.FogStart;
        internal int fogVersion;
        #endregion

        #region Fields
        private bool isDrawing = false;
        private FastList<Pass> activePasses = new FastList<Pass>();
        private FastList<IPostEffect> targetPasses = new FastList<IPostEffect>();
        private FastList<SurfaceFormat?> preferedFormats = new FastList<SurfaceFormat?>();
        private FastList<IDrawableObject> dynamicDrawables = new FastList<IDrawableObject>();
        private FastList<Type> dependentPassTypes = new FastList<Type>();
        private Dictionary<Type, Pass> dependentPassMapping = new Dictionary<Type, Pass>();
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        public DrawingContext(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Settings = new Settings();
            GraphicsDevice = graphics;
            Statistics = new Statistics();            
            DirectionalLights = new DirectionalLightCollection();
            matrices = new MatrixCollection();
            textures = new TextureCollection();
            MainPass = new PassGroup() { Name="MainPass" };
            MainPass.Passes.Add(new DrawingPass());
            RootPass = new PassGroup() { Name = "RootPass" };
            RootPass.Passes.Add(MainPass);
        }

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
        /// Provides an optimization hint to opt-out parameters that are not 
        /// changed since last drawing operation.
        /// </summary>
        internal Material PreviousMaterial;

        /// <summary>
        /// Gets the half pixel for the current viewport.
        /// </summary>
        internal void GetHalfPixel(out Vector2 halfPixel)
        {
            var viewport = GraphicsDevice.Viewport;
            halfPixel = new Vector2();
            halfPixel.X = 0.5f / viewport.Width;
            halfPixel.Y = 0.5f / viewport.Height;
        }

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, ISpatialQuery<IDrawableObject> scene, Matrix view, Matrix projection)
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
            ElapsedSeconds = (float)elapsedTime.TotalSeconds;
            TotalTime += elapsedTime;

            UpdateDefaultSamplerStates();

            try
            {
                if (RootPass == null)
                    return;

                dynamicDrawables.Clear();
                BoundingFrustum viewFrustum = ViewFrustum;
                scene.FindAll(ref viewFrustum, dynamicDrawables);

                // Notify each drawable when the frame begins
                for (int currentDrawable = 0; currentDrawable < dynamicDrawables.Count; currentDrawable++)
                    // TODO: Visibility filter
                    dynamicDrawables[currentDrawable].BeginDraw(this);

                UpdatePasses();

                activePasses.Clear();
                RootPass.GetActivePasses(activePasses);

                targetPasses.Resize(activePasses.Count);
                preferedFormats.Resize(activePasses.Count);

                // Determines which pass should be rendered to a texture.
                int lastPass = 0;
                IPostEffect lastPostEffect = null;
                for (int i = activePasses.Count - 1; i >= 0; i--)
                {
                    var postEffect = activePasses[i] as IPostEffect;
                    if (lastPostEffect != null && (postEffect != null || i == 0))
                    {
                        targetPasses[i] = lastPostEffect;
                        preferedFormats[i] = lastPostEffect.InputFormat;
                    }
                    else
                    {
                        targetPasses[i] = null;
                        preferedFormats[i] = null;
                    }

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
                    Matrix passView, passProjection;
                    if (pass.TryGetViewFrustum(out passView, out passProjection))
                    {
                        View = passView;
                        Projection = passProjection;
                        overrideViewFrustum = true;
                    }

                    if (overrideViewFrustum || overrideViewFrustumLastPass)
                    {
                        // Notify drawables when view frustum has changed
                        for (int currentDrawable = 0; currentDrawable < dynamicDrawables.Count; currentDrawable++)
                            dynamicDrawables[currentDrawable].EndDraw(this);

                        dynamicDrawables.Clear();
                        BoundingFrustum frustum = matrices.ViewFrustum;
                        Scene.FindAll(ref frustum, dynamicDrawables);
                        overrideViewFrustumLastPass = overrideViewFrustum;

                        for (int currentDrawable = 0; currentDrawable < dynamicDrawables.Count; currentDrawable++)
                            dynamicDrawables[currentDrawable].EndDraw(this);
                    }

                    try
                    {
                        if ((targetPass != null || i == activePasses.Count - 1) && intermediate != null)
                        {
                            intermediate.End();
                            lastRenderTarget = intermediate;
                        }

                        if (targetPass != null)
                        {
                            intermediate = pass.PrepareRenderTarget(this, intermediate, preferedFormats[i]);
                            intermediate.Begin();
                            RenderTargetPool.Lock(intermediate);
                        }

                        var postEffect = pass as IPostEffect;
                        if (postEffect != null)
                            postEffect.InputTexture = lastRenderTarget;

                        // Clear the screen when we are drawing to the backbuffer.
                        //  --> Or when we are drawing to the main pass ???
                        if (i == lastPass)
                            GraphicsDevice.Clear(Settings.BackgroundColor);

                        pass.Draw(this, dynamicDrawables);
                    }
                    finally
                    {
                        RenderTargetPool.Unlock(lastRenderTarget);
                    }
                }
            }
            finally
            {
                CurrentFrame++;
                isDrawing = false;

                // Notify each drawable when the frame begins
                for (int currentDrawable = 0; currentDrawable < dynamicDrawables.Count; currentDrawable++)
                    dynamicDrawables[currentDrawable].BeginDraw(this);
            }
        }

        /// <summary>
        /// Resets all texture sampler states to the default states specified in settings.
        /// </summary>
        private void UpdateDefaultSamplerStates()
        {
            if (settings.DefaultSamplerStateChanged)
            {
                var samplerState = settings.DefaultSamplerState;
                for (int i = 0; i < 16; i++)
                    GraphicsDevice.SamplerStates[i] = samplerState;
                settings.DefaultSamplerStateChanged = false;
            }
        }

        /// <summary>
        /// Make sure all the dependent passes for each material is valid.
        /// </summary>
        private void UpdatePasses()
        {
            try
            {
                for (int currentDrawable = 0; currentDrawable < dynamicDrawables.Count; currentDrawable++)
                {
                    var material = dynamicDrawables[currentDrawable].Material;
                    if (material != null)
                        material.GetDependentPasses(dependentPassTypes);
                }

                var passes = RootPass.Passes;
                var count = RootPass.Passes.Count;
                for (int i = 0; i < count; i++)
                {
                    var pass = passes[i];
                    if (pass.Enabled)
                        pass.GetDependentPasses(dependentPassTypes);
                }

                foreach (var pass in dependentPassMapping.Values)
                {
                    pass.Enabled = false;
                }

                for (int i = 0; i < dependentPassTypes.Count; i++)
                {
                    Pass pass;
                    var passType = dependentPassTypes[i];
                    if (dependentPassMapping.TryGetValue(passType, out pass))
                    {
                        pass.Enabled = true;
                        continue;
                    }
                    dependentPassMapping.Add(passType, pass = CreatePass(passType));
                    RootPass.Passes.Add(pass);
                    MainPass.AddDependency(pass);
                }
            }
            finally
            {
                dependentPassTypes.Clear();
            }
        }

        /// <summary>
        /// Creates a pass from the pass type.
        /// </summary>
        private Pass CreatePass(Type passType)
        {
            var defaultConstructor = passType.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor != null)
                return (Pass)defaultConstructor.Invoke(null);
            return (Pass)Activator.CreateInstance(passType, new object[] { GraphicsDevice });
        }
        #endregion
    }
}