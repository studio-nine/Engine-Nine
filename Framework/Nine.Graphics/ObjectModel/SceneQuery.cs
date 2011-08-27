#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ScreenEffects;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects.Deferred;
using Nine.Graphics.Effects;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Querys all drawables from a scene of ISpatialQueryable.
    /// This class checks each returned object to see if it implements
    /// T or IEnumerable.
    /// </summary>
    class SceneQuery<T> : ISpatialQuery<T> where T : class
    {
        ISpatialQuery<ISpatialQueryable> scene;

        public SceneQuery(ISpatialQuery<ISpatialQueryable> scene)
        {
            if (scene == null)
                throw new ArgumentNullException();
            this.scene = scene;
        }

        public IEnumerable<T> FindAll(Vector3 position, float radius)
        {
            return Enumerate(scene.FindAll(position, radius), false);
        }

        public IEnumerable<T> FindAll(Ray ray)
        {
            return Enumerate(scene.FindAll(ray), false);
        }

        public IEnumerable<T> FindAll(BoundingBox boundingBox)
        {
            return Enumerate(scene.FindAll(boundingBox), false);
        }

        public IEnumerable<T> FindAll(BoundingFrustum frustum)
        {
            return Enumerate(scene.FindAll(frustum), false);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate(scene, false).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<T> Enumerate(IEnumerable iEnumerable, bool ignoreSpatialQueryables)
        {
            foreach (var item in iEnumerable)
            {
                if (ignoreSpatialQueryables && item is ISpatialQueryable)
                    continue;

                T obj = item as T;
                if (obj != null)
                    yield return obj;

                IEnumerable enumerable = item as IEnumerable;
                if (enumerable != null)
                {
                    // Ignore ISpatialQueryable since they are explicitly
                    // added to the scene manager.
                    foreach (var result in Enumerate(enumerable, true))
                        yield return result;
                }
            }
        }
    }
}