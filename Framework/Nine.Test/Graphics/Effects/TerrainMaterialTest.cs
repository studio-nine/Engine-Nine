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
    public class TerrainMaterialTest : MaterialTest
    {
        [TestMethod()]
        public void TerrainMaterialDefaultTest()
        {
            MaterialEquals(ErrorCap
                , new BasicMaterialContent()
                , new TerrainMaterialContent());
        }

        [TestMethod()]
        public void TerrainMaterialLayerTest()
        {
            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { Texture = "box" }
                , new TerrainMaterialContent() 
                  { 
                      Layers = new List<TerrainLayerContent> 
                      {
                            new TerrainLayerContent { Texture = "box" }
                      }
                  });
        }
    }
}
