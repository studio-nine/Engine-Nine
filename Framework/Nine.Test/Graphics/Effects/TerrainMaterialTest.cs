#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nine.Content.Pipeline.Graphics.Effects;

namespace Nine.Graphics.Effects.Test
{
    [TestClass]
    public class TerrainMaterialTest : MaterialTest
    {
        [TestMethod()]
        public void TerrainMaterialDefaultTest()
        {
            MaterialEquals(ErrorCap
                , new BasicMaterialContent() { TextureEnabled = false }
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
