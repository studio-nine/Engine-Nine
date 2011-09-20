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
using Nine.Graphics.Primitives;
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

        Matrix view;
        Matrix projection;
        Vector3 eyePosition;
        
        /// <summary>
        /// Gets the graphics device used by this effect.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the effect used to render the graphics buffer between BeginScene and EndScene pair.
        /// </summary>
        public GraphicsBufferEffect Effect { get; private set; }

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
            this.Effect = new GraphicsBufferEffect(graphics);
            this.NormalBufferFormat = SurfaceFormat.Color;
            this.DepthBufferFormat = SurfaceFormat.Single;
            this.LightBufferFormat = SurfaceFormat.Color;
        }

        /// <summary>
        /// Begins the rendering of the scene using DepthNormalEffect.
        /// </summary>
        public void Begin()
        {
            if (hasSceneBegin || hasLightBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasSceneBegin = true;

            CreateDepthNormalBuffers();

            GraphicsDevice.SetRenderTargets(normalBuffer, depthBuffer);
            GraphicsDevice.BlendState = BlendState.Opaque;
            ClearRenderTargets();
        }

        /// <summary>
        /// Ends the rendering of the scene and generates DepthNormalMap.
        /// </summary>
        public void End()
        {
            if (!hasSceneBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            GraphicsDevice.SetRenderTarget(null);
            hasSceneBegin = false;
        }

        /// <summary>
        /// Draws the specified lights onto the light buffer.
        /// </summary>
        public void DrawLights(Matrix view, Matrix projection, IEnumerable<IDeferredLight> lights)
        {
            BeginLights(view, projection);
            foreach (IDeferredLight light in lights)
                DrawLight(light);
            EndLights();
        }

        /// <summary>
        /// Begins the rendering of all the lights in the scene.
        /// </summary>
        private void BeginLights(Matrix view, Matrix projection)
        {
            if (hasLightBegin || hasSceneBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasLightBegin = true;

            this.view = view;
            this.projection = projection;
            this.eyePosition = Matrix.Invert(view).Translation;

            CreateLightBuffer();

            lightBuffer.Begin();

            // Setup render states for light rendering
            GraphicsDevice.Clear(Color.Transparent);

            // Set render state for lights
            GraphicsDevice.BlendState = LightBlendState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
        }
        
        /// <summary>
        /// Draws a light instance for DeferredEffect.
        /// </summary>
        private void DrawLight(IDeferredLight light)
        {
            if (!hasLightBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            IEffectTexture texture = light.Effect as IEffectTexture;
            if (texture != null)
            {
                texture.SetTexture(TextureUsage.NormalMap, normalBuffer);
                texture.SetTexture(TextureUsage.DepthBuffer, depthBuffer);
            }

            IEffectMatrices matrices = light.Effect as IEffectMatrices;
            if (matrices != null)
            {
                matrices.View = view;
                matrices.Projection = projection;
            }
            
            // Set our vertex declaration, vertex buffer, and index buffer.
            GraphicsDevice.SetVertexBuffer(light.VertexBuffer);
            GraphicsDevice.Indices = light.IndexBuffer;

            // Draw the model, using the specified effect.
            foreach (EffectPass effectPass in light.Effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                // Setup correct cull mode so that each pixel is rendered only once.
                //
                // NOTE: Setup cullmode after applying effect so that the world matrix of
                //       DeferredSpotLight is alway updated before calling Contains.
                //
                // FIXME: Testing against eye position is not accurate at all ???
                //
                // TODO: Test against near clip plane
                GraphicsDevice.RasterizerState = light.Contains(eyePosition) ? RasterizerState.CullClockwise :
                                                                               RasterizerState.CullCounterClockwise;

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                     light.VertexBuffer.VertexCount, 0, 
                                                     light.IndexBuffer.IndexCount / 3);
            }
        }

        /// <summary>
        /// Ends the rendering of lights and generates LightTexture.
        /// </summary>
        private Texture2D EndLights()
        {
            if (!hasLightBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            lightBuffer.End();
            hasLightBegin = false;

            // Restore render state to default
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            return LightBuffer;
        }

        private void CreateDepthNormalBuffers()
        {
            if (normalBuffer == null || normalBuffer.Format != NormalBufferFormat ||
                normalBuffer.IsDisposed || normalBuffer.IsContentLost ||
                normalBuffer.Width != GraphicsDevice.Viewport.Width ||
                normalBuffer.Height != GraphicsDevice.Viewport.Height)
            {
                if (normalBuffer != null)
                    normalBuffer.Dispose();

                normalBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                             GraphicsDevice.Viewport.Height, false, NormalBufferFormat,
                                             GraphicsDevice.PresentationParameters.DepthStencilFormat);
            }
            
            if (depthBuffer == null || depthBuffer.Format != DepthBufferFormat ||
                depthBuffer.IsDisposed || depthBuffer.IsContentLost ||
                depthBuffer.Width != GraphicsDevice.Viewport.Width ||
                depthBuffer.Height != GraphicsDevice.Viewport.Height)
            {
                if (depthBuffer != null)
                    depthBuffer.Dispose();

                depthBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                             GraphicsDevice.Viewport.Height, false, DepthBufferFormat,
                                             GraphicsDevice.PresentationParameters.DepthStencilFormat);
            }
        }

        private void CreateLightBuffer()
        {
            if (lightBuffer == null || lightBuffer.Format != LightBufferFormat ||
                lightBuffer.IsDisposed || lightBuffer.IsContentLost ||
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
                if (depthBuffer != null)
                {
                    depthBuffer.Dispose();
                    depthBuffer = null;
                }                
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
