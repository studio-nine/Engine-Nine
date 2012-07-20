namespace Nine.Content.Pipeline.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Nine.Content.Pipeline.Xaml;
    using Nine.Graphics.ParticleEffects;

    [TestClass]
    public class XamlSerializerTest : GraphicsTest
    {
        [TestMethod]
        public void SaveParticleEffectTest()
        {
            var particleEffect = new ParticleEffect(GraphicsDevice);
            new XamlSerializer().Save("ParticleEffect.xaml", particleEffect);
        }
    }
}
