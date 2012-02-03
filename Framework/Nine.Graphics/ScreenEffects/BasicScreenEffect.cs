#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ScreenEffects
{
    /// <summary>
    /// Defines an basic screen effect that uses only one <c>Effect</c>.
    /// </summary>
    public class BasicScreenEffect : IScreenEffect, IUpdateable, IEffectTexture
    {
        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice 
        {
            get
            {
#if SILVERLIGHT
                return System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice;
#else
                return Effect != null ? Effect.GraphicsDevice : null; 
#endif
            } 
        }

        /// <summary>
        /// Gets or sets the effect file used by this <c>BasicScreenEffect</c>.
        /// </summary>
        [ContentSerializerIgnore]
        public Effect Effect { get; set; }

        [ContentSerializer(ElementName="Effect")]
        internal object EffectSerializer
        {
            get { return Effect; }
            set { Effect = value as Effect; }
        }

        /// <summary>
        /// Gets or sets whether this <c>BasicScreenEffect</c> is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the surface format of the render target.
        /// Specify null to use the surface format of the current backbuffer.
        /// </summary>
        public SurfaceFormat? SurfaceFormat { get; set; }

        /// <summary>
        /// Gets or sets the render target size.
        /// Specify null to use input texture size.
        /// </summary>
        public Vector2? RenderTargetSize { get; set; }

        /// <summary>
        /// Gets or sets the render target scale. This value is multiplied with
        /// <c>RenderTargetSize</c> to determine the final size of the render target.
        /// </summary>
        public float RenderTargetScale { get; set; }

        /// <summary>
        /// Creates a new instance of <c>ScreenEffectEdge</c>.
        /// </summary>
        public BasicScreenEffect()
        {
            Enabled = true;
            RenderTargetScale = 1;
        }
        
        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            IEffectTexture update = Effect as IEffectTexture;
            if (update != null)
                update.SetTexture(usage, texture);
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Texture2D Process(Texture2D input)
        {
            if (!Enabled || Effect == null)
                return input;

            RenderTargetPool.AddRef(input as RenderTarget2D);
            RenderTarget2D renderTarget = RenderTargetPool.AddRef(GraphicsDevice, input, RenderTargetSize, RenderTargetScale, SurfaceFormat);
            renderTarget.Begin();

            GraphicsDevice.DrawFullscreenQuad(input, SamplerState.PointClamp, BlendState.Opaque, Color.White, Effect);

            RenderTargetPool.Release(renderTarget);
            RenderTargetPool.Release(input as RenderTarget2D);
            return renderTarget.End();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ProcessAndDraw(Texture2D input)
        {
            if (Enabled)
            {
                GraphicsDevice.DrawFullscreenQuad(input, SamplerState.PointClamp, BlendState.Opaque, Color.White, Effect);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void Update(TimeSpan elapsedTime)
        {
            if (Enabled && Effect is IUpdateable)
                ((IUpdateable)Effect).Update(elapsedTime);
        }
    }

    /// <summary>
    /// Represents a collection of <c>ScreenEffectEdge</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ScreenEffectCollection : Collection<IScreenEffect>
    {
        internal ScreenEffectCollection() { }

        public void Add(Effect effect)
        {
            Add(new BasicScreenEffect() { Effect = effect });
        }

        public void Add(Effect effect, float renderTargetScale)
        {
            Add(new BasicScreenEffect() { Effect = effect, RenderTargetScale = renderTargetScale });
        }
    }
}

