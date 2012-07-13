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
using Nine.Graphics.ParticleEffects;
using Nine.Content.Pipeline.Xaml;

namespace Nine.Content.Pipeline.Test
{
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
