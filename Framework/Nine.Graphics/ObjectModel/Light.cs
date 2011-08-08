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
        /// Gets whether the light should cast a shadow.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool CastShadow  { get; set; }

        /// <summary>
        /// Gets the order of this light when it's been process by the renderer.
        /// Light might be discarded when the max affecting lights are reached.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public float Order { get; set; }

        /// <summary>
        /// Gets the multi-pass lighting effect used to draw object.
        /// </summary>
        public virtual Effect Effect { get { return null; } }

        /// <summary>
        /// Used by the rendering system to keep track of drawables affect by this light.
        /// </summary>
        internal List<Drawable> AffectedDrawables;

        /// <summary>
        /// Finds all the objects affected by this light.
        /// </summary>
        protected internal abstract IEnumerable<Drawable> FindAffectedDrawables(ISceneManager<Drawable> allDrawables,
                                                                                IEnumerable<Drawable> drawablesInViewFrustum);

        /// <summary>
        /// TODO: Tracky...
        /// </summary>
        protected internal abstract bool Apply(IEffectInstance effectInstance, int index, bool last);

        /// <summary>
        /// Draws the depth map of the specified drawables.
        /// </summary>
        public void DrawDepthMap(ISpatialQuery<Drawable> drawables) { }

        /// <summary>
        /// Draws the ligth frustum using Settings.Debug.LightFrustumColor.
        /// </summary>
        public virtual void DrawFrustum(GraphicsContext context) { }

        public Light()
        {
            Enabled = true;
        }
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