namespace Nine.Graphics.Drawing
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Xna.Framework.Graphics;
    
    /// <summary>
    /// Represents a pool of render targets.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RenderTargetPool
    {
        private static RenderTargetPoolKey sharedKey = new RenderTargetPoolKey();
        private static Dictionary<RenderTargetPoolKey, FastList<PooledRenderTarget2D>> renderTargetPools
                 = new Dictionary<RenderTargetPoolKey, FastList<PooledRenderTarget2D>>(new RenderTargetPoolKeyEqualityComparer());
        
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

            PooledRenderTarget2D tag;
            FastList<PooledRenderTarget2D> tags;
            if (!renderTargetPools.TryGetValue(key, out tags))
            {
                key = new RenderTargetPoolKey();
                key.Graphics = graphics;
                key.Width = width;
                key.Height = height;
                key.SurfaceFormat = surfaceFormat;
                key.DepthFormat = depthFormat;

                tag = new PooledRenderTarget2D(graphics, width, height, false, surfaceFormat, depthFormat, 0, RenderTargetUsage.DiscardContents);
                tags = new FastList<PooledRenderTarget2D>();
                tags.Add(tag);
                renderTargetPools.Add(key, tags);
                return tag;
            }

            for (int i = 0; i < tags.Count; ++i)
            {
                if ((tag = tags[i]).RefCount <= 0)
                    return tag;
            }

            tag = new PooledRenderTarget2D(graphics, width, height, false, surfaceFormat, depthFormat, 0, RenderTargetUsage.DiscardContents);
            tags.Add(tag);
            return tag;
        }

        /// <summary>
        /// Locks the target render target to prevent it from being created from the pool.
        /// </summary>
        public static void Lock(RenderTarget2D target)
        {
            var tag = target as PooledRenderTarget2D;
            if (tag == null)
                return;

            Debug.Assert(tag.RefCount >= 0);

            tag.RefCount++;
        }

        /// <summary>
        /// Unlocks the target render target.
        /// </summary>
        public static void Unlock(RenderTarget2D target)
        {
            var tag = target as PooledRenderTarget2D;
            if (tag == null)
                return;

            tag.RefCount--;

            Debug.Assert(tag.RefCount >= 0);
        }
    }

    class PooledRenderTarget2D : RenderTarget2D
    {
        public PooledRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
            : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage)
        {
        
        }
        public int RefCount;
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
