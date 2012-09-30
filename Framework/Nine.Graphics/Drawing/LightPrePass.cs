namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

    /// <summary>
    /// Defines a light used by deferred rendering.
    /// </summary>
    public interface IDeferredLight
    {
        /// <summary>
        /// Gets the drawable object that is used to generate the light buffer.
        /// </summary>
        IDrawableObject PrepareLightGeometry(DrawingContext context);
    }

    /// <summary>
    /// Represents a deferred lighting technique.
    /// </summary>
    [NotContentSerializable]
    public class LightPrePass : Pass, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the graphics device used by this effect.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

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
        #endregion

        #region Fields
        bool hasSceneBegin;
        bool hasLightBegin;

        RenderTarget2D normalBuffer;
        RenderTarget2D depthBuffer;
        RenderTarget2D lightBuffer;
        RenderTargetBinding[] renderTargetBinding = new RenderTargetBinding[2];

        DepthStencilState greaterDepth;
        BlendState lightBlendState;

        ClearMaterial clearMaterial;
        DepthAndNormalMaterial gBufferMaterial;
        FullScreenQuad clearQuad;

        DrawingPass drawingPass;
        ISpatialQuery<IDeferredLight> lightQuery;
        FastList<IDeferredLight> deferredLights;
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="LightPrePass"/> class.
        /// </summary>
        public LightPrePass(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.GraphicsDevice = graphics;
            this.NormalBufferFormat = SurfaceFormat.Color;
#if SILVERLIGHT
            this.DepthBufferFormat = SurfaceFormat.Color;
#else
            this.DepthBufferFormat = SurfaceFormat.Single;
#endif
            this.LightBufferFormat = SurfaceFormat.Color;
            this.gBufferMaterial = new DepthAndNormalMaterial(graphics);

            this.greaterDepth = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferFunction = CompareFunction.Greater,
                DepthBufferWriteEnable = false
            };

            this.lightBlendState = new BlendState()
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
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            Begin(context);
            {
                if (drawingPass == null)
                {
                    drawingPass = new DrawingPass();
                    drawingPass.MaterialUsage = MaterialUsage.DepthAndNormal;
                }
                drawingPass.Draw(context, drawables);
            }
            End(context);

            context.textures[TextureUsage.DepthBuffer] = depthBuffer;
            context.textures[TextureUsage.NormalBuffer] = normalBuffer;

            if (lightQuery == null)
            {
                lightQuery = context.CreateSpatialQuery<IDeferredLight>(null);
                deferredLights = new FastList<IDeferredLight>();
            }

            BeginLights(context);
            {
                lightQuery.FindAll(context.ViewFrustum, deferredLights);
                for (int i = 0; i < deferredLights.Count; ++i)
                    DrawLight(context, deferredLights[i]);
                deferredLights.Clear();
            }
            EndLights(context);

            context.textures[TextureUsage.LightBuffer] = lightBuffer;
        }

        /// <summary>
        /// Begins the rendering of the scene using DepthNormalEffect.
        /// </summary>
        private void Begin(DrawingContext context)
        {
            if (hasSceneBegin || hasLightBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasSceneBegin = true;

            CreateDepthNormalBuffers();

            // Maintain render target stack
            GraphicsExtensions.PushRenderTarget(context.graphics, null);

            renderTargetBinding[0] = new RenderTargetBinding(depthBuffer);
            renderTargetBinding[1] = new RenderTargetBinding(normalBuffer);

            GraphicsDevice.SetRenderTargets(renderTargetBinding);
            GraphicsDevice.BlendState = BlendState.Opaque;

            ClearRenderTargets(context);
        }

        /// <summary>
        /// Ends the rendering of the scene and generates DepthNormalMap.
        /// </summary>
        private void End(DrawingContext context)
        {
            if (!hasSceneBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            GraphicsExtensions.PopRenderTarget(context.graphics);

            context.textures[TextureUsage.DepthBuffer] = DepthBuffer;
            context.textures[TextureUsage.NormalBuffer] = NormalBuffer;
            GraphicsDevice.SetRenderTarget(null);
            hasSceneBegin = false;
        }

        /// <summary>
        /// Begins the rendering of all the lights in the scene.
        /// </summary>
        private void BeginLights(DrawingContext context)
        {
            if (hasLightBegin || hasSceneBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasLightBegin = true;

            CreateLightBuffer();

            lightBuffer.Begin();

            // Setup render states for light rendering
            // Clear specular intensity to 0            
            GraphicsDevice.Clear(new Color(context.ambientLightColor.X, context.ambientLightColor.Y, context.ambientLightColor.Z, 0));

            // Set render state for lights
            GraphicsDevice.BlendState = lightBlendState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Set up textures and sampler states
            GraphicsDevice.Textures[0] = depthBuffer;
            GraphicsDevice.Textures[1] = normalBuffer;
            GraphicsDevice.SamplerStates[0] = GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
        }

        /// <summary>
        /// Draws a light instance for DeferredEffect.
        /// </summary>
        private void DrawLight(DrawingContext context, IDeferredLight light)
        {
            if (!hasLightBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            var lightGeometry = light.PrepareLightGeometry(context);
            if (lightGeometry == null || !lightGeometry.OnAddedToView(context))
                return;

            var lightMaterial = lightGeometry.Material;
            if (lightMaterial == null)
                return;

            lightMaterial.SetTexture(TextureUsage.DepthBuffer, depthBuffer);
            lightMaterial.SetTexture(TextureUsage.NormalMap, normalBuffer);

            lightMaterial.BeginApply(context);

            // Draw the model, using the specified effect.
            // Setup correct cull mode so that each pixel is rendered only once.
            GraphicsDevice.DepthStencilState = greaterDepth;

            lightGeometry.Draw(context, lightMaterial);

            lightMaterial.EndApply(context);
        }

        /// <summary>
        /// Ends the rendering of lights and generates LightTexture.
        /// </summary>
        private Texture2D EndLights(DrawingContext context)
        {
            if (!hasLightBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            lightBuffer.End();
            hasLightBegin = false;

            // Restore render state to default
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = GraphicsDevice.SamplerStates[1] = context.SamplerState;

            context.textures[TextureUsage.LightBuffer] = LightBuffer;
            return LightBuffer;
        }

        private void CreateDepthNormalBuffers()
        {
            if (depthBuffer == null || depthBuffer.Format != DepthBufferFormat ||
#if SILVERLIGHT
                depthBuffer.IsDisposed ||
#else
 depthBuffer.IsDisposed || depthBuffer.IsContentLost ||
#endif
 depthBuffer.Width != GraphicsDevice.Viewport.Width ||
                depthBuffer.Height != GraphicsDevice.Viewport.Height)
            {
                if (depthBuffer != null)
                    depthBuffer.Dispose();

                depthBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                                 GraphicsDevice.Viewport.Height, false, DepthBufferFormat,
                                                 GraphicsDevice.PresentationParameters.DepthStencilFormat);
            }

            if (normalBuffer == null || normalBuffer.Format != NormalBufferFormat ||
#if SILVERLIGHT
                normalBuffer.IsDisposed ||
#else
 normalBuffer.IsDisposed || normalBuffer.IsContentLost ||
#endif
 normalBuffer.Width != GraphicsDevice.Viewport.Width ||
                normalBuffer.Height != GraphicsDevice.Viewport.Height)
            {
                if (normalBuffer != null)
                    normalBuffer.Dispose();

                normalBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                                  GraphicsDevice.Viewport.Height, false, NormalBufferFormat,
                                                  DepthFormat.None);
            }
        }

        private void CreateLightBuffer()
        {
            if (lightBuffer == null || lightBuffer.Format != LightBufferFormat ||
#if SILVERLIGHT
                lightBuffer.IsDisposed ||
#else
 lightBuffer.IsDisposed || lightBuffer.IsContentLost ||
#endif
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

        private void ClearRenderTargets(DrawingContext context)
        {
            if (clearMaterial == null)
            {
                clearMaterial = new ClearMaterial(GraphicsDevice);
                clearQuad = new FullScreenQuad(GraphicsDevice);
            }

            clearMaterial.effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            clearQuad.Draw(context, clearMaterial);

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

        ~LightPrePass()
        {
            Dispose(false);
        }
        #endregion
    }
}