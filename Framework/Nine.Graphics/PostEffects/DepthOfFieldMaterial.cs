namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class DepthOfFieldMaterial
    {
        public float FocalPlane { get; set; }
        public float FocalLength { get; set; }
        public float FocalDistance { get; set; }

        partial void OnCreated()
        {
            FocalPlane = 20;
            FocalLength = 20;
            FocalDistance = 50;
        }
        
        partial void BeginApplyLocalParameters(DrawingContext context, DepthOfFieldMaterial previousMaterial)
        {
            Texture2D depthBuffer;

            GraphicsDevice.Textures[0] = texture;
            GraphicsDevice.Textures[2] = depthBuffer = context.textures[TextureUsage.DepthBuffer] as Texture2D;
            GraphicsDevice.SamplerStates[0] = GraphicsDevice.SamplerStates[1] = GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
                        
            var projectionParams = new Vector4();
            projectionParams.X = context.matrices.projection.M43;
            projectionParams.Y = context.matrices.projection.M33;
            projectionParams.Z = context.matrices.projection.M34;

            var focalParams = new Vector4();
            focalParams.X = FocalPlane;
            focalParams.Y = FocalLength;
            focalParams.Z = FocalDistance;

            if (depthBuffer != null)
            {
                projectionParams.W = 1f / depthBuffer.Width;
                focalParams.W = 1f / depthBuffer.Height;
            }

            effect.ProjectionParams.SetValue(projectionParams);
            effect.FocalParams.SetValue(focalParams);
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
        }
    }
}