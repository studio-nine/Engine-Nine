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
    public class ModelMaterialTest : MaterialTest
    {
        [TestMethod()]
        public void ModelMaterialDefaultTest()
        {
            MaterialEquals(ErrorCap
                , new BasicMaterialContent()
                , new ModelMaterialContent());
        }

        [TestMethod()]
        public void ModelMaterialNoLightingTest()
        {
            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { TextureEnabled = false, LightingEnabled = false }
                , new ModelMaterialContent() { TextureEnabled = false, LightingEnabled = false });
        }

        [TestMethod()]
        public void ModelMaterialNoMaterialTest()
        {
            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { TextureEnabled = false }
                , new ModelMaterialContent() { TextureEnabled = false, MaterialEnabled = false });
        }

        [TestMethod()]
        public void ModelMaterialTextureTest()
        {
            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { TextureEnabled = false }
                , new ModelMaterialContent() { TextureEnabled = false }

                , new BasicMaterialContent() { TextureEnabled = true }
                , new ModelMaterialContent() { TextureEnabled = true }

                , new BasicMaterialContent() { Texture = "box" }
                , new ModelMaterialContent() { Texture = "box" });
        }

        [TestMethod()]
        public void ModelMaterialAlphaTest()
        {
            var alpha = RandomFloat();

            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box", Alpha = alpha }
                , new ModelMaterialContent() { Texture = "box", Alpha = alpha });
        }

        [TestMethod()]
        public void ModelMaterialAlphaNotEqualTest()
        {
            MaterialNotEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box", Alpha = 0.2f }
                , new ModelMaterialContent() { Texture = "box", Alpha = 0.8f });
        }

        [TestMethod()]
        public void ModelMaterialColorTest()
        {
            var color = RandomColor().ToVector3();

            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box", DiffuseColor = color }
                , new ModelMaterialContent() { Texture = "box", DiffuseColor = color }

                , new BasicMaterialContent() { Texture = "box", EmissiveColor = color }
                , new ModelMaterialContent() { Texture = "box", EmissiveColor = color });
        }

        [TestMethod()]
        public void ModelMaterialColorNotEqualTest()
        {
            MaterialNotEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box", DiffuseColor = Vector3.One * 0.1f }
                , new ModelMaterialContent() { Texture = "box", DiffuseColor = Vector3.One * 0.7f }

                , new BasicMaterialContent() { Texture = "box", EmissiveColor = Vector3.One * 0.1f }
                , new ModelMaterialContent() { Texture = "box", EmissiveColor = Vector3.One * 0.7f });
        }

        [TestMethod()]
        public void ModelMaterialSpecularTest()
        {
            var specularPower = RandomFloat() * 64;
            var color = RandomColor().ToVector3();
            LightSpecularColor = RandomColor().ToVector3();

            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box", SpecularColor = color, SpecularPower = specularPower }
                , new ModelMaterialContent() { Texture = "box", SpecularColor = color, SpecularPower = specularPower });
        }

        [TestMethod()]
        public void ModelMaterialSpecularNotEqualTest()
        {
            var random = new Random();
            var color = RandomColor().ToVector3();
            LightSpecularColor = Vector3.One;

            MaterialNotEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box", SpecularColor = color, SpecularPower = 1 }
                , new ModelMaterialContent() { Texture = "box", SpecularColor = color, SpecularPower = 128 });
        }

        [TestMethod()]
        public void ModelMaterialEmissiveMapTest()
        {
            var color = RandomColor();
            var map = BuildTexture(color);

            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box", EmissiveColor = color.ToVector3() }
                , new ModelMaterialContent() { Texture = "box", EmissiveMappingEnabled = true, EmissiveMap = map });
        }

        [TestMethod()]
        public void ModelMaterialNormalMapTest()
        {
            var map = BuildTexture(new Color(0, 0, 1.0f));

            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box" }
                , new ModelMaterialContent() { Texture = "box", NormalMappingEnabled = true, NormalMap = map });
        }

        [TestMethod()]
        public void ModelMaterialNormalMapNotEqualTest()
        {
            var map = BuildTexture(new Color(1.0f, 0, 0));

            MaterialNotEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box" }
                , new ModelMaterialContent() { Texture = "box", NormalMappingEnabled = true, NormalMap = map });
        }

        [TestMethod()]
        public void ModelMaterialFogTest()
        {
            FogEnabled = true;

            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box" }
                , new ModelMaterialContent() { Texture = "box", FogEnabled = true });
        }

        [TestMethod()]
        public void ModelMaterialTextureAlphaUsageTest()
        {
            var opacity = BuildTexture(new Color(1.0f, 1.0f, 1.0f, RandomFloat()));

            var overlayIntensity = RandomFloat();
            var overlay = BuildTexture(new Color(1.0f, 1.0f, 1.0f, overlayIntensity), Parameters("PremultiplyAlpha", false));
            var overlayColor = RandomColor();
            var overlayResult = BuildTexture(Color.Lerp(Color.White, overlayColor, overlayIntensity));

            var specularColor = RandomFloat();
            var specular = BuildTexture(new Color(1.0f, 1.0f, 1.0f, specularColor), Parameters("PremultiplyAlpha", false));
            var white = BuildTexture(Color.White);

            MaterialEquals(0.8f // Overlay color may vary
                , new BasicMaterialContent() { Texture = opacity }
                , new ModelMaterialContent() { Texture = opacity, TextureAlphaUsage = TextureAlphaUsage.Opacity }

                , new BasicMaterialContent() { Texture = overlayResult }
                , new ModelMaterialContent() { Texture = overlay, TextureAlphaUsage = TextureAlphaUsage.Overlay, OverlayColor = overlayColor.ToVector3() }

                , new BasicMaterialContent() { Texture = white, SpecularColor = specularColor * Vector3.One }
                , new ModelMaterialContent() { Texture = specular, SpecularColor = Vector3.One, TextureAlphaUsage = TextureAlphaUsage.Specular });
        }
    }
}
