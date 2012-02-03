#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nine.Graphics.ObjectModel.Test
{
    [TestClass]
    public class DisplayObjectTest : ContentPipelineTest
    {
        class Node : Transformable { }

        [TestMethod]
        public void TransformBindingSortTest()
        {
            var container = new DisplayObject();
            
            var a = new Node();
            var b = new Node();
            var c = new Node();

            container.Children.Add(a);
            container.Children.Add(b);
            container.Children.Add(c);

            container.Bind(a, b);
            container.Bind(b, c);

            container.Update(TimeSpan.Zero);

            Assert.AreEqual(c, container.sortedTransformBindings[0].Target);
            Assert.AreEqual(b, container.sortedTransformBindings[1].Target);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransformBindingCircularSortTest()
        {
            var container = new DisplayObject();

            var a = new Node();
            var b = new Node();
            var c = new Node();

            container.Children.Add(a);
            container.Children.Add(b);
            container.Children.Add(c);

            container.Bind(a, b);
            container.Bind(b, c);
            container.Bind(c, a);

            container.Update(TimeSpan.Zero);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransformBindingCircularSortTest2()
        {
            var container = new DisplayObject();

            var a = new Node();
            var b = new Node();
            var c = new Node();

            container.Children.Add(a);
            container.Children.Add(b);
            container.Children.Add(c);

            container.Bind(a, c);
            container.Bind(b, c);
            container.Bind(c, a);

            container.Update(TimeSpan.Zero);
        }
    }
}
