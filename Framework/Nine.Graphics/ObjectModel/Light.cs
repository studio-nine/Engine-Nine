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
using Nine.Graphics.Effects;
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
        public BoundingFrustum ShadowFrustum
        {
            get { return shadowFrustum; } 
        }
        private BoundingFrustum shadowFrustum;

        /// <summary>
        /// Used by the rendering system to keep track of drawables affect by this light.
        /// </summary>
        internal List<IDrawableObject> AffectedDrawables;
        internal List<IDrawableObject> AffectedShadowCasters;

#if !WINDOWS_PHONE
        internal ShadowMap ShadowMap;
#endif

        /// <summary>
        /// For now we don't allow custom light types.
        /// </summary>
        internal Light()
        {
            Enabled = true;
            shadowFrustum = new BoundingFrustum(new Matrix());
        }

        /// <summary>
        /// Finds all the objects affected by this light.
        /// </summary>
        protected internal virtual void FindAll(Scene scene, List<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            for (var i = 0; i < drawablesInViewFrustum.Count; i++)
                result.Add(drawablesInViewFrustum[i]);
        }

        /// <summary>
        /// TODO:
        /// </summary>
        protected internal abstract bool Apply(Material material, int index, bool last);

        /// <summary>
        /// Draws the depth map of the specified drawables.
        /// </summary>
        internal void DrawShadowMap(GraphicsContext context, Scene scene,
                                    HashSet<ISpatialQueryable> shadowCastersInLightFrustum,
                                    HashSet<ISpatialQueryable> shadowCastersInViewFrustum)
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

            Matrix shadowFrustumMatrix = new Matrix();
            GetShadowFrustum(context, shadowCastersInLightFrustum, shadowCastersInViewFrustum, out shadowFrustumMatrix);
            ShadowFrustum.Matrix = shadowFrustumMatrix;

            ShadowMap.Begin();
            {
                context.View = Matrix.Identity;
                context.Projection = shadowFrustumMatrix;
                context.Begin(BlendState.Opaque, null, DepthStencilState.Default, null);
                {
                    DepthEffect depthEffect = (DepthEffect)ShadowMap.Effect;
                    if (AffectedShadowCasters == null)
                        AffectedShadowCasters = new List<IDrawableObject>();
                    else
                        AffectedShadowCasters.Clear();
                    scene.FindAll(ref shadowFrustum, AffectedShadowCasters);
                    
                    for (int currentShadowCaster = 0; currentShadowCaster < AffectedShadowCasters.Count; currentShadowCaster++)
                    {
                        var shadowCaster = AffectedShadowCasters[currentShadowCaster];
                        if (Scene.CastShadow(shadowCaster))
                        {
                            depthEffect.TextureEnabled = shadowCaster.Material != null && shadowCaster.Material.DepthAlphaEnabled;
                            shadowCaster.Draw(context, depthEffect);
                        }
                    }
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
                                                HashSet<ISpatialQueryable> drawablesInLightFrustum,
                                                HashSet<ISpatialQueryable> drawablesInViewFrustum,
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
        protected internal sealed override bool Apply(Material material, int index, bool last)
        {
            IEffectLights<T> lightables = material.Find<IEffectLights<T>>();
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