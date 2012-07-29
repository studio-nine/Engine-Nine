namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class DepthOfFieldMaterial
    {
        public float FocalLength { get; set; }
        public float FocalPlane { get; set; }
        public float FocalDistance { get; set; }

        partial void OnCreated()
        {
            FocalDistance = 0.5f;
        }
        
        partial void BeginApplyLocalParameters(DrawingContext context, DepthOfFieldMaterial previousMaterial)
        {
            GraphicsDevice.Textures[0] = texture;
            GraphicsDevice.Textures[2] = context.textures[TextureUsage.DepthBuffer];

            effect.FocalDistance.SetValue(FocalDistance);
            effect.FocalLength.SetValue(FocalLength);
            effect.FocalPlane.SetValue(FocalPlane);
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            GraphicsDevice.Textures[1] = null;
            GraphicsDevice.Textures[2] = null;
            GraphicsDevice.SamplerStates[1] = GraphicsDevice.SamplerStates[2] = context.Settings.DefaultSamplerState;            
        }

        public override void GetDependentPasses(ICollection<Type> passTypes)
        {
            passTypes.Add(typeof(DepthPrePass));
        }

        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            if (textureUsage == TextureUsage.Blur)
                GraphicsDevice.Textures[1] = texture;
            else if (textureUsage == TextureUsage.DepthBuffer)
                GraphicsDevice.Textures[2] = texture;
        }
    }
}