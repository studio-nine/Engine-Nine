namespace Nine.Graphics.UI.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Controls;

    [TestClass]
    public class ScrollViewerTest
    {
        [TestMethod()]
        public void ScrollViewerSize()
        {
            StackPanel stackPanel;
            ScrollContentPresenter scrollContentPresenter;
            ScrollViewer scrollViewer;

            stackPanel = new StackPanel();
            stackPanel.Height = 600;

            scrollContentPresenter = new ScrollContentPresenter();
            scrollContentPresenter.Content = stackPanel;

            scrollViewer = new ScrollViewer();
            scrollViewer.Content = scrollContentPresenter;

            scrollViewer.Measure(new Vector2(100, 300));
            scrollViewer.Arrange(new BoundingRectangle(0, 0, 100, 300));

            Assert.AreEqual(100, scrollViewer.ActualWidth);
            Assert.AreEqual(300, scrollViewer.ActualHeight);

            Assert.AreEqual(0, scrollContentPresenter.Extent.X);
            Assert.AreEqual(600, scrollContentPresenter.Extent.Y);

            Assert.AreEqual(100, scrollContentPresenter.Viewport.X);
            Assert.AreEqual(300, scrollContentPresenter.Viewport.Y);

            Assert.AreEqual(0, scrollContentPresenter.Offset.X);
            Assert.AreEqual(0, scrollContentPresenter.Offset.Y);

            scrollContentPresenter.SetVerticalOffset(100);

            Assert.AreEqual(0, scrollContentPresenter.Offset.X);
            Assert.AreEqual(100, scrollContentPresenter.Offset.Y);

            Assert.AreEqual(100, scrollContentPresenter.Viewport.X);
            Assert.AreEqual(300, scrollContentPresenter.Viewport.Y);
        }
    }
}
