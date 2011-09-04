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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ParticleEffects;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines a base class for a light used by the render system.
    /// </summary>
    public abstract class Light : Transformable
    {
        /// <summary>
        /// Gets whether the light is enabled.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets whether the light should cast a shadow.
        /// </summary>
        public bool CastShadow  { get; set; }

        /// <summary>
        /// Gets the order of this light when it's been process by the renderer.
        /// Light might be discarded when the max affecting lights are reached.
        /// </summary>
        public float Order { get; set; }

        /// <summary>
        /// Gets the multi-pass lighting effect used to draw object.
        /// </summary>
        public virtual Effect MultiPassEffect { get { return null; } }

        /// <summary>
        /// Gets the shadow frustum of this light.
        /// </summary>
        public BoundingFrustum ShadowFrustum { get; private set; }

        /// <summary>
        /// Used by the rendering system to keep track of drawables affect by this light.
        /// </summary>
        internal List<IDrawableObject> AffectedDrawables;
        internal List<ISpatialQueryable> AffectedBoundables;

#if !WINDOWS_PHONE
        internal ShadowMap ShadowMap;
#endif

        /// <summary>
        /// For now we don't allow custom light types.
        /// </summary>
        internal Light()
        {
            Enabled = true;
            ShadowFrustum = new BoundingFrustum(new Matrix());
        }

        /// <summary>
        /// Finds all the objects affected by this light.
        /// </summary>
        protected internal abstract IEnumerable<ISpatialQueryable> Find(ISpatialQuery<ISpatialQueryable> allObjects, IEnumerable<ISpatialQueryable> objectsInViewFrustum);

        /// <summary>
        /// TODO:
        /// </summary>
        protected internal abstract bool Apply(IEffectInstance effectInstance, int index, bool last);

        /// <summary>
        /// Draws the depth map of the specified drawables.
        /// </summary>
        public virtual void DrawShadowMap(GraphicsContext context,
                                          ISpatialQuery<IDrawableObject> drawables, 
                                          IEnumerable<ISpatialQueryable> objectsInLightFrustum,
                                          IEnumerable<ISpatialQueryable> objectsInViewFrustum)
        {
#if !WINDOWS_PHONE
            if (ShadowMap == null || ShadowMap.Size != context.Settings.ShadowMapResolution)
            {
                if (ShadowMap != null)
                    ShadowMap.Dispose();
                ShadowMap = new ShadowMap(context.GraphicsDevice, context.Settings.ShadowMapResolution);
            }

            Matrix view = context.View;
            Matrix projection = context.Projection;

            Matrix shadowFrustum = new Matrix();
            GetShadowFrustum(context, objectsInLightFrustum, objectsInViewFrustum, out shadowFrustum);
            ShadowFrustum.Matrix = shadowFrustum;

            ShadowMap.Begin();
            {
                context.View = Matrix.Identity;
                context.Projection = shadowFrustum;
                context.Begin(BlendState.Opaque, null, DepthStencilState.Default, null);
                {
                    drawables.FindAll(ShadowFrustum).ForEach(d => d.Draw(context, ShadowMap.Effect));
                }
                context.End();
                context.View = view;
                context.Projection = projection;
            }
            ShadowMap.End();
#endif
        }

        /// <summary>
        /// Gets the shadow frustum of this light.
        /// </summary>
        protected virtual void GetShadowFrustum(GraphicsContext context,
                                                IEnumerable<ISpatialQueryable> drawablesInLightFrustum,
                                                IEnumerable<ISpatialQueryable> drawablesInViewFrustum,
                                                out Matrix frustumMatrix)
        {
            frustumMatrix = context.ViewFrustum.Matrix;
        }

        /// <summary>
        /// Draws the light frustum using Settings.Debug.LightFrustumColor.
        /// </summary>
        public virtual void DrawFrustum(GraphicsContext context) { }
    }

    /// <summary>
    /// Base class for all lights.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class Light<T> : Light where T : class
    {
        protected internal sealed override bool Apply(IEffectInstance effectInstance, int index, bool last)
        {
            IEffectLights<T> lightables = effectInstance.As<IEffectLights<T>>();
            if (lightables == null || lightables.Lights == null || index >= lightables.Lights.Count)
                return false;
            Enable(lightables.Lights[index]);
            if (last)
            {
                for (int i = index + 1; i < lightables.Lights.Count; i++)
                    Disable(lightables.Lights[i]);
            }
            return true;
        }

        protected abstract void Enable(T light);
        protected abstract void Disable(T light);
    }
}