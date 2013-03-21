namespace Nine.Graphics.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;

    [TestClass()]
    public class ModelTest : GraphicsTest
    {
        [TestMethod()]
        public void ModelAttachmentTest()
        {
            var model = new Model(GraphicsDevice);
            var box = new Box(GraphicsDevice);

            model.Attachments.Add(box);
            model.Attachments.Clear();
            model.Attachments.Add(box);
            model.Attachments.Remove(box);

            var scene = new Scene();
            scene.Add(model);
            model.Attachments.Add(box);
            scene.Clear();

            scene.Add(model);
            Assert.IsTrue(model.Attachments.Remove(box));
        }
    }
}
