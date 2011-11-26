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
using Nine.Content.Pipeline;
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;

namespace Nine.Graphics.Effects.Test
{
    [TestClass]
    [DeploymentItem(@"..\Samples\Content\Fonts\Consolas.spritefont")]
    [DeploymentItem(@"..\Samples\Content\Textures\box.dds")]
    [DeploymentItem(@"..\Samples\Content\Textures\box_n.dds")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\dude.fbx")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\head.tga")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\head_n.dds")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\jacket.tga")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\jacket_n.dds")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\jacket_s.dds")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\pants.tga")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\pants_n.dds")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\pants_s.dds")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\upBodyC.tga")]
    //[DeploymentItem(@"..\Samples\Content\Models\Dude\upBodyC_n.dds")]
    public class MaterialTest : ContentPipelineTest
    {
        public const double ErrorCap = 0.1;

        protected bool FogEnabled = false;
        protected Vector3 FogColor = Vector3.One;
        protected Vector3 LightDiffuseColor = Vector3.One;
        protected Vector3 LightSpecularColor = Vector3.Zero;

        [TestInitialize]
        public override void Initialize()
        {
            FogEnabled = false;
            FogColor = Vector3.One;
            LightDiffuseColor = Vector3.One;
            LightSpecularColor = Vector3.Zero;

            base.Initialize();
        }

        protected void ShowMaterial(params object[] materialContents)
        {
            Test(() =>
            {
                Queue<string> assets = new Queue<string>();
                foreach (var materialContent in materialContents)
                    assets.Enqueue(BuildObjectUsingDefaultContentProcessor(materialContent));
                BuildFont("Consolas.spritefont");
                BuildTexture("box.dds");
                BuildNormalMap("box_n.dds");
                RunTheBuild();

                var spriteBatch = new SpriteBatch(GraphicsDevice);
                var scene = new Scene(GraphicsDevice);
                var surface = new DrawableSurface(GraphicsDevice, 1, 64, 64, 8) { Position = new Vector3(-32, -32, 0) };
                surface.ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(InitializeSurfaceVertices);
                scene.Camera = new TopDownEditorCamera(GraphicsDevice) { Radius = 100 };
                scene.Add(surface);
                scene.Add(new Nine.Graphics.ObjectModel.DirectionalLight(GraphicsDevice));

                foreach (var materialContent in materialContents)
                {
                    surface.Material = Content.Load<Material>(assets.Dequeue());

                    SaveScreenShot(materialContent.GetType().Name, () =>
                    {
                        scene.Draw(TimeSpan.Zero);

                        DrawDescription(spriteBatch, materialContent);
                    });
                }
                scene.Dispose();
                spriteBatch.Dispose();
            });
        }

        private void InitializeSurfaceVertices(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionNormalTangentBinormalTexture output)
        {
            output.Position = input.Position;
            output.Normal = input.Normal;
            output.TextureCoordinate = input.TextureCoordinate;
            output.Tangent = Vector3.UnitX;
            output.Binormal = Vector3.UnitY;
        }

        private void DrawDescription(SpriteBatch spriteBatch, object materialContent)
        {
            var font = Content.Load<SpriteFont>("Consolas");
            spriteBatch.Begin();

            var scale = 1f;
            var height = font.MeasureString("X").Y * scale;
            Vector2 position = new Vector2(10, 10);
            spriteBatch.DrawString(font, materialContent.GetType().Name, position, Color.Yellow, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            position += new Vector2(0, height + 2 * scale);

            var defaultObject = Activator.CreateInstance(materialContent.GetType());
            foreach (var property in materialContent.GetType().GetProperties())
            {
                var defaultValue = property.GetValue(defaultObject, null);
                var value = property.GetValue(materialContent, null);
                if (value != null && !value.Equals(defaultValue) &&
                   (value.GetType().IsPrimitive || value.GetType().IsValueType || value.GetType().IsEnum || value is string))
                {
                    string text = string.Format("{0}: {1}", property.Name, value.ToString());
                    spriteBatch.DrawString(font, text, position + Vector2.One, Color.Gray, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, text, position, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    position += new Vector2(0, height + 2 * scale);
                }
            }
            spriteBatch.End();
        }

        protected void MaterialEquals(double errorCap, params object[] materialContents)
        {
            MaterialEqualityTest(true, errorCap, materialContents);
        }

        protected void MaterialNotEquals(double errorCap, params object[] materialContents)
        {
            MaterialEqualityTest(false, errorCap, materialContents);
        }

        /// <param name="materialContents">Material contents to be tested in pair.</param>
        private void MaterialEqualityTest(bool equals, double errorCap, params object[] materialContents)
        {
            Test(() =>
            {
                Queue<string> assets = new Queue<string>();
                foreach (var materialContent in materialContents)
                    assets.Enqueue(BuildObjectUsingDefaultContentProcessor(materialContent));
                foreach (var materialContent in materialContents)
                    assets.Enqueue(BuildObjectUsingDefaultContentProcessor(GetDeferredMaterialContent(materialContent)));
                BuildFont("Consolas.spritefont");
                BuildTexture("box.dds");
                BuildNormalMap("box_n.dds");
                RunTheBuild();

                var spriteBatch = new SpriteBatch(GraphicsDevice);
                var scene = new Scene(GraphicsDevice);
                var surface = new DrawableSurface(GraphicsDevice, 1, 64, 64, 8) { Position = new Vector3(-32, -32, 0) };
                surface.ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(InitializeSurfaceVertices);
                var backgroundColor = RandomColor(0, 0.3f);
                scene.Settings.BackgroundColor = backgroundColor;
                scene.Camera = new TopDownEditorCamera(GraphicsDevice) { Radius = 100 };
                scene.Add(surface);
                scene.Add(new AmbientLight(GraphicsDevice) { AmbientLightColor = RandomColor(0, 0.2f).ToVector3() });
                scene.Add(new Nine.Graphics.ObjectModel.DirectionalLight(GraphicsDevice) 
                {
                    DiffuseColor = LightDiffuseColor,
                    SpecularColor = LightSpecularColor,
                });
                if (FogEnabled)
                {
                    scene.Add(new Fog() { FogColor = FogColor, FogEnabled = true, FogStart = 0.1f, FogEnd = 200.0f });
                }

                for (int i = 0; i < materialContents.Length /* * 2 */; i += 2)
                {
                    var material1 = Content.Load<Material>(assets.Dequeue());
                    var material2 = Content.Load<Material>(assets.Dequeue());

                    var file1 = material1.GetType().Name;
                    var file2 = material2.GetType().Name;

                    surface.Material = material1;
                    var result1 = SaveScreenShot(ref file1,
                        () => { scene.Draw(TimeSpan.Zero); },
                        () => { DrawDescription(spriteBatch, GetDeferredMaterialContent(materialContents[i % materialContents.Length], i >= materialContents.Length)); });

                    surface.Material = material2;
                    var result2 = SaveScreenShot(ref file2,
                        () => { scene.Draw(TimeSpan.Zero); },
                        () => { DrawDescription(spriteBatch, GetDeferredMaterialContent(materialContents[(i + 1) % materialContents.Length], i >= materialContents.Length)); });
                    
                    bool compareResult = TextureEquals(result1, result2, errorCap) ^ equals;
                    
                    result1.Dispose();
                    result2.Dispose();

                    if (compareResult)
                    {
#if DEBUG
                        ShowImage(file1);
                        ShowImage(file2);
#endif
                    }
                    Assert.IsFalse(compareResult);
                }
                scene.Dispose();
            });
        }

        private object GetDeferredMaterialContent(object materialContent)
        {
            var content = materialContent as BasicLinkedMaterialContent;
            if (content != null)
                content.PreferDeferredLighting = true;
            return materialContent;
        }

        private object GetDeferredMaterialContent(object materialContent, bool isDeferred)
        {
            var content = materialContent as BasicLinkedMaterialContent;
            if (content != null)
                content.PreferDeferredLighting = isDeferred;
            return materialContent;
        }

        private void ShowImage(string filename)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(filename);
            info.UseShellExecute = true;
            info.Verb = "open";
            System.Diagnostics.Process.Start(info);
        }

        private bool TextureEquals(Texture2D result1, Texture2D result2, double errorCap)
        {
            if (result1.Bounds != result2.Bounds)
                return false;
            if (result1.Format != result2.Format)
                return false;

            var length = result1.Width * result1.Height;
            var data1 = new Color[length];
            var data2 = new Color[length];

            result1.GetData<Color>(0, null, data1, 0, length);
            result2.GetData<Color>(0, null, data2, 0, length);

            double error = 0;
            for (int i = 0; i < length; i++)
                error += (long)(Math.Abs(data1[i].R - data2[i].R) + Math.Abs(data1[i].G - data2[i].G) + Math.Abs(data1[i].B - data2[i].B) + Math.Abs(data1[i].A - data2[i].A));
            error /= length;
            System.Diagnostics.Trace.WriteLine("Error Amount: " + error);
            return error <= errorCap;
        }
    }
}
