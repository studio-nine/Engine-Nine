#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ScreenEffects;
#endregion

namespace Nine.Graphics.Effects
{  
    /// <summary>
    /// Represents a shadow drawing technique using shadowmap.
    /// </summary>
    public class ShadowMap : IDisposable
    {
        bool hasBegin;
        Effect effect;
        BlurEffect blur;
        RenderTarget2D renderTarget;
        RenderTarget2D depthBlur;

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the size of the shadow map texture.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the preferred surface format of the shadow map texture.
        /// The default value is SurfaceFormat.Single.
        /// </summary>
        public SurfaceFormat SurfaceFormat { get; set; }

        /// <summary>
        /// Gets the underlying shadow map texture.
        /// </summary>
        public Texture2D Texture { get; private set; }

        /// <summary>
        /// Gets whether a bluring pass is applied to the final shadow map.
        /// </summary>
        public bool BlurEnabled { get; set; }

        /// <summary>
        /// Gets the effect used to draw models between Begin/End pair.
        /// </summary>
        public Effect Effect 
        {
            get { return effect ?? (effect = new DepthEffect(GraphicsDevice)); } 
        }

        /// <summary>
        /// Gets the underlying blur effect used to blur the final shadow map.
        /// </summary>
        public BlurEffect Blur
        {
            get { return blur ?? (blur = new BlurEffect(GraphicsDevice) 
                { SampleCount = BlurEffect.MinSampleCount }); } 
        }

        /// <summary>
        /// Initializes a new instance of ShadowMap.
        /// </summary>
        public ShadowMap(GraphicsDevice graphics, int size)
        {
            GraphicsDevice = graphics;
            Size = size;
#if SILVERLIGHT
            SurfaceFormat = SurfaceFormat.Color;
#else
            SurfaceFormat = SurfaceFormat.Single;
#endif
        }

        /// <summary>
        /// Begins the shadowmap generation process and clears the shadowmap to white.
        /// </summary>
        public void Begin()
        {
            if (hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasBegin = true;
            if (renderTarget == null || renderTarget.IsDisposed ||
#if !SILVERLIGHT
                renderTarget.IsContentLost || 
#endif
                renderTarget.Format != SurfaceFormat || renderTarget.Width != Size)
            {
                if (renderTarget != null)
                    renderTarget.Dispose();
                renderTarget = new RenderTarget2D(GraphicsDevice, Size, Size, false, SurfaceFormat,
                                                  GraphicsDevice.PresentationParameters.DepthStencilFormat);
            }
            renderTarget.Begin();
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Ends the shadowmap generation process and returns the result shadowmap texture.
        /// </summary>
        public Texture2D End()
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            hasBegin = false;

            Texture2D map = renderTarget.End();

            if (BlurEnabled)
            {
                if (depthBlur == null || depthBlur.IsDisposed ||
#if !SILVERLIGHT
                    depthBlur.IsContentLost ||
#endif
                    depthBlur.Format != SurfaceFormat || depthBlur.Width != Size)
                {
                    if (depthBlur != null)
                        depthBlur.Dispose();
                    depthBlur = new RenderTarget2D(GraphicsDevice, Size, Size, false, SurfaceFormat,
                                                   GraphicsDevice.PresentationParameters.DepthStencilFormat);
                }

                // Blur H
                depthBlur.Begin();
                Blur.Direction = MathHelper.PiOver4;

                GraphicsDevice.DrawFullscreenQuad(map, SamplerState.PointWrap, Color.White, Blur);

                map = depthBlur.End();

                // Blur V
                renderTarget.Begin();
                Blur.Direction = -MathHelper.PiOver4;

                GraphicsDevice.DrawFullscreenQuad(map, SamplerState.PointWrap, Color.White, Blur);

                map = renderTarget.End();

                // Restore states
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            return Texture = map;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (renderTarget != null)
                {
                    renderTarget.Dispose();
                    renderTarget = null;
                }
                if (depthBlur != null)
                {
                    depthBlur.Dispose();
                    depthBlur = null;
                }
            }
        }

        ~ShadowMap()
        {
            Dispose(false);
        }
    }
}
