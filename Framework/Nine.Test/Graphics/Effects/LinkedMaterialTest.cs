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
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;

namespace Nine.Graphics.Effects.Test
{
    [TestClass]
    public class LinkedMaterialTest : ContentPipelineTest
    {
        [TestMethod()]
        public void LinkedEffectBuildDefaultTest()
        {
            Test(() =>
            {
                var linkedEffectContent = new LinkedEffectContent();
                linkedEffectContent.EffectParts.Add(new PositionNormalTextureEffectPartContent());
                linkedEffectContent.EffectParts.Add(new VertexTransformEffectPartContent());
                linkedEffectContent.EffectParts.Add(new VertexShaderOutputEffectPartContent());
                linkedEffectContent.EffectParts.Add(new BasicTextureEffectPartContent());
                linkedEffectContent.EffectParts.Add(new PixelShaderOutputEffectPartContent());

                var filename = BuildObject(linkedEffectContent, "LinkedEffectProcessor", null);
                RunTheBuild();

                var linkedEffect = Content.Load<LinkedEffect>(filename);
            }); 
        }

        [TestMethod()]
        public void LinkedEffectBuildIncludeMaterialTest()
        {
            Test(() =>
            {
                var linkedEffectContent = new LinkedEffectContent();
                linkedEffectContent.EffectParts.Add(new PositionNormalTextureEffectPartContent());
                linkedEffectContent.EffectParts.Add(new VertexTransformEffectPartContent());
                linkedEffectContent.EffectParts.Add(new VertexShaderOutputEffectPartContent());
                linkedEffectContent.EffectParts.Add(new MaterialEffectPartContent() { DiffuseColor = Vector3.One * 0.1f });
                linkedEffectContent.EffectParts.Add(new BasicTextureEffectPartContent());
                linkedEffectContent.EffectParts.Add(new PixelShaderOutputEffectPartContent());

                var filename = BuildObject(linkedEffectContent, "LinkedEffectProcessor", null);
                RunTheBuild();

                // Linked effect should not keep any material info
                var linkedEffect = Content.Load<LinkedEffect>(filename);
                Assert.AreEqual(Vector3.One * 0.1f, linkedEffect.Find<IEffectMaterial>().DiffuseColor);
            });
        }

        [TestMethod()]
        public void DuplicateLinkedMaterialTest()
        {
            Test(() =>
            {
                var linkedMaterialContent1 = new LinkedMaterialContent();
                linkedMaterialContent1.EffectParts.Add(new PositionNormalTextureEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new VertexTransformEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new VertexShaderOutputEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new BasicTextureEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new PixelShaderOutputEffectPartContent());

                var linkedMaterialContent2 = new LinkedMaterialContent();
                linkedMaterialContent2.EffectParts.Add(new PositionNormalTextureEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new VertexTransformEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new VertexShaderOutputEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new BasicTextureEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new PixelShaderOutputEffectPartContent());

                var filename1 = BuildObjectUsingDefaultContentProcessor(linkedMaterialContent1);
                var filename2 = BuildObjectUsingDefaultContentProcessor(linkedMaterialContent2);

                RunTheBuild();
                
                var linkedMaterial1 = Content.Load<LinkedMaterial>(filename1);
                var linkedMaterial2 = Content.Load<LinkedMaterial>(filename2);

                Assert.AreEqual(linkedMaterial1.Effect, linkedMaterial2.Effect);
            });
        }

        [TestMethod()]
        public void DuplicateLinkedMaterialWithDifferenceMaterialTest()
        {
            Test(() =>
            {
                var linkedMaterialContent1 = new LinkedMaterialContent();
                linkedMaterialContent1.EffectParts.Add(new PositionNormalTextureEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new VertexTransformEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new VertexShaderOutputEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new BasicTextureEffectPartContent());
                linkedMaterialContent1.EffectParts.Add(new MaterialEffectPartContent() { DiffuseColor = Vector3.One * 0.2f });
                linkedMaterialContent1.EffectParts.Add(new PixelShaderOutputEffectPartContent());

                var linkedMaterialContent2 = new LinkedMaterialContent();
                linkedMaterialContent2.EffectParts.Add(new PositionNormalTextureEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new VertexTransformEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new VertexShaderOutputEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new BasicTextureEffectPartContent());
                linkedMaterialContent2.EffectParts.Add(new MaterialEffectPartContent() { DiffuseColor = Vector3.One * 0.8f });
                linkedMaterialContent2.EffectParts.Add(new PixelShaderOutputEffectPartContent());

                var filename1 = BuildObjectUsingDefaultContentProcessor(linkedMaterialContent1);
                var filename2 = BuildObjectUsingDefaultContentProcessor(linkedMaterialContent2);

                RunTheBuild();

                var linkedMaterial1 = Content.Load<LinkedMaterial>(filename1);
                var linkedMaterial2 = Content.Load<LinkedMaterial>(filename2);

                Assert.AreNotEqual(linkedMaterial1, linkedMaterial2);
                Assert.AreEqual(linkedMaterial1.Effect, linkedMaterial2.Effect);
                Assert.AreEqual(Vector3.One * 0.2f, linkedMaterial1.DiffuseColor);
                Assert.AreEqual(Vector3.One * 0.8f, linkedMaterial2.DiffuseColor);
            });
        }
    }
}
