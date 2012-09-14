namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    using Nine.Graphics.Cameras;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using DirectionalLight = Nine.Graphics.DirectionalLight;

    /// <summary>
    /// A drawing context contains commonly used global parameters for rendering.
    /// </summary>
    public class DrawingContext : ISpatialQuery
    {
        #region Properties
        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphics; }
        }
        internal GraphicsDevice graphics;

        /// <summary>
        /// Gets or sets the active camera of the current drawing frustum.
        /// </summary>
        /// <remarks>
        /// This camera will be initialized to first camera found in the scene.
        /// If no cameras are found, then a default free camera is used.
        /// </remarks>
        public ICamera Camera
        {
            get { return camera; }
            set { camera = value; }
        }
        internal ICamera camera;
        
        /// <summary>
        /// Gets the graphics settings
        /// </summary>
        public Settings Settings
        {
            get { return settings; }
            set { settings = (value ?? new Settings()); }
        }
        internal Settings settings;

        /// <summary>
        /// Gets the graphics statistics.
        /// </summary>
        public Statistics Statistics 
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the elapsed time since last update.
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return elapsedTime; }
        }
        internal TimeSpan elapsedTime;
        internal float elapsedSeconds;

        /// <summary>
        /// Gets the total seconds since the beginning of the drawing context.
        /// </summary>
        public TimeSpan TotalTime
        {
            get { return totalTime; }
        }
        internal TimeSpan totalTime;
        internal float totalSeconds;

        /// <summary>
        /// Gets all the visible drawables used by this drawing context.
        /// </summary>
        public ISpatialQuery<IDrawableObject> Drawables
        {
            get { return drawables; }
        }
        internal ISpatialQuery<IDrawableObject> drawables;

        /// <summary>
        /// Gets the bounding box of the current drawing context.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { UpdateBounds(); return boundingBox; }
        }
        private BoundingBox boundingBox;
        private ISpatialQuery<ISpatialQueryable> boundsQuery;
        private bool boundingBoxNeedsUpdate = true;

        /// <summary>
        /// Gets the number of elapsed frames since the beginning of the draw context.
        /// </summary>
        public int CurrentFrame
        {
            get { return currentFrame; }
        }
        internal int currentFrame;

        /// <summary>
        /// Gets a collection of global textures.
        /// </summary>
        public TextureCollection Textures
        {
            get { return textures; }
        }
        internal TextureCollection textures;

        /// <summary>
        /// Gets the main pass that is used to render the scene.
        /// </summary>
        public PassGroup MainPass
        {
            get { return mainPass; }
        }
        internal PassGroup mainPass;

        /// <summary>
        /// Gets the root pass of this drawing context composition chain.
        /// </summary>
        public PassGroup RootPass
        {
            get { return rootPass; }
        }
        internal PassGroup rootPass;
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
        /// Gets the current camera position.
        /// </summary>
        public Vector3 CameraPosition
        {
            get { return matrices.cameraPosition; }
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
            set { ambientLightColor = value; }
        }
        internal Vector3 ambientLightColor;

        /// <summary>
        /// Gets a global sorted collection of directional lights of this <see cref="DrawingContext"/>.
        /// </summary>
        public DirectionalLightCollection DirectionalLights
        {
            get { return directionalLights; }
        }
        internal DirectionalLightCollection directionalLights;

        /// <summary>
        /// Gets the default or main directional light of this <see cref="DrawingContext"/>.
        /// </summary>
        public DirectionalLight DirectionalLight
        {
            get { return directionalLights[0] ?? defaultLight; }
        }
        private DirectionalLight defaultLight;
        #endregion

        #region Fog
        /// <summary>
        /// Gets the global fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }
        internal Vector3 fogColor = Constants.FogColor;

        /// <summary>
        /// Gets or sets the fog end.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }
            set { fogEnd = value; }
        }
        internal float fogEnd = Constants.FogEnd;

        /// <summary>
        /// Gets or sets the fog start.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }
            set { fogStart = value; }
        }
        internal float fogStart = Constants.FogStart;
        #endregion

        #region Fields
        private bool isDrawing = false;
        private ISpatialQuery spatialQuery;
        private ISpatialQuery<IDebugDrawable> debugDrawables;
        private ISpatialQuery<ISpatialQueryable> debugBounds;
        private DynamicPrimitive debugPrimitive;
        private FastList<ISpatialQueryable> debugBoundsInViewFrustum;
        private FastList<IDebugDrawable> debugDrawablesInViewFrustum;
        private FastList<Pass> activePasses = new FastList<Pass>();
        private FastList<IPostEffect> targetPasses = new FastList<IPostEffect>();
        private FastList<SurfaceFormat?> preferedFormats = new FastList<SurfaceFormat?>();
        private FastList<IDrawableObject> drawablesInViewFrustum = new FastList<IDrawableObject>();
        private HashSet<Type> dependentPassTypes = new HashSet<Type>();
        private Dictionary<Type, Pass> dependentPassMapping = new Dictionary<Type, Pass>();
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        public DrawingContext(GraphicsDevice graphics) : this(graphics, new SpatialQuery())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        public DrawingContext(GraphicsDevice graphics, IEnumerable objects) : this(graphics, new SpatialQuery(objects))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        public DrawingContext(GraphicsDevice graphics, ISpatialQuery spatialQuery)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (spatialQuery == null)
                throw new ArgumentNullException("spatialQuery");

            this.spatialQuery = spatialQuery;
            this.drawables = spatialQuery.CreateSpatialQuery<IDrawableObject>(drawable => drawable.Visible);
            this.settings = new Settings();
            this.graphics = graphics;
            this.defaultLight = new DirectionalLight(graphics)
            {
                DiffuseColor = Vector3.Zero,
                SpecularColor = Vector3.Zero,
                Direction = Vector3.Down,
                Enabled = false
            };
            this.directionalLights = new DirectionalLightCollection(defaultLight);
            this.matrices = new MatrixCollection();
            this.textures = new TextureCollection();
            this.mainPass = new PassGroup() { name = "MainPass" };
            this.mainPass.Passes.Add(new DrawingPass() { ClearBackground = true, TransparencySortEnabled = true });
            this.mainPass.Passes.Add(new SpritePass());
            this.rootPass = new PassGroup() { name = "RootPass" };
            this.rootPass.Passes.Add(mainPass);
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
        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            if (VertexBuffer != vertexBuffer || VertexOffset != vertexOffset)
            {
                graphics.SetVertexBuffer(vertexBuffer, vertexOffset);
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
        /// Gets the half pixel size of the current viewport.
        /// </summary>
        internal Vector2 HalfPixel
        {
            get
            {
                var vp = graphics.Viewport;
                return new Vector2(0.5f / vp.Width, 0.5f / vp.Height);
            }
        }
        
        /// <summary>
        /// Create a spatial query of the specified type from this scene.
        /// </summary>
        public ISpatialQuery<T> CreateSpatialQuery<T>(Predicate<T> condition) where T : class
        {
            return spatialQuery.CreateSpatialQuery<T>(condition);
        }

        /// <summary>
        /// Updates the axis aligned bounding box that exactly contains the bounds of all objects.
        /// </summary>
        private void UpdateBounds()
        {
            if (boundingBoxNeedsUpdate)
            {
                var maxBounds = 1E6F;
                var hasBoundable = false;                
                var queryBounds = new BoundingBox(-Vector3.One * maxBounds, Vector3.One * maxBounds);
                if (boundsQuery == null)
                    boundsQuery = spatialQuery.CreateSpatialQuery<ISpatialQueryable>();
                
                boundingBox.Min = Vector3.Zero;
                boundingBox.Max = Vector3.Zero;
                boundsQuery.FindAll(ref queryBounds, boundable =>
                {
                    if (hasBoundable)
                    {
                        var childBounds = boundable.BoundingBox;
                        BoundingBox.CreateMerged(ref boundingBox, ref childBounds, out boundingBox);
                    }
                    else
                    {
                        boundingBox = boundable.BoundingBox;
                        hasBoundable = true;
                    }
                });
                boundingBoxNeedsUpdate = false;
            }
        }
        
        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public void Draw(TimeSpan elapsedTime)
        {
            var activeCamera = camera;
            Matrix view, projection;
            Viewport? viewport;
            activeCamera.TryGetViewFrustum(out view, out projection, out viewport);
            Draw(elapsedTime, view, projection);
        }

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, Matrix view, Matrix projection)
        {
            if (isDrawing)
                throw new InvalidOperationException("Cannot trigger another drawing of the scene while it's still been drawn");

            this.View = view;
            this.Projection = projection;
            this.isDrawing = true;
            this.VertexOffset = 0;
            this.VertexBuffer = null;            
            this.PreviousMaterial = null;
            this.elapsedTime = elapsedTime;
            this.totalTime += elapsedTime;
            this.elapsedSeconds = (float)elapsedTime.TotalSeconds;
            this.totalSeconds = (float)totalTime.TotalSeconds;
            this.boundingBoxNeedsUpdate = true;

            if (rootPass == null)
                return;

            graphics.SetVertexBuffer(null);
            UpdateDefaultSamplerStates();

            drawablesInViewFrustum.Clear();
            BoundingFrustum viewFrustum = ViewFrustum;
            drawables.FindAll(viewFrustum, drawablesInViewFrustum);

            // Notify each drawable when the frame begins
            for (int currentDrawable = 0; currentDrawable < drawablesInViewFrustum.Count; currentDrawable++)
                drawablesInViewFrustum[currentDrawable].OnAddedToView(this);

            UpdatePassGraph();
            UpdateActivePasses();

            RenderTarget2D lastRenderTarget = null;
            RenderTarget2D intermediate = null;
            bool overrideViewFrustumLastPass = false;

            for (int i = 0; i < activePasses.Count; ++i)
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
                    drawablesInViewFrustum.Clear();
                    BoundingFrustum frustum = matrices.ViewFrustum;
                    drawables.FindAll(frustum, drawablesInViewFrustum);
                    overrideViewFrustumLastPass = overrideViewFrustum;
                }

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

                pass.Draw(this, drawablesInViewFrustum);

                RenderTargetPool.Unlock(lastRenderTarget);
            }

            currentFrame++;
            isDrawing = false;
        }

        /// <summary>
        /// Resets all texture sampler states to the default states specified in settings.
        /// </summary>
        private void UpdateDefaultSamplerStates()
        {
            if (settings.DefaultSamplerStateChanged)
            {
                var samplerState = settings.SamplerState;
                for (int i = 0; i < 16; ++i)
                    graphics.SamplerStates[i] = samplerState;
                settings.DefaultSamplerStateChanged = false;
            }
        }

        /// <summary>
        /// Builds a dependency pass group from materials.
        /// </summary>
        private void UpdatePassGraph()
        {
            for (int currentDrawable = 0; currentDrawable < drawablesInViewFrustum.Count; currentDrawable++)
            {
                var material = drawablesInViewFrustum[currentDrawable].Material;
                if (material != null)
                    material.GetDependentPasses(dependentPassTypes);
            }

            var passes = rootPass.Passes;
            var count = rootPass.Passes.Count;
            for (int i = 0; i < count; ++i)
            {
                var pass = passes[i];
                if (pass.Enabled)
                    pass.GetDependentPasses(dependentPassTypes);
            }

            foreach (var pass in dependentPassMapping.Values)
            {
                pass.Enabled = false;
            }

            foreach (var passType in dependentPassTypes)
            {
                Pass pass;
                if (dependentPassMapping.TryGetValue(passType, out pass))
                {
                    pass.Enabled = true;
                    continue;
                }
                dependentPassMapping.Add(passType, pass = CreatePass(passType));
                rootPass.Passes.Add(pass);
                mainPass.AddDependency(pass);
            }

            dependentPassTypes.Clear();
        }

        /// <summary>
        /// Creates a pass from the pass type.
        /// </summary>
        private Pass CreatePass(Type passType)
        {
            var defaultConstructor = passType.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor != null)
                return (Pass)defaultConstructor.Invoke(null);
            return (Pass)Activator.CreateInstance(passType, new object[] { graphics });
        }

        /// <summary>
        /// Builds an array of active passes from the current pass graph.
        /// </summary>
        private void UpdateActivePasses()
        {
            activePasses.Clear();
            rootPass.GetActivePasses(activePasses);

            targetPasses.Resize(activePasses.Count);
            preferedFormats.Resize(activePasses.Count);

            // Determines which pass should be rendered to a texture.
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
                    lastPostEffect = postEffect;
            }
        }

        /// <summary>
        /// Draws the debug overlay of the target scene.
        /// </summary>
        internal void DrawDebugOverlay()
        {
            if (debugDrawables == null)
            {
                debugDrawables = CreateSpatialQuery<IDebugDrawable>(drawable => drawable.Visible);
                debugBounds = CreateSpatialQuery<ISpatialQueryable>(queryable => true);
                debugPrimitive = new DynamicPrimitive(graphics) { DepthBias = 0.0002f };
                debugDrawablesInViewFrustum = new FastList<IDebugDrawable>();
                debugBoundsInViewFrustum = new FastList<ISpatialQueryable>();                
            }
            
            BoundingFrustum viewFrustum = ViewFrustum;
            debugDrawables.FindAll(viewFrustum, debugDrawablesInViewFrustum);
            debugBounds.FindAll(viewFrustum, debugBoundsInViewFrustum);

            debugPrimitive.AddBox(BoundingBox, null, Constants.SceneBoundsColor, 4);

            for (int i = 0; i < debugBoundsInViewFrustum.Count; ++i)
            {
                debugPrimitive.AddBox(debugBoundsInViewFrustum[i].BoundingBox, null, Constants.BoundingBoxColor, Constants.MiddleLineWidth);
            }
            
            for (int i = 0; i < debugDrawablesInViewFrustum.Count; ++i)
            {
                debugDrawablesInViewFrustum[i].Draw(this, debugPrimitive);
            }

            graphics.DepthStencilState = DepthStencilState.Default;
            debugPrimitive.Draw(this, null);
            debugPrimitive.Clear();
            debugDrawablesInViewFrustum.Clear();
            debugBoundsInViewFrustum.Clear();
        }
        #endregion
    }
}