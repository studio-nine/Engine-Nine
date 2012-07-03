#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// Represents a pool of render targets.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RenderTargetPool
    {
        private static RenderTargetPoolKey sharedKey = new RenderTargetPoolKey();
        private static Dictionary<RenderTargetPoolKey, FastList<RenderTargetPoolTag>> renderTargetPools
                 = new Dictionary<RenderTargetPoolKey, FastList<RenderTargetPoolTag>>(new RenderTargetPoolKeyEqualityComparer());
        
        /// <summary>
        /// Creates a render target from the pool without locking it.
        /// </summary>
        public static RenderTarget2D GetRenderTarget(GraphicsDevice graphics, int width, int height, SurfaceFormat surfaceFormat, DepthFormat depthFormat)
        {
            var key = sharedKey;
            key.Graphics = graphics;
            key.Width = width;
            key.Height = height;
            key.SurfaceFormat = surfaceFormat;
            key.DepthFormat = depthFormat;

            RenderTargetPoolTag tag;
            FastList<RenderTargetPoolTag> tags;
            if (!renderTargetPools.TryGetValue(key, out tags))
            {
                key = new RenderTargetPoolKey();
                key.Graphics = graphics;
                key.Width = width;
                key.Height = height;
                key.SurfaceFormat = surfaceFormat;
                key.DepthFormat = depthFormat;

                tag = new RenderTargetPoolTag();
                tag.RenderTarget = new RenderTarget2D(graphics, width, height, false, surfaceFormat, depthFormat, 0, RenderTargetUsage.DiscardContents);
                tag.RenderTarget.Tag = tag;

                tags = new FastList<RenderTargetPoolTag>();
                tags.Add(tag);
                renderTargetPools.Add(key, tags);
                return tag.RenderTarget;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                if ((tag = tags[i]).RefCount <= 0)
                    return tag.RenderTarget;                
            }
            
            tag = new RenderTargetPoolTag();
            tag.RenderTarget = new RenderTarget2D(graphics, width, height, false, surfaceFormat, depthFormat, 0, RenderTargetUsage.DiscardContents);
            tag.RenderTarget.Tag = tag;
            tags.Add(tag);
            return tag.RenderTarget;
        }

        /// <summary>
        /// Locks the target render target to prevent it from being created from the pool.
        /// </summary>
        public static void Lock(RenderTarget2D target)
        {
            RenderTargetPoolTag tag = null;
            if (target == null || (tag = target.Tag as RenderTargetPoolTag) == null)
                return;

            Debug.Assert(tag.RefCount >= 0);

            tag.RefCount++;
        }

        /// <summary>
        /// Unlocks the target render target.
        /// </summary>
        public static void Unlock(RenderTarget2D target)
        {
            RenderTargetPoolTag tag = null;
            if (target == null || (tag = target.Tag as RenderTargetPoolTag) == null)
                return;

            tag.RefCount--;

            Debug.Assert(tag.RefCount >= 0);
        }
    }

    class RenderTargetPoolTag
    {
        public int RefCount;
        public RenderTarget2D RenderTarget;
    }

    class RenderTargetPoolKey
    {
        public GraphicsDevice Graphics;
        public int Width;
        public int Height;
        public SurfaceFormat SurfaceFormat;
        public DepthFormat DepthFormat;
    }

    class RenderTargetPoolKeyEqualityComparer : IEqualityComparer<RenderTargetPoolKey>
    {
        public bool Equals(RenderTargetPoolKey x, RenderTargetPoolKey y)
        {
            return x.Graphics == y.Graphics &&
                   x.Width == y.Width &&
                   x.Height == y.Height &&
                   x.SurfaceFormat == y.SurfaceFormat &&
                   x.DepthFormat == y.DepthFormat;
        }

        public int GetHashCode(RenderTargetPoolKey obj)
        {
            return obj.Width.GetHashCode() ^
                   obj.Height.GetHashCode() ^
                   (int)obj.SurfaceFormat ^
                   (int)obj.DepthFormat;
        }
    }
}
