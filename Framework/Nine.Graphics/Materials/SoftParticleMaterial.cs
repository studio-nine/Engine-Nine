namespace Nine.Graphics.Materials
{
    using Nine.Graphics.Drawing;

    [NotContentSerializable]
    partial class SoftParticleMaterial
    {
        public float DepthFade
        {
            get { return depthFade.HasValue ? depthFade.Value : Constants.SoftParticleFade; }
            set { depthFade = (value == Constants.SoftParticleFade ? (float?)null : value); }
        }
        private float? depthFade;

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            effect.Projection.SetValue(context.Projection);
            effect.projectionInverse.SetValue(context.matrices.ProjectionInverse);
            effect.DepthBuffer.SetValue(context.textures[TextureUsage.DepthBuffer]);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, SoftParticleMaterial previousMaterial)
        {
            effect.Texture.SetValue(texture);
            if (depthFade.HasValue)
                effect.DepthFade.SetValue(depthFade.Value);
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            if (depthFade.HasValue)
                effect.DepthFade.SetValue(Constants.SoftParticleFade);
        }
    }
}
