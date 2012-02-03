#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace Nine.Test
{
    [TestClass()]
    public class TriangleTest
    {
        [TestMethod()]
        public void IntersectsTest()
        {
            Triangle target = new Triangle();

            target.V1 = Vector3.Zero;
            target.V2 = Vector3.UnitX * 2;
            target.V3 = Vector3.UnitY;

            Ray ray = new Ray(Vector3.One, -Vector3.Normalize(Vector3.One));

            Assert.AreEqual<bool>(true, target.Intersects(ray).HasValue);

            Assert.AreEqual<bool>(true, target.Intersects(Vector3.One, Vector3.UnitZ * -0.1f).HasValue);
            Assert.AreEqual<bool>(false, target.Intersects(Vector3.One, Vector3.UnitZ * 0.1f).HasValue);

            BoundingBox box = new BoundingBox();

            box.Min = new Vector3(1.8f, 0.01f, -0.1f);
            box.Max = Vector3.One * 4;

            Assert.AreEqual<bool>(true, target.Intersects(box.Min, box.Min + Vector3.UnitZ * 4).HasValue);
            Assert.AreEqual<ContainmentType>(ContainmentType.Intersects, box.Contains(target));
        }
    }
}
