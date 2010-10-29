using Nine.Animations;
using Microsoft.Xna.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Nine.Animations.Test
{
    /// <summary>
    ///This is a test class for KeyframeAnimationTest and is intended
    ///to contain all KeyframeAnimationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KeyframeAnimationTest
    {
        class TestAnimation : KeyframeAnimation
        {
            public TestAnimation()
            {
                Repeat = 1;
                FramesPerSecond = 1;
            }

            protected override int GetTotalFrames()
            {
                return 5;
            }

            protected override void OnSeek(int startFrame, int endFrame, float percentage)
            {
                Assert.IsTrue(percentage >= 0 && percentage <= 1);
                Assert.IsTrue(startFrame >= 0 && startFrame < TotalFrames);
                Assert.IsTrue(endFrame >= 0 && endFrame < TotalFrames);
            }
        }

        [TestMethod()]
        public void DurationTest()
        {
            TestAnimation animation = new TestAnimation();
            Assert.AreEqual<TimeSpan>(TimeSpan.FromSeconds(5), animation.Duration);
        }

        [TestMethod()]
        public void IsPlayingTest()
        {
            TestAnimation animation = new TestAnimation();
            Assert.AreNotEqual<AnimationState>(AnimationState.Playing, animation.State);

            animation.Play();
            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(5)));
            Assert.AreNotEqual<AnimationState>(AnimationState.Playing, animation.State);
        }

        [TestMethod()]
        public void DirectionTest()
        {
            TestAnimation animation = new TestAnimation();
            
            animation.StartupDirection = AnimationDirection.Backward;
            animation.Play();
            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2)));

            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);
            Assert.AreEqual<TimeSpan>(TimeSpan.FromSeconds(3), animation.ElapsedTime);
        }

        [TestMethod()]
        public void RepeatTest()
        {
            TestAnimation animation = new TestAnimation();

            animation.Repeat = 2.5f;
            animation.Play();

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(12.4)));
            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));
            Assert.AreNotEqual<AnimationState>(AnimationState.Playing, animation.State);
        }

        [TestMethod()]
        public void AutoReverseTest()
        {
            TestAnimation animation = new TestAnimation();

            animation.Repeat = 2.25f;
            animation.AutoReverse = true;
            animation.Play();

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(6)));
            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);
            Assert.AreEqual<TimeSpan>(TimeSpan.FromSeconds(4), animation.ElapsedTime);

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(5)));
            Assert.AreEqual<TimeSpan>(TimeSpan.FromSeconds(1), animation.ElapsedTime);
            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.5)));
            Assert.AreNotEqual<AnimationState>(AnimationState.Playing, animation.State);
        }

        [TestMethod()]
        public void SeekTest()
        {
            TestAnimation animation = new TestAnimation();

            animation.Play();

            animation.Seek(TimeSpan.FromSeconds(4.5f));
            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);
            Assert.AreEqual<TimeSpan>(TimeSpan.FromSeconds(4.5), animation.ElapsedTime);

            animation.Seek(2);
            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);
            Assert.AreEqual<TimeSpan>(TimeSpan.FromSeconds(2), animation.ElapsedTime);

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(3)));
            Assert.AreNotEqual<AnimationState>(AnimationState.Playing, animation.State);

            animation.StartupDirection = AnimationDirection.Backward;
            animation.Play();
            animation.Seek(1);
            Assert.AreEqual<AnimationState>(AnimationState.Playing, animation.State);

            animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1)));
            Assert.AreNotEqual<AnimationState>(AnimationState.Playing, animation.State);
        }

        [TestMethod()]
        public void EnterFrameTest()
        {
            TestAnimation animation = new TestAnimation();

            int index = 0;

            animation.EnterFrame += (o, e) =>
                {
                    if (index < animation.TotalFrames)
                        Assert.AreEqual(index, e.Frame);
                    index++;
                };
            animation.Play();

            for (int i = 0; i < 20; i++)
                animation.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.25)));

            Assert.AreNotEqual<AnimationState>(AnimationState.Playing, animation.State);
            Assert.AreEqual(6, index);
        }
    }
}
