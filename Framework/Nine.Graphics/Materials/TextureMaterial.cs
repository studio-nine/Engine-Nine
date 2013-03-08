namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Represents the minimum basic material that only contains a texture and diffuse color.
    /// </summary>
    public class TextureMaterial : Material
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : Constants.DiffuseColor; }
            set { diffuseColor = (value == Constants.DiffuseColor ? (Vector3?)null : value); }
        }
        internal Vector3? diffuseColor;

        public bool VertexColorEnabled
        {
            get { return vertexColorEnabled; }
            set { vertexColorEnabled = value; }
        }
        private bool vertexColorEnabled;

#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public SamplerState SamplerState
        {
            get { return samplerState; }
            set { samplerState = value; }
        }
        private SamplerState samplerState;
        #endregion

        #region Fields
        private BasicEffect effect;
        private EffectPass pass;

        private static Texture2D previousTexture;
        #endregion

        #region Methods
        public TextureMaterial(GraphicsDevice graphics)
        {
            effect = GraphicsResources<BasicEffect>.GetInstance(graphics, typeof(TextureMaterial));
            effect.LightingEnabled = false;            
            pass = effect.CurrentTechnique.Passes[0];
            GraphicsDevice = graphics;
        }

        protected override void OnBeginApply(DrawingContext context, Material previousMaterial)
        {
            var previousTextureMaterial = previousMaterial as TextureMaterial;
            if (previousTextureMaterial == null)
            {
                effect.View = context.View;
                effect.Projection = context.Projection;
            }

            if (alpha != Constants.Alpha)
                effect.Alpha = alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = diffuseColor.Value;
            
            if (previousTextureMaterial == null || texture != previousTexture)
                previousTexture = effect.Texture = texture;

            effect.World = world;
            effect.TextureEnabled = texture != null;
            effect.VertexColorEnabled = vertexColorEnabled;

            // Finally apply the shader.
            pass.Apply();

            // Update sampler state
            if (samplerState != null)
                context.graphics.SamplerStates[0] = samplerState;
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (alpha != Constants.Alpha)
                effect.Alpha = Constants.Alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = Constants.DiffuseColor;

            if (samplerState != null)
                GraphicsDevice.SamplerStates[0] = context.SamplerState;
        }
        #endregion
    }
}