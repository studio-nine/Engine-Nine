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
        public void GridLayout()
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

            var border1 = new Border()
            {
                Content = new Image()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                }
            };
            Grid.SetRow(border1, 1);
            Grid.SetColumn(border1, 0);
            grid.Children.Add(border1);

            var border2 = new Border()
            {
                Content = new Border() { }
            };
            Grid.SetRow(border2, 0);
            Grid.SetColumn(border2, 0);
            grid.Children.Add(border2);

            window.Update();

            Assert.AreEqual(new Vector2(100, 100), border1.RenderSize);
            Assert.AreEqual(new Vector2(0, 100), border1.VisualOffset);
            Assert.AreEqual(new Vector2(0, 100), border1.Content.VisualOffset);

            Assert.AreEqual(new Vector2(100, 100), border2.RenderSize);
            Assert.AreEqual(new Vector2(0, 0), border2.VisualOffset);
            Assert.AreEqual(new Vector2(0, 0), border2.Content.VisualOffset);
            // I might have made my math wrong
        }

        [TestMethod()]
        public void StackPanelLayout()
        {
            StackPanel sp;
            Window window = new Window();
            window.Viewport = new Rectangle(0, 0, 800, 800);

            window.Content = sp = new StackPanel() { Orientation = Orientation.Horizontal };

            Border Border1, Border2, Border21;

            sp.Children.Add(Border1 = new Border() { Height = 50 });
            sp.Children.Add(Border2 = new Border() { Height = 50, Content = Border21 = new Border() });

            Assert.AreEqual(new Vector2(0, 50), Border2.AbsoluteVisualOffset);
            Assert.AreEqual(new Vector2(0, 50), Border21.AbsoluteVisualOffset);
            // I might have made my math wrong
        }
    }
}
