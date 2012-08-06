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

    /// <summary>
    /// Defines a base class for a light used by the render system.
    /// </summary>
    [ContentProperty("Shadow")]
    public abstract class Light : Transformable, ISceneObject
    {
        #region Properties
        /// <summary>
        /// Gets whether the light is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        private bool enabled = true;

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
        /// null to indicate that this light does not support multi-pass lighting effect.
        /// </summary>
        public virtual MaterialUsage? MultiPassMaterial
        {
            get { return null; } 
        }

        /// <summary>
        /// Keeps track of the owner drawing context.
        /// </summary>
        protected DrawingContext Context { get; private set; }
        #endregion

        #region Shadow
        /// <summary>
        /// Gets or sets whether the light should cast a shadow.
        /// </summary>
        public bool CastShadow
        {
            get { return castShadow; }
            set { castShadow = value; }
        }
        private bool castShadow = false;

        /// <summary>
        /// Gets or sets the shadow technique used by this light.
        /// </summary>
        public ShadowMap Shadow
        {
            get { return shadow; }
            set
            {
                if (shadow != value)
                {
                    if (shadow != null)
                    {
                        if (shadow.Light == null)
                            throw new InvalidOperationException();
                        shadow.Light = null;
                        shadow.Dispose();
                        if (Context != null)
                            Context.MainPass.Passes.Remove(shadow);
                    }
                    shadow = value;
                    if (shadow != null)
                    {
                        if (shadow.Light != null)
                            throw new InvalidOperationException();
                        if (Context != null)
                            Context.MainPass.Passes.Insert(0, shadow);
                        shadow.Light = this;
                    }
                }
            }
        }
        private ShadowMap shadow;

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
        protected Light()
        {
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

        void ISceneObject.OnAdded(DrawingContext context)
        {
            if (this.Context != null)
                throw new InvalidOperationException();
            if (shadow == null)
                shadow = new ShadowMap(context.GraphicsDevice);
            context.MainPass.Passes.Insert(0, shadow);
            this.Context = context;
            OnAdded(context);
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {
            if (this.Context == null || context != this.Context)
                throw new InvalidOperationException();
            if (shadow != null)
            {
                context.MainPass.Passes.Remove(shadow);
                shadow.Dispose();
                shadow = null;
            }
            this.Context = null;
            OnRemoved(context);
        }

        /// <summary>
        /// Finds all the objects affected by this light.
        /// </summary>
        public virtual void FindAll(Scene scene, IList<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            for (var i = 0; i < drawablesInViewFrustum.Count; i++)
                result.Add(drawablesInViewFrustum[i]);
        }

        /// <summary>
        /// Gets the shadow frustum of this light.
        /// </summary>
        /// <returns>
        /// Returns true when a shadow caster is found.
        /// </returns>
        public abstract bool GetShadowFrustum(BoundingFrustum viewFrustum, IList<IDrawableObject> drawablesInViewFrustum, out Matrix shadowFrustum);

        /// <summary>
        /// Draws the light frustum using Settings.Debug.LightFrustumColor
        /// </summary>
        public virtual void DrawFrustum(DrawingContext context) { }
        #endregion
    }
}