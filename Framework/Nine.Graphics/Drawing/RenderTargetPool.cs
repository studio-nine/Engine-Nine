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
    public sealed class RenderTargetPool : IDisposable
    {
        private int refCount;
        private RenderTargetPoolKey key;
        private FastList<RenderTarget2D> targets = new FastList<RenderTarget2D>();

        /// <summary>
        /// Prevents a default instance of the <see cref="RenderTargetPool"/> class from being created.
        /// </summary>
        private RenderTargetPool() { }

        #region Lock & Unlock
        /// <summary>
        /// Creates a render target from the pool without locking it.
        /// </summary>
        public RenderTarget2D Create()
        {
            RenderTarget2D result;
            for (int i = 0; i < targets.Count; i++)
            {
                result = targets[i];
                if (((RenderTargetPoolTag)(result.Tag)).RefCount <= 0)
                {
                    if (!result.IsDisposed && !result.IsContentLost)
                        return result;
                    targets[i] = targets[--targets.Count];
                    i--;
                }
            }

            result = new RenderTarget2D(
                    key.Graphics, key.Width, key.Height, key.Mipmap, key.SurfaceFormat,
                    key.DepthFormat, key.PreferredMultiSampleCount, key.RenderTargetUsage);
            
            result.Tag = new RenderTargetPoolTag()
            {
                RefCount = 0,
                Pool = this,
            };

            targets.Add(result);
            return result;
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
        #endregion

        #region AddRef & Release
        private static Dictionary<RenderTargetPoolKey, RenderTargetPool> renderTargetPools = new Dictionary<RenderTargetPoolKey, RenderTargetPool>(new RenderTargetPoolKeyEqualityComparer());        
        
        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTargetPool AddRef(GraphicsDevice graphics, int width, int height)
        {
            return AddRef(graphics, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        }

        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTargetPool AddRef(GraphicsDevice graphics, int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat)
        {
            return AddRef(graphics, width, height, mipMap, surfaceFormat, depthFormat, 0, RenderTargetUsage.DiscardContents);
        }

        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTargetPool AddRef(GraphicsDevice graphics, int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat, int preferredMultiSampleCount, RenderTargetUsage renderTargetUsage)
        {
            var key = sharedKey;
            key.Graphics = graphics;
            key.Width = width;
            key.Height = height;
            key.Mipmap = mipMap;
            key.SurfaceFormat = surfaceFormat;
            key.DepthFormat = depthFormat;
            key.PreferredMultiSampleCount = preferredMultiSampleCount;
            key.RenderTargetUsage = renderTargetUsage;

            RenderTargetPool renderTargetPool;
            if (!renderTargetPools.TryGetValue(key, out renderTargetPool))
            {
                key = new RenderTargetPoolKey();
                key.Graphics = graphics;
                key.Width = width;
                key.Height = height;
                key.Mipmap = mipMap;
                key.SurfaceFormat = surfaceFormat;
                key.DepthFormat = depthFormat;
                key.PreferredMultiSampleCount = preferredMultiSampleCount;
                key.RenderTargetUsage = renderTargetUsage;

                renderTargetPools.Add(key, renderTargetPool = new RenderTargetPool() { key = key });
            }
            renderTargetPool.refCount++;
            return renderTargetPool;
        }

        private static RenderTargetPoolKey sharedKey = new RenderTargetPoolKey();

        /// <summary>
        /// Releases a reference to the render target pool.
        /// </summary>
        public void Release()
        {
            if (--refCount == 0)
            {
                ((IDisposable)this).Dispose();
                renderTargetPools.Remove(this.key);
            }
        }

        void IDisposable.Dispose()
        {
            for (int i = 0; i < targets.Count; i++)
                targets[i].Dispose();
        }
        #endregion

        #region Statistics
#if DEBUG
        /// <summary>
        /// Gets the number of render targets been used.
        /// </summary>
        public static int ActiveRenderTargets
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Gets the number of render targets been created.
        /// </summary>
        public static int TotalRenderTargets
        {
            get
            {
                int count = 0;
                foreach (var list in renderTargetPools.Values)
                    if (list.refCount > 0)
                        foreach (var value in list.targets)
                            if (!value.IsDisposed)
                                count++;
                return count;
            }
        }
#endif
        #endregion
    }

    class RenderTargetPoolTag
    {
        public int RefCount;
        public RenderTargetPool Pool;
    }

    class RenderTargetPoolKey
    {
        public GraphicsDevice Graphics;
        public int Width;
        public int Height;
        public bool Mipmap;
        public int PreferredMultiSampleCount;
        public SurfaceFormat SurfaceFormat;
        public DepthFormat DepthFormat;
        public RenderTargetUsage RenderTargetUsage;
    }

    class RenderTargetPoolKeyEqualityComparer : IEqualityComparer<RenderTargetPoolKey>
    {
        public bool Equals(RenderTargetPoolKey x, RenderTargetPoolKey y)
        {
            return x.Graphics == y.Graphics &&
                   x.Width == y.Width &&
                   x.Height == y.Height &&
                   x.Mipmap == y.Mipmap &&
                   x.PreferredMultiSampleCount == y.PreferredMultiSampleCount &&
                   x.SurfaceFormat == y.SurfaceFormat &&
                   x.DepthFormat == y.DepthFormat &&
                   x.RenderTargetUsage == y.RenderTargetUsage;
        }

        public int GetHashCode(RenderTargetPoolKey obj)
        {
            return obj.Width.GetHashCode() ^
                   obj.Height.GetHashCode() ^
                   obj.Mipmap.GetHashCode() ^
                   obj.PreferredMultiSampleCount.GetHashCode() ^
                   (int)obj.SurfaceFormat ^
                   (int)obj.DepthFormat ^
                   (int)obj.RenderTargetUsage;
        }
    }
}
