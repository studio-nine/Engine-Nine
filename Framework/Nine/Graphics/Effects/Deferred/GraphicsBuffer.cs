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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ScreenEffects;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
    /// <summary>
    /// Represents the G-Buffer used by light pre-pass deferred lighting.
    /// </summary>
    public class GraphicsBuffer : IDisposable
    {
        bool hasSceneBegin;
        bool hasLightBegin;

        RenderTarget2D normalBuffer;
        RenderTarget2D depthBuffer;
        RenderTarget2D lightBuffer;

        ClearEffect clearEffect;
        Quad clearQuad;
        
        /// <summary>
        /// Gets the graphics device used by this effect.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the effect used to render the graphics buffer between BeginScene and EndScene pair.
        /// </summary>
        public GraphicsBufferEffect GraphicsBufferEffect { get; private set; }

        /// <summary>
        /// Gets the texture that contains world space normal info of the scene.
        /// </summary>
        /// <remarks>
        /// World space normal info is stored in the RGB channel of the texture.
        /// Specular power is stored in the A channel of the texture.
        /// </remarks>
        public Texture2D NormalBuffer { get { return normalBuffer; } }

        /// <summary>
        /// Gets the texture that contains depth info of the scene.
        /// </summary>
        /// <remarks>
        /// Depth info is stored in the R channel of the texture.
        /// </remarks>
        public Texture2D DepthBuffer { get { return depthBuffer; } }

        /// <summary>
        /// Gets the texture that contains lighting info of the scene.
        /// </summary>
        /// <remarks>
        /// Light color is stored in the RGB channel of the texture.
        /// Light specular multiplier is stored in the Alpha channel of the texture.
        /// Light specular color is ignored.
        /// </remarks>
        public Texture2D LightBuffer { get { return lightBuffer; } }

        /// <summary>
        /// Gets or sets the preferred surface format for graphics buffer.
        /// The default value is SurfaceFormat.Color.
        /// </summary>
        public SurfaceFormat NormalBufferFormat { get; set; }

        /// <summary>
        /// Gets or sets the preferred surface format for graphics buffer.
        /// The default value is SurfaceFormat.Single.
        /// </summary>
        public SurfaceFormat DepthBufferFormat { get; set; }

        /// <summary>
        /// Gets or sets the preferred surface format for light buffer.
        /// The default value is SurfaceFormat.Color.
        /// </summary>
        public SurfaceFormat LightBufferFormat { get; set; }

        /// <summary>
        /// Gets the blend state used to draw the lights.
        /// </summary>
        public BlendState BlendState { get { return LightBlendState; } }

        /// <summary>
        /// Gets the depth stencil state used to draw the lights.
        /// </summary>
        public DepthStencilState DepthStencilState { get { return DepthStencilState.None; } }
        
        internal static BlendState LightBlendState;
        static GraphicsBuffer()
        {
            LightBlendState = new BlendState()
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };
        }

        /// <summary>
        /// Initializes a new instance of DeferredEffect.
        /// </summary>
        public GraphicsBuffer(GraphicsDevice graphics)
        {
            if (graphics == null || graphics.IsDisposed)
                throw new ArgumentException("graphics");

            this.GraphicsDevice = graphics;
            this.GraphicsBufferEffect = new GraphicsBufferEffect(graphics);
            this.NormalBufferFormat = SurfaceFormat.Color;
            this.DepthBufferFormat = SurfaceFormat.Single;
            this.LightBufferFormat = SurfaceFormat.Color;
        }

        /// <summary>
        /// Begins the rendering of the scene using DepthNormalEffect.
        /// </summary>
        public void BeginScene()
        {
            if (hasSceneBegin || hasLightBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasSceneBegin = true;

            CreateDepthNormalBuffers();

            //GraphicsExtensions.PushRenderTarget(normalBuffer);
            GraphicsDevice.SetRenderTargets(normalBuffer, depthBuffer);
            GraphicsDevice.Clear(Color.Black);
            //GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.White, 1, 0);
            ClearRenderTargets();
        }

        /// <summary>
        /// Ends the rendering of the scene and generates DepthNormalTexture.
        /// </summary>
        public void EndScene()
        {
            if (!hasSceneBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            //GraphicsExtensions.PopRenderTarget(GraphicsDevice);
            GraphicsDevice.SetRenderTarget(null);
            hasSceneBegin = false;
        }

        /// <summary>
        /// Begins the rendering of all the lights in the scene.
        /// </summary>
        public void BeginLights()
        {
            if (hasLightBegin || hasSceneBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasLightBegin = true;

            CreateLightBuffer();

            lightBuffer.Begin();

            // Setup render states for light rendering
            GraphicsDevice.Clear(Color.Transparent);

            // Set render state for lights
            GraphicsDevice.BlendState = LightBlendState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
        }

        /// <summary>
        /// Ends the rendering of lights and generates LightTexture.
        /// </summary>
        public Texture2D EndLights()
        {
            if (!hasLightBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            lightBuffer.End();
            hasLightBegin = false;

            // Restore render state to default
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            return LightBuffer;
        }

        private void CreateDepthNormalBuffers()
        {
            if (normalBuffer == null || depthBuffer == null ||
                normalBuffer.Width != GraphicsDevice.Viewport.Width ||
                normalBuffer.Height != GraphicsDevice.Viewport.Height)
            {
                if (normalBuffer != null)
                    normalBuffer.Dispose();
                if (depthBuffer != null)
                    depthBuffer.Dispose();

                normalBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                             GraphicsDevice.Viewport.Height, false, NormalBufferFormat,
                                             GraphicsDevice.PresentationParameters.DepthStencilFormat);

                depthBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                             GraphicsDevice.Viewport.Height, false, DepthBufferFormat,
                                             GraphicsDevice.PresentationParameters.DepthStencilFormat);
            }
        }

        private void CreateLightBuffer()
        {
            if (lightBuffer == null ||
                lightBuffer.Width != GraphicsDevice.Viewport.Width ||
                lightBuffer.Height != GraphicsDevice.Viewport.Height)
            {
                if (lightBuffer != null)
                    lightBuffer.Dispose();

                lightBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                                 GraphicsDevice.Viewport.Height, false, LightBufferFormat,
                                                 DepthFormat.None);
            }
        }

        private void ClearRenderTargets()
        {
            if (clearEffect == null)
                clearEffect = new ClearEffect(GraphicsDevice);
            if (clearQuad == null)
                clearQuad = new Quad(GraphicsDevice);

            clearEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SetVertexBuffer(clearQuad.VertexBuffer);
            GraphicsDevice.Indices = clearQuad.IndexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, clearQuad.VertexCount, 0, clearQuad.PrimitiveCount);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
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
                if (normalBuffer != null)
                {
                    normalBuffer.Dispose();
                    normalBuffer = null;
                }
                if (lightBuffer != null)
                {
                    lightBuffer.Dispose();
                    lightBuffer = null;
                }
            }
        }

        ~GraphicsBuffer()
        {
            Dispose(false);
        }
    }
}
