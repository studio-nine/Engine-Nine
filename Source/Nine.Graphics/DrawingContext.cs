namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// A drawing context contains commonly used global parameters for rendering.
    /// </summary>
    public class DrawingContext : ISpatialQuery, IDisposable
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
        /// Gets or sets the color of the background.
        /// </summary>
        public Color BackgroundColor { get; set; }

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
        /// Gets the elapsed time since last update.
        /// </summary>
        public float ElapsedTime
        {
            get { return elapsedTime; }
        }
        internal float elapsedTime;

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
        public Pass MainPass
        {
            get { return mainPass; }
        }
        internal Pass mainPass;

        /// <summary>
        /// Gets the passes that is used to render the scene.
        /// </summary>
        public IList<Pass> Passes
        {
            get { return rootPass.Passes; }
        }
        internal PassGroup rootPass;

        #endregion

        #region SamplerState
        /// <summary>
        /// Gets or sets the texture filter quality for this drawing pass.
        /// </summary>
        public TextureFilter TextureFilter
        {
            get { return textureFilter; }
            set
            {
                if (textureFilter != value)
                {
                    textureFilter = value;
                    samplerStateNeedsUpdate = true;
                }
            }
        }
        private TextureFilter textureFilter = TextureFilter.Linear;

        /// <summary>
        /// Gets or sets the maximum anisotropy. The default value is 4.
        /// </summary>
        public int MaxAnisotropy
        {
            get { return maxAnisotropy; }
            set
            {
                if (maxAnisotropy != value)
                {
                    maxAnisotropy = value;
                    samplerStateNeedsUpdate = true;
                }
            }
        }
        private int maxAnisotropy = 4;
        private bool samplerStateNeedsUpdate = false;
        private SamplerState samplerState = SamplerState.LinearWrap;

        /// <summary>
        /// Gets the default sampler state.
        /// </summary>
        public SamplerState SamplerState
        {
            get
            {
                if (samplerStateNeedsUpdate)
                {
                    samplerStateNeedsUpdate = false;
                    if (maxAnisotropy == 4)
                    {
                        if (textureFilter == TextureFilter.Linear)
                            samplerState = SamplerState.LinearWrap;
                        else if (textureFilter == TextureFilter.Point)
                            samplerState = SamplerState.PointWrap;
                        else if (textureFilter == TextureFilter.Anisotropic)
                            samplerState = SamplerState.AnisotropicWrap;
                        else
                            samplerStateNeedsUpdate = true;
                    }
                    else
                    {
                        samplerStateNeedsUpdate = true;
                    }

                    if (samplerStateNeedsUpdate)
                    {
                        samplerState = new SamplerState();
                        samplerState.AddressU = TextureAddressMode.Wrap;
                        samplerState.AddressV = TextureAddressMode.Wrap;
                        samplerState.Filter = textureFilter;
                        samplerState.MaxAnisotropy = maxAnisotropy;
                        samplerStateNeedsUpdate = false;
                    }
                }
                return samplerState;
            }
        }
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

        #region Fields
        private bool isDrawing = false;
        private ISpatialQuery spatialQuery;
        private ISpatialQuery<IDrawableObject> drawables;
        private ISpatialQuery<IDebugDrawable> debugDrawables;
        private ISpatialQuery<ISpatialQueryable> debugBounds;
        private DynamicPrimitive debugPrimitive;
        private FastList<ISpatialQueryable> debugBoundsInViewFrustum;
        private FastList<IDebugDrawable> debugDrawablesInViewFrustum;
        private FastList<Pass> activePasses = new FastList<Pass>();        
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

            this.graphics = graphics;
            this.spatialQuery = spatialQuery;
            this.BackgroundColor = new Color(95, 120, 157);
            this.drawables = spatialQuery.CreateSpatialQuery<IDrawableObject>(drawable => drawable.OnAddedToView(this)); 

            this.matrices = new MatrixCollection();
            this.textures = new TextureCollection();
            this.rootPass = new PassGroup();
            this.rootPass.Passes.Add(new SpritePass());
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
        public void Draw(float elapsedTime)
        {
            var activeCamera = camera;
            Matrix view, projection;
            activeCamera.TryGetViewFrustum(out view, out projection);
            Draw(elapsedTime, view, projection);
        }

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public void Draw(float elapsedTime, Matrix view, Matrix projection)
        {
            if (isDrawing)
                throw new InvalidOperationException("Cannot trigger another drawing of the scene while it's still been drawn");

            this.matrices.View = view;
            this.matrices.Projection = projection;
            this.isDrawing = true;
            this.VertexOffset = 0;
            this.VertexBuffer = null;            
            this.PreviousMaterial = null;
            this.elapsedTime = elapsedTime;
            this.totalTime += TimeSpan.FromSeconds(elapsedTime);
            this.totalSeconds = (float)totalTime.TotalSeconds;
            this.boundingBoxNeedsUpdate = true;

            if (rootPass == null)
                return;

            graphics.SetVertexBuffer(null);
            graphics.Textures[0] = null;

            UpdateDefaultSamplerStates();

            AddDrawablesToView(matrices.ViewFrustum);

            UpdatePassGraph();
            UpdateActivePasses();

            RenderTarget2D lastRenderTarget = null;
            RenderTarget2D intermediate = null;
            bool overrideViewFrustumLastPass = false;

            for (int i = 0; i < activePasses.Count; ++i)
            {
                var pass = activePasses[i];
                var overrideViewFrustum = false;

                // Query the drawables in the current view frustum only when the view frustum changed
                // or the pass overrides the frustum.
                Matrix passView, passProjection;
                if (pass.TryGetViewFrustum(out passView, out passProjection))
                {
                    matrices.View = passView;
                    matrices.Projection = passProjection;
                    overrideViewFrustum = true;
                }

                if (overrideViewFrustum || overrideViewFrustumLastPass)
                {
                    AddDrawablesToView(matrices.ViewFrustum);
                    overrideViewFrustumLastPass = overrideViewFrustum;
                }
                
                if ((pass.PassOperation & PassOperation.EndRenderTarget) != 0)
                {
                    intermediate.End();
                    lastRenderTarget = intermediate;
                }

                if ((pass.PassOperation & PassOperation.BeginRenderTarget) != 0)
                {
                    intermediate = pass.PrepareRenderTarget(graphics, intermediate, pass.PassFormat);
                    intermediate.Begin();
                    RenderTargetPool.Lock(intermediate);
                }
                
                var postEffect = pass as IPostEffect;
                if (postEffect != null)
                {
                    postEffect.InputTexture = lastRenderTarget;
                    pass.Draw(this, drawablesInViewFrustum);
                    RenderTargetPool.Unlock(lastRenderTarget);
                }
                else
                {
                    pass.Draw(this, drawablesInViewFrustum);
                }
            }

            currentFrame++;
            isDrawing = false;
        }

        /// <summary>
        /// Finds and adds all the drawables in the view frustum.
        /// </summary>
        private void AddDrawablesToView(BoundingFrustum viewFrustum)
        {
            drawablesInViewFrustum.Clear();
            drawables.FindAll(viewFrustum, drawablesInViewFrustum);
        }

        /// <summary>
        /// Resets all texture sampler states to the default states specified in settings.
        /// </summary>
        private void UpdateDefaultSamplerStates()
        {
            var samplerState = SamplerState;
            for (int i = 0; i < 16; ++i)
                graphics.SamplerStates[i] = samplerState;
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
                    pass.GetDependentPassTypes(dependentPassTypes);
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
#if WINRT
            var defaultConstructor = passType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
#else
            var defaultConstructor = passType.GetConstructor(Type.EmptyTypes);
#endif
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

            IPostEffect lastPostEffect = null;

            for (int i = activePasses.Count - 1; i >= 0; i--)
            {
                var pass = activePasses[i];
                var postEffect = pass as IPostEffect;

                pass.PassOperation = PassOperation.None;
                pass.PassFormat = null;

                if (lastPostEffect != null)
                {
                    if (postEffect != null || i == 0)
                    {
                        pass.PassOperation |= PassOperation.BeginRenderTarget;
                        pass.PassFormat = lastPostEffect.InputFormat;
                    }
                }

                if (postEffect != null)
                {
                    pass.PassOperation |= PassOperation.EndRenderTarget;
                    lastPostEffect = postEffect;
                }
            }
        }

        /// <summary>
        /// Draws the debug overlay of the target scene.
        /// </summary>
        internal void DrawDiagnostics()
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

            AddDiagnostics(debugPrimitive, debugBoundsInViewFrustum);

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

        protected virtual void AddDiagnostics(DynamicPrimitive dynamicPrimitive, IList<ISpatialQueryable> boundsInView) { }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (samplerState != null)
                    samplerState.Dispose();
            }
        }
        #endregion
    }
}