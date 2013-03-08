namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using Nine.Graphics.Drawing;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework;

    [Nine.Serialization.NotBinarySerializable]
    partial class SoftParticleMaterial
    {
        public float DepthFade
        {
            get { return depthFade; }
            set { depthFade = value; }
        }
        private float depthFade = Constants.SoftParticleFade;

        public override void GetDependentPasses(ICollection<Type> passTypes)
        {
            // TODO:
            //passTypes.Add(typeof(DepthPrePass));
        }

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            effect.Projection.SetValue(context.Projection);
            effect.projectionInverse.SetValue(context.matrices.ProjectionInverse);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, SoftParticleMaterial previousMaterial)
        {
            context.graphics.Textures[0] = texture;
            context.graphics.Textures[1] = context.textures[TextureUsage.DepthBuffer];
            context.graphics.SamplerStates[1] = SamplerState.PointClamp;

            Matrix worldView;
            Matrix.Multiply(ref world, ref context.matrices.view, out worldView);
            effect.worldView.SetValue(worldView);
            effect.DepthFade.SetValue(depthFade);
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            context.graphics.Textures[1] = null;
            context.graphics.SamplerStates[1] = context.SamplerState;
        }
    }
}
