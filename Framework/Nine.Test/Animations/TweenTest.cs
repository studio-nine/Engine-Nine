#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Nine.Animations;
using Microsoft.Xna.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Nine.Animations.Test
{
    [TestClass()]
    public class TweenTest
    {
        [TestMethod()]
        public void CompleteTest()
        {
            bool completed = false;
            TweenAnimation<float> tween = new TweenAnimation<float>()
            {
                From = -0.239572048f,
                To = 1.2223f,
                Duration = TimeSpan.FromSeconds(2.34123f),
            };
            tween.Completed += (o, e) =>
            {
                completed = true;
            };
            tween.Play();
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.34234f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.284957f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.0001479f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2.38562939f)));

            Assert.IsTrue(completed);
            Assert.AreEqual<TimeSpan>(tween.Duration, tween.Position);
            Assert.AreEqual<TimeSpan>(tween.Duration, tween.ElapsedTime);
            Assert.AreEqual<float>(tween.To.Value, tween.Value);
        }

        [TestMethod()]
        public void CompleteTestWithZeroDuration()
        {
            bool completed = false;
            TweenAnimation<float> tween = new TweenAnimation<float>()
            {
                From = -1,
                To = 1,
                Duration = TimeSpan.Zero,
            };
            tween.Completed += (o, e) =>
            {
                completed = true;
            };
            tween.Play();
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.34234f)));

            Assert.IsTrue(completed);
            Assert.AreEqual<TimeSpan>(tween.Duration, tween.Position);
            Assert.AreEqual<TimeSpan>(tween.Duration, tween.ElapsedTime);
            Assert.AreEqual<float>(tween.From.Value, tween.Value);
        }

        [TestMethod()]
        public void CompleteTestWithAutoReverse()
        {
            bool completed = false;
            TweenAnimation<float> tween = new TweenAnimation<float>()
            {
                From = -0.239572048f,
                To = 1.2223f,
                AutoReverse = true,
                Duration = TimeSpan.FromSeconds(2.34123f),
            };
            tween.Completed += (o, e) =>
            {
                completed = true;
            };
            tween.Play();
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.34234f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.284957f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.0001479f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2.38562939f)));

            Assert.IsTrue(completed);
            Assert.AreEqual<TimeSpan>(tween.Duration, tween.Position);
            Assert.AreEqual<TimeSpan>(tween.Duration, tween.ElapsedTime);
            Assert.AreEqual<float>(tween.To.Value, tween.Value);
        }

        [TestMethod()]
        public void CompleteTestBackward()
        {
            bool completed = false;
            TweenAnimation<float> tween = new TweenAnimation<float>()
            {
                From = -0.239572048f,
                To = 1.2223f,
                Duration = TimeSpan.FromSeconds(2.34123f),
            };
            tween.Completed += (o, e) =>
            {
                completed = true;
            };
            tween.StartupDirection = AnimationDirection.Backward;
            tween.Play();
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.34234f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.284957f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.0001479f)));
            tween.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2.38562939f)));

            Assert.IsTrue(completed);
            Assert.AreEqual<TimeSpan>(TimeSpan.Zero, tween.Position);
            Assert.AreEqual<TimeSpan>(tween.Duration, tween.ElapsedTime);
            Assert.AreEqual<float>(tween.From.Value, tween.Value);
        }

        [TestMethod()]
        public void ByTest()
        {
            Assert.AreEqual<Vector2>(
                Vector2.One,
                ExpressionAdd<Vector2>(Vector2.UnitY, Vector2.UnitX));

            Assert.AreEqual<int>(
                3,
                ExpressionAdd<int>(1, 2));

            Assert.AreEqual<Vector2>(
                Vector2.One,
                DynamicAdd<Vector2>(Vector2.UnitY, Vector2.UnitX));

            Assert.AreEqual<int>(
                3,
                DynamicAdd<int>(1, 2));
        }

        private T ExpressionAdd<T>(T x, T y)
        {
            object result = Expression.Lambda<Func<object>>
                (Expression.Convert(Expression.Add(
                    Expression.Constant(x), 
                        Expression.Constant(y)), 
                            typeof(object))).Compile()();

            return (T)result;
        }

        private T ReflectionAdd<T>(T x, T y)
        {
            return (default(T));
        }

        private T DynamicAdd<T>(T x, T y)
        {
            dynamic dx = x;
            dynamic dy = y;

            return (T)(dx + dy);
        }
    }
}
