namespace Nine.Content.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Serialization;

    [TestClass]
    public class PipelineBuilderTest
    {
        [TestMethod]
        [DeploymentItem(@"..\Samples\Content\Textures\box.dds")]
        public void LoadTextureTest()
        {
            var texture = new PipelineBuilder().Load<Texture2D>("box.dds");

            Assert.AreEqual(512, texture.Width);
            Assert.AreEqual(512, texture.Height);
        }

        [TestMethod]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\AlphaPalm.x")]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\Palm.tga")]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\PalmLeave.tga")]
        public void LoadModelTest()
        {
            var model = new PipelineBuilder().Load<Model>("AlphaPalm.x");

            Assert.AreEqual(2, model.Meshes.Count);
        }
    }
}
