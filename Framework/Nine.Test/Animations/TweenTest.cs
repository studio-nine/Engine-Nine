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
