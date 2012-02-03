#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;

namespace Nine.Graphics.ParticleEffects.Test
{
    [TestClass()]
    public class ParticleEffectTest : ContentPipelineTest
    {
        [TestMethod()]
        public void DefaultConstructorTest()
        {
            Test(() =>
            {
                var effect = new ParticleEffect();

                Assert.AreEqual(true, effect.Enabled);
                Assert.AreEqual(false, effect.DepthSortEnabled);
                Assert.AreEqual(BlendState.Additive, effect.BlendState);
                Assert.AreEqual(0, effect.ActiveEmitters.Count);
                Assert.IsNotNull(effect.Emitter);

                effect.Update(ElapsedTime);

                bool hasParticle = false;
                effect.ForEach((ref Particle particle) => { hasParticle = true; });

                Assert.IsFalse(hasParticle);
            });
        }

        [TestMethod()]
        public void TriggerTest()
        {
            Test(() =>
            {
                var effect = new ParticleEffect();                
                effect.Trigger();
                Assert.AreEqual(1, effect.ActiveEmitters.Count);
                effect.Update(ElapsedTime);

                bool hasParticle = false;
                effect.ForEach((ref Particle particle) => { hasParticle = true; });

                Assert.IsTrue(hasParticle);
            });
        }
    }
}
