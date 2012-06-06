#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace Nine.Content.Pipeline.Test
{
    [TestClass]
    public class PipelineBuilderTest
    {
        [TestMethod]
        [DeploymentItem(@"..\Samples\Content\Textures\box.dds")]
        public void LoadTextureTest()
        {
            var texture = PipelineBuilder.Load<Texture2D>("box.dds");

            Assert.AreEqual(512, texture.Width);
            Assert.AreEqual(512, texture.Height);
        }

        [TestMethod]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\AlphaPalm.x")]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\Palm.tga")]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\PalmLeave.tga")]
        public void LoadModelTest()
        {
            var model = PipelineBuilder.Load<Model>("AlphaPalm.x");

            Assert.AreEqual(2, model.Meshes.Count);
        }

        [TestMethod]
        [DeploymentItem(@"Nine.Test\Content\TestWorld.xaml")]
        [DeploymentItem(@"..\Samples\Content\Models\Terrain\SkyCubeMap.dds")]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\AlphaPalm.x")]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\Palm.tga")]
        [DeploymentItem(@"..\Samples\Content\Models\Palm\PalmLeave.tga")]
        public void LoadWorldTest()
        {
            var cubeMap = PipelineBuilder.Load<TextureCube>("SkyCubeMap.dds");
            var world = PipelineBuilder.Load<World>("TestWorld.xaml");

            Assert.IsNotNull(world);
            //Assert.AreEqual(1, world.WorldObjects.Count);
        }
    }
}
