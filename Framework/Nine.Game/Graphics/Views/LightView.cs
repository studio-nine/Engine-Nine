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
using Nine.Graphics.Passes;
#endregion

namespace Nine.Graphics.Views
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class LightView<T> : ILight where T : class
    {
        [ContentSerializer(Optional=true)]
        public bool Enabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool CastShadow { get; set; }

        [ContentSerializer(Optional = true)]
        public float Order { get; set; }

        public virtual Effect Effect { get { return null; } }

        public LightView()
        {
            Enabled = true;
        }

        public void DrawDepthMap(ISpatialQuery<IDrawableView> drawables)
        {
            throw new NotImplementedException();
        }

        public void Light(ISpatialQuery<IDrawableView> drawables)
        {
#if !WINDOWS_PHONE
            foreach (var lit in FindLitObjects(drawables))
            {
                IMaterial material = lit as IMaterial;
                if (material == null)
                    continue;
                
                T lightable = material.Effect as T;
                if (lightable != null)
                {
                    Light(lightable);
                }
                else
                {
                    IEffectLights<T> lightables = material.Effect as IEffectLights<T>;
                    if (lightable == null || lightables.Lights == null || lightables.Lights.Count <= 0)
                        continue;

                    foreach (T lightable1 in lightables.Lights)
                    {
                        Light(lightable1);
                    }
                }
            }
#endif
        }
        
        /// <summary>
        /// Gets a list of drawables that will potentially be lit by this light.
        /// </summary>
        protected abstract IEnumerable<IDrawableView> FindLitObjects(ISpatialQuery<IDrawableView> drawables);

        protected abstract void Light(T light);
        protected abstract void Dark(T light);
    }
}