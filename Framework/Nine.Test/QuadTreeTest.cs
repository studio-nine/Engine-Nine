namespace Nine.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;

    [TestClass()]
    public class QuadTreeTest
    {
        [TestMethod()]
        public void AddRemoveTest()
        {
            Octree<bool> oct = new Octree<bool>(new BoundingBox(Vector3.Zero, Vector3.One * 4), 2);

            oct.ExpandAll((o) => { return o.bounds.Contains(Vector3.One * 0.1f) == ContainmentType.Contains; });

            object a = new object();
            object b = new object();

            QuadTree<object> tree = new QuadTree<object>(new BoundingRectangle(4, 4), 2);
        }

        [TestMethod()]
        public void ExpandTest()
        {
            Assert.AreEqual(ContainmentType.Contains,
                new BoundingBox(Vector3.Zero, Vector3.One).Contains(Vector3.One));

            BoundingRectangle bounds = new BoundingRectangle(4, 4);

            QuadTree<object> tree = new QuadTree<object>(bounds, 2);

            Assert.AreEqual<BoundingRectangle>(bounds, tree.Bounds);
            Assert.AreEqual<BoundingRectangle>(bounds, tree.root.Bounds);
            Assert.AreEqual<int>(0, tree.root.depth);
        }
    }
}
