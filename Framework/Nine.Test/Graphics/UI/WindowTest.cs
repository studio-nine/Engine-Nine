namespace Nine.Test.Graphics.UI
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Media;

    [TestClass]
    public class WindowTest
    {
        [TestMethod()]
        public void Layout()
        {
            Grid grid;
            Window window = new Window();

            window.Viewport = new Rectangle(0, 0, 800, 800);
            window.Content = grid = new Grid()
            {
                RowDefinitions =
                        {
                            new RowDefinition { Height = new GridLength(100) }, 
                            new RowDefinition { Height = new GridLength(100) }, 
                            new RowDefinition { Height = new GridLength(100) }, 
                        },
                ColumnDefinitions =
                        {
                            new ColumnDefinition() { Width = new GridLength(100) },
                        },
            };

            var border1 = new Border();
            Grid.SetRow(border1, 1);
            Grid.SetColumn(border1, 0);
            grid.Children.Add(border1);

            var border2 = new Border();
            Grid.SetRow(border2, 0);
            Grid.SetColumn(border2, 0);
            grid.Children.Add(border2);

            window.Update();

            Assert.AreEqual(new Vector2(100, 100), border1.RenderSize);
            Assert.AreEqual(new Vector2(0, 100), border1.VisualOffset);

            Assert.AreEqual(new Vector2(100, 100), border2.RenderSize);
            Assert.AreEqual(new Vector2(0, 0), border2.VisualOffset);
        }
    }
}
