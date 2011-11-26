#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Test;
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
    }
}
