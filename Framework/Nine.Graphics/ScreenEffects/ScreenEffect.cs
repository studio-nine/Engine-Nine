#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ScreenEffects
{
    /// <summary>
    /// Interface for a single post processing effect.
    /// </summary>
    public interface IScreenEffect
    {
        /// <summary>
        /// Processes the input texture and returns the output texture.
        /// </summary>
        Texture2D Process(Texture2D input);
        
        /// <summary>
        /// Processes and draws the input texture on to the screen.
        /// </summary>
        void ProcessAndDraw(Texture2D input);
    }

    /// <summary>
    /// Represents post processing effects.
    /// </summary>
    public class ScreenEffect : ChainedScreenEffect
    {
        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the surface format of the render target.
        /// Specify null to use the surface format of the current backbuffer.
        /// </summary>
        public SurfaceFormat? SurfaceFormat { get; set; }

        /// <summary>
        /// Gets or sets the render target size.
        /// Specify null to use current viewport size.
        /// </summary>
        public Vector2? RenderTargetSize { get; set; }

        /// <summary>
        /// Gets or sets the render target scale. This value is multiplied with
        /// <c>RenderTargetSize</c> to determine the final size of the render target.
        /// </summary>
        public float RenderTargetScale { get; set; }

        private RenderTarget2D renderTarget;
        private bool hasBegin;

        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        /// <param name="graphics">A GraphicsDevice instance.</param>
        public ScreenEffect(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            RenderTargetScale = 1;
            GraphicsDevice = graphics;
        }

        /// <summary>
        /// Begins the rendering of the scene to be post processed.
        /// </summary>
        public void Begin()
        {
            if (hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasBegin = true;

            if (Effects.Count <= 0 || !Enabled)
                return;

            renderTarget = RenderTargetPool.AddRef(GraphicsDevice, null, RenderTargetSize, RenderTargetScale, SurfaceFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
            renderTarget.Begin();
        }

        /// <summary>
        /// Ends the rendering of the scene, applying all the post processing effects.
        /// </summary>
        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            hasBegin = false;

            if (renderTarget != null)
            {
                renderTarget.End();
                RenderTargetPool.Release(renderTarget);
                ((IScreenEffect)this).ProcessAndDraw(renderTarget);
                renderTarget = null;

                for (int i = 0; i < 16; i++)
                    GraphicsDevice.Textures[i] = null;
            }
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Creates a bloom post processing effect.
        /// </summary>
        public static ScreenEffect CreateBloom(GraphicsDevice graphics, float threshold, float blurAmount)
        {
            MultiPassScreenEffectPass basePass = new MultiPassScreenEffectPass();
            basePass.BlendState = BlendState.Opaque;

            MultiPassScreenEffectPass brightPass = new MultiPassScreenEffectPass();
            brightPass.Effects.Add(new ScaleEffect(graphics), 0.5f);
            brightPass.Effects.Add(new ThresholdEffect(graphics) { Threshold = threshold });
            brightPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction = 0 });
            brightPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction = MathHelper.PiOver2 });
            brightPass.Effects.Add(new ScaleEffect(graphics), 2);
            brightPass.BlendState = BlendState.Additive;

            MultiPassScreenEffect multipassEffect = new MultiPassScreenEffect(graphics);
            multipassEffect.Passes.Add(basePass);
            multipassEffect.Passes.Add(brightPass);

            ScreenEffect screenEffect = new ScreenEffect(graphics);
            screenEffect.Effects.Add(multipassEffect);
            return screenEffect;
        }

        /// <summary>
        /// Creates a High Dynamic Range (HDR) post processing effect.
        /// </summary>
        public static ScreenEffect CreateHighDynamicRange(GraphicsDevice graphics, float threshold, float blurAmount, float exposure, float maxLuminance, float? adoption)
        {
            SurfaceFormat hdrFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat.HdrBlendable;

            MultiPassScreenEffectPass basePass = new MultiPassScreenEffectPass();

            MultiPassScreenEffectPass brightPass = new MultiPassScreenEffectPass();
            brightPass.Effects.Add(new ScaleEffect(graphics), 0.5f);
            brightPass.Effects.Add(new ThresholdEffect(graphics) { Threshold = threshold });
            brightPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction = 0 });
            brightPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction = MathHelper.PiOver2 });
            brightPass.Effects.Add(new ScaleEffect(graphics), 2);
            brightPass.TextureUsage = TextureUsage.Bloom;

            MultiPassScreenEffectPass luminancePass = CreateLuminanceChain(graphics, 4, adoption);

            MultiPassScreenEffect multipassEffect = new MultiPassScreenEffect(graphics);
            multipassEffect.Passes.Add(basePass);
            multipassEffect.Passes.Add(brightPass);
            multipassEffect.Passes.Add(luminancePass);
            multipassEffect.CombineEffect = new ToneMappingEffect(graphics) { Exposure = exposure, MaxLuminance = maxLuminance };

            ScreenEffect screenEffect = new ScreenEffect(graphics);
            screenEffect.Effects.Add(multipassEffect);
            screenEffect.SurfaceFormat = hdrFormat;
            return screenEffect;
        }

        /// <summary>
        /// Creates a luminance chain for computing the luminance of the scene.
        /// </summary>
        public static MultiPassScreenEffectPass CreateLuminanceChain(GraphicsDevice graphics, int scale, float? adoption)
        {
            MultiPassScreenEffectPass luminancePass = new MultiPassScreenEffectPass();

            int size = (int)MathHelper.Min(graphics.Viewport.Width / scale, graphics.Viewport.Height / scale);
            size = UpperPowerOfTwo(size);
            luminancePass.Effects.Add(new BasicScreenEffect()
            {
                Effect = new LuminanceEffect(graphics),
                RenderTargetSize = Vector2.One * size,
                SurfaceFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Vector2,
            });
            size = Math.Max(1, size / scale);

            while (size >= 1)
            {
                luminancePass.Effects.Add(new BasicScreenEffect()
                {
                    Effect = new ScaleEffect(graphics),
                    RenderTargetSize = Vector2.One * size,
                });
                size /= scale;
            }

            if (adoption.HasValue)
            {
                luminancePass.Effects.Add(new AdoptionEffect(graphics)
                {
                    Speed = adoption.Value,
                    RenderTargetSize = Vector2.One,
                });
            }

            luminancePass.RenderTargetSize = Vector2.One;
            luminancePass.SurfaceFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Vector2;
            luminancePass.TextureUsage = TextureUsage.Luminance;
            return luminancePass;
        }

        private static int UpperPowerOfTwo(int v)
        {
            v--;    
            v |= v >> 1;   
            v |= v >> 2;
            v |= v >> 4;   
            v |= v >> 8;   
            v |= v >> 16;   
            v++;
            return v;
        }

        /// <summary>
        /// Creates a depth of field post processing effect.
        /// </summary>
        public static ScreenEffect CreateDepthOfField(GraphicsDevice graphics, float blurAmount, float focalPlane, float focalLength, float focalDistance)
        {
            MultiPassScreenEffectPass basePass = new MultiPassScreenEffectPass();

            MultiPassScreenEffectPass blurPass = new MultiPassScreenEffectPass();
            blurPass.Effects.Add(new ScaleEffect(graphics), 0.5f);
            blurPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction = 0 });
            blurPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction = MathHelper.PiOver2 });
            blurPass.Effects.Add(new ScaleEffect(graphics), 2);
            blurPass.TextureUsage = TextureUsage.Blur;

            MultiPassScreenEffect multipassEffect = new MultiPassScreenEffect(graphics);
            multipassEffect.Passes.Add(basePass);
            multipassEffect.Passes.Add(blurPass);
            multipassEffect.CombineEffect = new DepthOfFieldEffect(graphics) { FocalPlane = focalPlane, FocalLength = focalLength, FocalDistance = focalDistance };

            ScreenEffect screenEffect = new ScreenEffect(graphics);
            screenEffect.Effects.Add(multipassEffect);
            return screenEffect;
        }
#endif
        /// <summary>
        /// Creates a post processing effect that additively blends the scene.
        /// </summary>
        public static ScreenEffect CreateAdditive(GraphicsDevice graphics, float brightness)
        {
            ScreenEffect screenEffect = new ScreenEffect(graphics);
            MultiPassScreenEffect multipassEffect = new MultiPassScreenEffect(graphics);

            multipassEffect.Passes.Add(new MultiPassScreenEffectPass() { BlendState = BlendState.Opaque });
            multipassEffect.Passes.Add(new MultiPassScreenEffectPass() { BlendState = BlendState.Additive, Color = Color.White * brightness });

            screenEffect.Effects.Add(multipassEffect);
            return screenEffect;
        }

        /// <summary>
        /// Creates a basic post processing effect.
        /// </summary>
        public static ScreenEffect CreateEffect(Effect effect)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");

            ScreenEffect screenEffect = new ScreenEffect(effect.GraphicsDevice);
            screenEffect.Effects.Add(effect);
            return screenEffect;
        }

        /// <summary>
        /// Creates a merged post processing effect from a list of existing effects.
        /// </summary>
        public static ScreenEffect CreateMerged(GraphicsDevice graphics, params IScreenEffect[] effects)
        {
            return CreateMerged(graphics, (IEnumerable<IScreenEffect>)effects);
        }

        /// <summary>
        /// Creates a merged post processing effect from a list of existing effects.
        /// </summary>
        public static ScreenEffect CreateMerged(GraphicsDevice graphics, IEnumerable<IScreenEffect> effects)
        {
            SurfaceFormat hdrFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat.HdrBlendable;

            ScreenEffect screenEffect = new ScreenEffect(graphics);
            foreach (var effect in effects)
            {
                screenEffect.Effects.Add(effect);

                // Special walkaround for Hdr surface format
                ScreenEffect hdr = effects as ScreenEffect;
                if (hdr != null && hdr.SurfaceFormat == hdrFormat)
                {
                    screenEffect.SurfaceFormat = hdr.SurfaceFormat;
                }
            }
            return screenEffect;
        }
    }
}

