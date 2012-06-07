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
        private int nextValidRenderTarget;
        private RenderTargetPoolKey key;
        private List<RenderTarget2D> renderTargets = new List<RenderTarget2D>();

        /// <summary>
        /// Prevents a default instance of the <see cref="RenderTargetPool"/> class from being created.
        /// </summary>
        private RenderTargetPool() { }

        #region Lock & Unlock
        /// <summary>
        /// Creates a render target from the pool and then lock it.
        /// </summary>
        public RenderTarget2D Lock()
        {
            RenderTarget2D result;
            if (nextValidRenderTarget < renderTargets.Count)
            {
                result = renderTargets[nextValidRenderTarget];
                if (!result.IsDisposed && !result.IsContentLost)
                    return result;

                MakeInvalid(((RenderTargetPoolTag)(result.Tag)).Index);
            }

            result = new RenderTarget2D(
                    key.Graphics, key.Width, key.Height, key.Mipmap, key.SurfaceFormat,
                    key.DepthFormat, key.PreferredMultiSampleCount, key.RenderTargetUsage);
            
            result.Tag = new RenderTargetPoolTag()
            {
                Index = nextValidRenderTarget++,
                RefCount = 1,
            };

            renderTargets.Add(result);
            return result;
        }

        /// <summary>
        /// Locks the target render target to prevent it from being created from the pool.
        /// </summary>
        public void Lock(RenderTarget2D target)
        {
            RenderTargetPoolTag tag = null;
            if (target == null || (tag = target.Tag as RenderTargetPoolTag) == null)
                return;

            tag.RefCount++;
            nextValidRenderTarget++;
        }

        /// <summary>
        /// Unlocks the target render target.
        /// </summary>
        public void Unlock(RenderTarget2D target)
        {
            RenderTargetPoolTag tag = null;
            if (target == null || (tag = target.Tag as RenderTargetPoolTag) == null)
                return;

            if (--tag.RefCount == 0)
                MakeAvailable(tag.Index);
        }

        private void MakeAvailable(int i)
        {
            nextValidRenderTarget--;
            if (nextValidRenderTarget != i)
            {
                var temp = renderTargets[nextValidRenderTarget];
                renderTargets[nextValidRenderTarget] = renderTargets[i];
                renderTargets[i] = temp;

                ((RenderTargetPoolTag)(renderTargets[i].Tag)).Index = i;
                ((RenderTargetPoolTag)(renderTargets[nextValidRenderTarget].Tag)).Index = nextValidRenderTarget;
            }
        }

        private void MakeInvalid(int i)
        {
            MakeAvailable(i);

            if (nextValidRenderTarget < renderTargets.Count)
            {
                var last = renderTargets.Count - 1;
                if (last != nextValidRenderTarget)
                {
                    var temp = renderTargets[nextValidRenderTarget];
                    renderTargets[nextValidRenderTarget] = renderTargets[last];
                    renderTargets[last] = temp;

                    ((RenderTargetPoolTag)(renderTargets[nextValidRenderTarget].Tag)).Index = nextValidRenderTarget;
                    renderTargets.RemoveAt(last);
                }
            }
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
            var key = new RenderTargetPoolKey();
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
                renderTargetPools.Add(key, renderTargetPool = new RenderTargetPool() { key = key });
            renderTargetPool.refCount++;
            return renderTargetPool;
        }

        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTargetPool AddRef(GraphicsDevice graphicsDevice, Texture2D input, Vector2? renderTargetSize, float renderTargetScale, SurfaceFormat? surfaceFormat)
        {
            return AddRef(graphicsDevice, input, renderTargetSize, renderTargetScale, surfaceFormat, null);
        }

        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTargetPool AddRef(GraphicsDevice graphicsDevice, Texture2D input, Vector2? renderTargetSize, float renderTargetScale, SurfaceFormat? surfaceFormat, DepthFormat? depthFormat)
        {
            float width = input != null ? input.Width : graphicsDevice.Viewport.Width;
            float height = input != null ? input.Height : graphicsDevice.Viewport.Height;
            SurfaceFormat inputFormat = input != null ? input.Format : graphicsDevice.PresentationParameters.BackBufferFormat;

            float renderTargetWidth = renderTargetSize.HasValue ? renderTargetSize.Value.X : width;
            float renderTargetHeight = renderTargetSize.HasValue ? renderTargetSize.Value.Y : height;
            SurfaceFormat sFormat = surfaceFormat.HasValue ? surfaceFormat.Value : inputFormat;
            DepthFormat dFormat = depthFormat.HasValue ? depthFormat.Value : DepthFormat.None;

            return AddRef(graphicsDevice, (int)(renderTargetWidth * renderTargetScale),
                                          (int)(renderTargetHeight * renderTargetScale),
                                          false, sFormat, dFormat);
        }

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
            for (int i = 0; i < renderTargets.Count; i++)
                renderTargets[i].Dispose();
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
                        foreach (var value in list.renderTargets)
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
        public int Index;
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
            return obj.Graphics.GetHashCode() ^
                   obj.Width.GetHashCode() ^
                   obj.Height.GetHashCode() ^
                   obj.Mipmap.GetHashCode() ^
                   obj.PreferredMultiSampleCount.GetHashCode() ^
                   obj.SurfaceFormat.GetHashCode() ^
                   obj.DepthFormat.GetHashCode() ^
                   obj.RenderTargetUsage.GetHashCode();
        }
    }
}
