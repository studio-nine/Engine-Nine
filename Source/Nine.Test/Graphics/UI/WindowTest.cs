namespace Nine.Graphics.UI.Test
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

            window.Messure();

            Assert.AreEqual(new Vector2(100, 100), border1.RenderSize);
            Assert.AreEqual(new Vector2(0, 100), border1.VisualOffset);

            Assert.AreEqual(new Vector2(100, 100), border2.RenderSize);
            Assert.AreEqual(new Vector2(0, 0), border2.VisualOffset);
        }

        [TestMethod()]
        public void StackPanelLayout()
        {
            StackPanel sp;
            Window window = new Window();
            window.Viewport = new Rectangle(0, 0, 200, 800);

            window.Content = sp = new StackPanel() { Orientation = Orientation.Vertical };

            Border Border1, Border2, Border3;

            sp.Children.Add(Border1 = new Border() { Height = 100 });
            sp.Children.Add(Border2 = new Border() { Height = 100 });
            sp.Children.Add(Border3 = new Border() { Height = 100 });

            window.Messure();

            Assert.AreEqual(new Vector2(0, 0), Border1.AbsoluteVisualOffset);
            Assert.AreEqual(new Vector2(0, 100), Border2.AbsoluteVisualOffset);
            Assert.AreEqual(new Vector2(0, 200), Border3.AbsoluteVisualOffset);
        }
    }
}
