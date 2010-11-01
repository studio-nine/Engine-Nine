#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Nine.Graphics;
using Nine.Animations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Nine.Graphics.Test
{
    /// <summary>
    ///This is a test class for BoneAnimationTest and is intended
    ///to contain all BoneAnimationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BoneAnimationTest
    {
        /// <summary>
        ///A test for Play
        ///</summary>
        [TestMethod()]
        public void PlayTest()
        {
            BoneAnimation target = new BoneAnimation();

            Assert.AreEqual<AnimationState>(AnimationState.Playing, target.State);
            Assert.AreEqual<TimeSpan>(TimeSpan.Zero, target.Duration);

            target.Stop();

            Assert.AreEqual<AnimationState>(AnimationState.Stopped, target.State);

            target.Play();

            Assert.AreEqual<AnimationState>(AnimationState.Playing, target.State);

            target.Pause();

            Assert.AreEqual<AnimationState>(AnimationState.Paused, target.State);

            target.Resume();

            Assert.AreEqual<AnimationState>(AnimationState.Playing, target.State);
        }
    }
}
