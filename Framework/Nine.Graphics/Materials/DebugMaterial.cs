namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Design;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DebugMaterial : Material
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public TextureUsage TextureUsage { get; set; }

        [TypeConverter(typeof(SamplerStateConverter))]
        public SamplerState SamplerState { get; set; }

        private BasicEffect effect;

        public DebugMaterial(GraphicsDevice graphics)
        {
            effect = GraphicsResources<BasicEffect>.GetInstance(graphics);
            GraphicsDevice = graphics;
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
                GraphicsDevice.SamplerStates[0] = context.Settings.DefaultSamplerState;
        }
    }
}