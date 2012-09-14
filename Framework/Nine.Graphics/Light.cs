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
#if !WINDOWS_PHONE
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

        /// <summary>
        /// Gets the shadow frustum of this light.
        /// </summary>
        public BoundingFrustum ShadowFrustum
        {
            get { return shadowFrustum; } 
        }
        internal BoundingFrustum shadowFrustum = new BoundingFrustum(new Matrix());
#endif
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
#if !WINDOWS_PHONE
            if (this.context != null)
                throw new InvalidOperationException();
            if (castShadow)
            {
                if (shadowMap == null)
                    shadowMap = new ShadowMap(GraphicsDevice);
                context.mainPass.Passes.Insert(0, shadowMap);
            }
            this.context = context;
#endif
            OnAdded(context);
        }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void ISceneObject.OnRemoved(DrawingContext context)
        {
#if !WINDOWS_PHONE
            if (this.context == null || context != this.context)
                throw new InvalidOperationException();
            if (shadowMap != null)
            {
                context.mainPass.Passes.Remove(shadowMap);
                shadowMap.Dispose();
                shadowMap = null;
            }
            this.context = null;
#endif
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

#if !WINDOWS_PHONE
        /// <summary>
        /// Computes the shadow frustum of this light based on the current
        /// view frustum and objects in the current view frustum;
        /// </summary>
        public virtual void UpdateShadowFrustum(DrawingContext context, ISpatialQuery<IDrawableObject> shadowCasterQuery)
        {

        }
#endif

        bool IDebugDrawable.Visible
        {
            get { return enabled; }
        }

        /// <summary>
        /// Draws the light frustum.
        /// </summary>
        public virtual void Draw(DrawingContext context, DynamicPrimitive primitive)
        {
#if !WINDOWS_PHONE
            primitive.AddFrustum(ShadowFrustum, null, Constants.ShadowFrustumColor, Constants.TinyLineWidth);
#endif
        }
        #endregion
    }
}