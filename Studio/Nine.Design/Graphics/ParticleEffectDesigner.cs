namespace Nine.Graphics.Design
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Studio;
    using Nine.Studio.Extensibility;
    using Nine.Studio.Visualizers;
    using Nine.Graphics.ParticleEffects;

    [Export(typeof(IFactory))]
    [LocalizedCategory("Graphics")]
    [LocalizedDisplayName("ParticleEffect", typeof(Nine.Design.Resources))]
    public class ParticleEffectFactory : Factory<ParticleEffect, object>
    {

    }

    [Export(typeof(IVisualizer))]
    [ExportMetadata(IsDefault=true)]
    public class ParticleEffectVisualizer : GraphicsVisualizer<ParticleEffect>
    {
        [EditorBrowsable()]
        [LocalizedDisplayName("ShowWireframe")]
        public bool ShowWireframe { get; set; }

        public ParticleEffectVisualizer()
        {
            DisplayName = string.Format(Strings.ViewFormat, Nine.Design.Resources.ParticleEffect);
        }

        protected override void Draw(float elapsedTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
        }
    }
}
