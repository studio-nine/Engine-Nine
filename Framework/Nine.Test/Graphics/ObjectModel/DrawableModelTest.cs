#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.ObjectModel;
using Nine.Graphics.Effects;

namespace Nine.Graphics.ObjectModel.Test
{
    [TestClass]
    [DeploymentItem(@"..\Samples\Content\Models\Dude\dude.fbx")]
    public class DrawableModelTest : ContentPipelineTest
    {
        [TestMethod()]
        public void DrawableModelMaterialTest()
        {
            Test(() =>
            {
                BuildModel("dude.fbx");
                RunTheBuild();

                var dude = new DrawableModel(Content.Load<Model>("dude"));

                Assert.IsInstanceOfType(dude.Material, typeof(SkinnedMaterial));
                Assert.AreEqual(true, dude.IsAnimated);
                Assert.AreEqual(true, dude.IsSkinned);
                Assert.AreEqual(1, dude.Alpha);
                Assert.AreEqual(1, dude.Animations.Animations.Count);
                Assert.AreEqual(true, dude.LightingEnabled);
                Assert.AreEqual(true, dude.CastShadow);
                Assert.AreEqual(false, dude.ReceiveShadow);
                Assert.AreEqual(true, dude.Visible);

                Assert.IsTrue(dude.ModelParts.Count > 1);
                foreach (var modelPart in dude.ModelParts)
                {
                    Assert.IsInstanceOfType(modelPart.Material, typeof(SkinnedMaterial));
                    Assert.AreNotEqual(dude.Material, modelPart.Material);
                }

                // Change material
                dude.Material = new BasicMaterial(GraphicsDevice) { Alpha = 0.5f };
                foreach (var modelPart in dude.ModelParts)
                {
                    Assert.IsInstanceOfType(modelPart.Material, typeof(BasicMaterial));
                    Assert.AreNotEqual(dude.Material, modelPart.Material);
                    Assert.AreEqual(0.5f, ((BasicMaterial)modelPart.Material).Alpha);
                }

                // Set material to null
                dude.Material = null;
                Assert.IsInstanceOfType(dude.Material, typeof(SkinnedMaterial));
                foreach (var modelPart in dude.ModelParts)
                {
                    Assert.IsInstanceOfType(modelPart.Material, typeof(SkinnedMaterial));
                    Assert.AreNotEqual(dude.Material, modelPart.Material);
                }

                // Change part material
                dude.ModelParts[0].Material = new BasicMaterial(GraphicsDevice);
                for (int i = 1; i < dude.ModelParts.Count; i++)
                {
                    var modelPart = dude.ModelParts[i];
                    Assert.IsInstanceOfType(modelPart.Material, typeof(SkinnedMaterial));
                    Assert.AreNotEqual(dude.Material, modelPart.Material);
                }
                Assert.IsInstanceOfType(dude.ModelParts[0].Material, typeof(BasicMaterial));
            }); 
        }

        [TestMethod()]
        public void DrawableModelMaterialContentPipelineTest()
        {
            Test(() =>
            {
                var content = new DrawableModelContent();
                content.Model = "Dude";
                content.ModelParts = new []
                {
                    new DrawableModelPartContent
                    {
                        Material = new DualTextureMaterialContent(),
                    }
                };

                BuildModel("dude.fbx");
                var model = BuildObjectUsingDefaultContentProcessor(content);
                RunTheBuild();

                var dude = Content.Load<DrawableModel>(model);

                Assert.IsInstanceOfType(dude.Material, typeof(SkinnedMaterial));
                Assert.IsTrue(dude.ModelParts.Count > 1);
                Assert.IsInstanceOfType(dude.ModelParts[0].Material, typeof(DualTextureMaterial));
            });
        }
    }
}
