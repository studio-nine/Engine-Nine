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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Represents a pool of render targets.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RenderTargetPool
    {
        private static Dictionary<RenderTargetPoolKey, List<RenderTargetPoolValue>> registry = new Dictionary<RenderTargetPoolKey, List<RenderTargetPoolValue>>(new RenderTargetPoolKeyEqualityComparer());
        private static RenderTargetPoolKey key = new RenderTargetPoolKey();

        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTarget2D AddRef(GraphicsDevice graphics, int width, int height)
        {
            return AddRef(graphics, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        }

        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTarget2D AddRef(GraphicsDevice graphics, int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat)
        {
            return AddRef(graphics, width, height, mipMap, surfaceFormat, depthFormat, 0, RenderTargetUsage.DiscardContents);
        }

        /// <summary>
        /// Acquires a render target with the specified parameter.
        /// </summary>
        public static RenderTarget2D AddRef(GraphicsDevice graphics, int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat, int preferredMultiSampleCount, RenderTargetUsage renderTargetUsage)
        {
            RenderTargetPoolKey key = new RenderTargetPoolKey();
            key.Graphics = graphics;
            key.Width = width;
            key.Height = height;
            key.Mipmap = mipMap;
            key.SurfaceFormat = surfaceFormat;
            key.DepthFormat = depthFormat;
            key.PreferredMultiSampleCount = preferredMultiSampleCount;
            key.RenderTargetUsage = renderTargetUsage;

            RenderTargetPoolValue renderTarget;
            List<RenderTargetPoolValue> list;

            if (!registry.TryGetValue(key, out list))
            {
                RenderTargetPoolKey newKey = new RenderTargetPoolKey();
                newKey.Graphics = graphics;
                newKey.Width = width;
                newKey.Height = height;
                newKey.Mipmap = mipMap;
                newKey.SurfaceFormat = surfaceFormat;
                newKey.DepthFormat = depthFormat;
                newKey.PreferredMultiSampleCount = preferredMultiSampleCount;
                newKey.RenderTargetUsage = renderTargetUsage;
                registry.Add(newKey, list = new List<RenderTargetPoolValue>());
            }
            if ((renderTarget = FindAvailableRenderTarget(list)) == null)
            {
                list.Add(renderTarget = new RenderTargetPoolValue()
                {
                    RenderTarget = new RenderTarget2D(graphics, width, height, mipMap, surfaceFormat, depthFormat, 0, renderTargetUsage),
                });
            }

            renderTarget.RefCount++;
            return renderTarget.RenderTarget;
        }

        /// <summary>
        /// Adds a reference to an existing render target.
        /// </summary>
        public static void AddRef(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
                return;

            foreach (List<RenderTargetPoolValue> list in registry.Values)
                foreach (RenderTargetPoolValue value in list)
                    if (value.RenderTarget == renderTarget)
                    {
                        value.RefCount++;
                        return;
                    }

            throw new ArgumentException("Did not found the specified render target.");
        }

        internal static RenderTarget2D AddRef(GraphicsDevice graphicsDevice, Texture2D input, Vector2? renderTargetSize, float renderTargetScale, SurfaceFormat? surfaceFormat)
        {
            return AddRef(graphicsDevice, input, renderTargetSize, renderTargetScale, surfaceFormat, null);
        }

        internal static RenderTarget2D AddRef(GraphicsDevice graphicsDevice, Texture2D input, Vector2? renderTargetSize, float renderTargetScale, SurfaceFormat? surfaceFormat, DepthFormat? depthFormat)
        {
            float width = input != null ? input.Width : graphicsDevice.PresentationParameters.BackBufferWidth;
            float height = input != null ? input.Height : graphicsDevice.PresentationParameters.BackBufferHeight;
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
        /// Releases a render target.
        /// </summary>
        public static void Release(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
                return;

            foreach (List<RenderTargetPoolValue> list in registry.Values)
                foreach (RenderTargetPoolValue value in list)
                    if (value.RenderTarget == renderTarget)
                    {
                        value.RefCount--;
                        return;
                    }
        }
        
        /// <summary>
        /// Gets the number of render targets been used.
        /// </summary>
        public static int ActiveRenderTargets
        {
            get
            {
                int count = 0;
                foreach (List<RenderTargetPoolValue> list in registry.Values)
                    foreach (RenderTargetPoolValue value in list)
                        if (value.RefCount > 0)
                            count++;
                return count;
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
                foreach (List<RenderTargetPoolValue> list in registry.Values)
                    foreach (RenderTargetPoolValue value in list)
                        if (!value.RenderTarget.IsDisposed)
                            count++;
                return count;
            }
        }

        static RenderTargetPoolValue FindAvailableRenderTarget(List<RenderTargetPoolValue> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
#if SILVERLIGHT
                if (list[i].RenderTarget.IsDisposed)
#else
                if (list[i].RenderTarget.IsDisposed || list[i].RenderTarget.IsContentLost)
#endif
                {
                    list.RemoveAt(i);
                    i--;
                }
                else if (list[i].RefCount <= 0)
                {
                    return list[i];
                }
            }
            return null;
        }
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
            return obj.Graphics.GetHashCode() +
                   obj.Width.GetHashCode() +
                   obj.Height.GetHashCode() +
                   obj.Mipmap.GetHashCode() +
                   obj.PreferredMultiSampleCount.GetHashCode() +
                   obj.SurfaceFormat.GetHashCode() +
                   obj.DepthFormat.GetHashCode() +
                   obj.RenderTargetUsage.GetHashCode();
        }
    }

    class RenderTargetPoolValue
    {
        public RenderTarget2D RenderTarget;
        public int RefCount;
    }
}
