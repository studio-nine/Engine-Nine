namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;

    /// <summary>
    /// Defines a base class for a light used by the render system.
    /// </summary>
    [ContentProperty("ShadowMap")]
    public abstract class Light : Transformable, ISceneObject, IDebugDrawable
    {
        #region Properties
        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets whether the light is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        internal bool enabled = true;

        /// <summary>
        /// Gets the order of this light when it's been process by the renderer.
        /// Light might be discarded when the max affecting lights are reached.
        /// </summary>
        public float Order
        {
            get { return order; }
            set { order = value; }
        }
        internal float order;

        /// <summary>
        /// Gets the material usage to draw multi-pass lighting effect, or return 
        /// MaterialUsage.Default to indicate that this light does not support
        /// multi-pass lighting effect.
        /// </summary>
        public virtual MaterialUsage MultiPassMaterial
        {
            get { return MaterialUsage.Default; } 
        }
        #endregion

        #region Shadow
        /// <summary>
        /// Gets or sets whether the light should cast a shadow.
        /// </summary>
        public bool CastShadow
        {
            get { return castShadow; }
            set 
            {
                if (castShadow != value)
                {
                    castShadow = value;
                    if (castShadow && shadowMap == null)
                        ShadowMap = new ShadowMap(GraphicsDevice);
                }
            }
        }
        internal bool castShadow = false;

        /// <summary>
        /// Gets or sets the shadow technique used by this light.
        /// </summary>
        public ShadowMap ShadowMap
        {
            get { return shadowMap; }
            set
            {
                if (shadowMap != value)
                {
                    if (shadowMap != null)
                    {
                        if (shadowMap.Light == null)
                            throw new InvalidOperationException();
                        shadowMap.Light = null;
                        shadowMap.Dispose();
                        if (context != null)
                            context.mainPass.Passes.Remove(shadowMap);
                    }
                    shadowMap = value;
                    if (shadowMap != null)
                    {
                        if (shadowMap.Light != null)
                            throw new InvalidOperationException();
                        if (context != null)
                            context.mainPass.Passes.Insert(0, shadowMap);
                        shadowMap.Light = this;
                    }
                }
            }
        }
        private ShadowMap shadowMap;
        private DrawingContext context;
        private HashSet<ISpatialQueryable> shadowCasters;

        /// <summary>
        /// Gets the shadow frustum of this light.
        /// </summary>
        public BoundingFrustum ShadowFrustum
        {
            get { return shadowFrustum; } 
        }
        private BoundingFrustum shadowFrustum;
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        protected Light(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            GraphicsDevice = graphics;
            shadowFrustum = new BoundingFrustum(new Matrix());
        }

        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        protected virtual void OnAdded(DrawingContext context) { }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        protected virtual void OnRemoved(DrawingContext context) { }

        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        void ISceneObject.OnAdded(DrawingContext context)
        {
            if (this.context != null)
                throw new InvalidOperationException();
            if (castShadow)
            {
                if (shadowMap == null)
                    shadowMap = new ShadowMap(GraphicsDevice);
                context.mainPass.Passes.Insert(0, shadowMap);
            }
            this.context = context;
            OnAdded(context);
        }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void ISceneObject.OnRemoved(DrawingContext context)
        {
            if (this.context == null || context != this.context)
                throw new InvalidOperationException();
            if (shadowMap != null)
            {
                context.mainPass.Passes.Remove(shadowMap);
                shadowMap.Dispose();
                shadowMap = null;
            }
            this.context = null;
            OnRemoved(context);
        }

        /// <summary>
        /// Finds all the objects affected by this light.
        /// </summary>
        public virtual void FindAll(Scene scene, IList<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            for (var i = 0; i < drawablesInViewFrustum.Count; ++i)
                result.Add(drawablesInViewFrustum[i]);
        }

        /// <summary>
        /// Computes the shadow frustum of this light based on the current
        /// view frustum and objects in the current scene;
        /// </summary>
        public void UpdateShadowFrustum(BoundingFrustum viewFrustum, IList<IDrawableObject> drawables)
        {
            // TODO: This value can be calculated once per frame.
            UpdateShadowCasters(drawables);

            Matrix shadowFrustumMatrix;
            UpdateShadowFrustum(viewFrustum, shadowCasters, out shadowFrustumMatrix);
            shadowFrustum.Matrix = shadowFrustumMatrix;
        }
        
        /// <summary>
        /// Computes the shadow frustum of this light based on the current
        /// view frustum and objects in the current scene;
        /// </summary>
        protected abstract void UpdateShadowFrustum(BoundingFrustum viewFrustum, HashSet<ISpatialQueryable> shadowCasters, out Matrix shadowFrustum);

        /// <summary>
        /// Finds all the shadow casters based on drawables.
        /// </summary>
        private void UpdateShadowCasters(IList<IDrawableObject> drawables)
        {
            if (shadowCasters == null)
                shadowCasters = new HashSet<ISpatialQueryable>();

            shadowCasters.Clear();
            for (int currentDrawable = 0; currentDrawable < drawables.Count; currentDrawable++)
            {
                var drawable = drawables[currentDrawable];
                var lightable = drawable as ILightable;
                if (lightable != null && lightable.CastShadow)
                {
                    var parentSpatialQueryable = ContainerTraverser.FindParentContainer<ISpatialQueryable>(drawable);
                    if (parentSpatialQueryable != null)
                        shadowCasters.Add(parentSpatialQueryable);
                }
            }
        }
        
        bool IDebugDrawable.Visible
        {
            get { return enabled; }
        }
        /// <summary>
        /// Draws the light frustum.
        /// </summary>
        public virtual void Draw(DrawingContext context, DynamicPrimitive primitive)
        {
            primitive.AddFrustum(ShadowFrustum, null, Constants.ShadowFrustumColor);
        }
        #endregion
    }
}