namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DebugMaterial : Material
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public TextureUsage TextureUsage { get; set; }

#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public SamplerState SamplerState { get; set; }

        private BasicEffect effect;
        private static Dictionary<GraphicsDevice, BasicEffect> cachedEffects = new Dictionary<GraphicsDevice, BasicEffect>();

        public DebugMaterial(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            if (!cachedEffects.TryGetValue(graphics, out effect))
            {
                cachedEffects.Add(graphics, effect = new BasicEffect(graphics)
                {
                    LightingEnabled = false,
                    FogEnabled = false,
                    VertexColorEnabled = false
                });
            }
        }

        protected override void OnBeginApply(DrawingContext context, Material previousMaterial)
        {
            if (effect.TextureEnabled = (TextureUsage != TextureUsage.None))
                effect.Texture = context.textures[TextureUsage] as Texture2D;
            effect.CurrentTechnique.Passes[0].Apply();
            if (SamplerState != null)
                GraphicsDevice.SamplerStates[0] = SamplerState;
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (SamplerState != null)
            {
                GraphicsDevice.Textures[0] = null;
                GraphicsDevice.SamplerStates[0] = context.SamplerState;
            }
        }
    }
}