#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Nine;
using Nine.Components;
using Microsoft.Xna.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using Nine.Animations;

namespace Nine.Test
{
    [TestClass()]
    public class Expressionest
    {
        class Container
        {
            public Child Child = new Child();
            public List<Child> Children = new List<Child>();
            public Dictionary<string, Child> Dictionary = new Dictionary<string, Child>();
            public Container()
            {
                Children.Add(new Child());
                Dictionary.Add("Name", new Child());
            }
        }

        class Child
        {
            public int X = 10;
        }

        [TestMethod()]
        public void ExpressionTest()
        {
            var a = new Container();
            var expression = new PropertyExpression<int>(a, "Child.X");
            Assert.AreEqual(10, expression.Value);
            expression.Value = 1;
            Assert.AreEqual(1, expression.Value);
            Assert.AreEqual(1, a.Child.X);

            expression = new PropertyExpression<int>(a, "Children[0].X");
            Assert.AreEqual(10, expression.Value);
            expression.Value = 1;
            Assert.AreEqual(1, expression.Value);
            Assert.AreEqual(1, a.Children[0].X);

            expression = new PropertyExpression<int>(a, "Dictionary[\"Name\"].X");
            Assert.AreEqual(10, expression.Value);
            expression.Value = 1;
            Assert.AreEqual(1, expression.Value);
            Assert.AreEqual(1, a.Dictionary["Name"].X);
        }
    }
}
