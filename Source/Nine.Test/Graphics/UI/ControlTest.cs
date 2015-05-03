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
            //Border border1, border2;
            //Window window = new Window
            //{
            //    Viewport = new Rectangle(0, 0, 800, 800),
            //    Content = new Grid(
            //        new ColumnDefinition[] {
            //            new ColumnDefinition() { Width = new GridLength(100) },
            //        },
            //        new RowDefinition[] {
            //            new RowDefinition { Height = new GridLength(100) }, 
            //            new RowDefinition { Height = new GridLength(100) }, 
            //            new RowDefinition { Height = new GridLength(100) }, 
            //        },
            //        new UIElement[] {
            //            border1 = new Border() {
            //                Content = new Image() {
            //                    HorizontalAlignment = HorizontalAlignment.Stretch,
            //                    VerticalAlignment = VerticalAlignment.Stretch
            //                }
            //            },
            //            border2 = new Border() {
                            
            //            }
            //        })
            //    {
                    
            //    }
            //};

            //Grid.SetRow(border1, 1);
            //Grid.SetColumn(border1, 0);
            //Grid.SetRow(border2, 0);
            //Grid.SetColumn(border2, 0);

            //window.Messure();

            //Assert.AreEqual(new Vector2(100, 100), border1.RenderSize);
            //Assert.AreEqual(new Vector2(0, 100), border1.VisualOffset);

            //Assert.AreEqual(new Vector2(100, 100), border2.RenderSize);
            //Assert.AreEqual(new Vector2(0, 0), border2.VisualOffset);
        }

        [TestMethod()]
        public void StackPanelLayout()
        {
            //Border Border1, Border2, Border3;
            //Window window = new Window
            //{
            //    Viewport = new Rectangle(0, 0, 200, 800),
            //    Content = new StackPanel(
            //        new UIElement[] {
            //            Border1 = new Border() { Height = 100 },
            //            Border2 = new Border() { Height = 100 },
            //            Border3 = new Border() { Height = 100 },
            //        }) { 
            //        Orientation = Orientation.Vertical 
            //    },
            //};

            //window.Messure();

            //Assert.AreEqual(new Vector2(0, 0), Border1.AbsoluteVisualOffset);
            //Assert.AreEqual(new Vector2(0, 100), Border2.AbsoluteVisualOffset);
            //Assert.AreEqual(new Vector2(0, 200), Border3.AbsoluteVisualOffset);

            //Assert.AreEqual(Border2, HitTest(window.Content, new Vector2(100, 200)));
        }

        [TestMethod()]
        public void MarginLayout()
        {
            //Border border1, border2, border3;
            //Window window = new Window()
            //{
            //    Viewport = new Rectangle(0, 0, 200, 200),
            //    Content = border1 = new Border() { 
            //        Margin = new Thickness(5),
            //        Content = border2 = new Border() {
            //            Margin = new Thickness(5),
            //            Content = border3 = new Border() {
            //                Margin = new Thickness(5),
            //            }
            //        }
            //    }
            //};

            //window.Messure();

            
            //Assert.AreEqual(new BoundingRectangle(05, 05, 190, 190), border1.AbsoluteRenderTransform);
            //Assert.AreEqual(new BoundingRectangle(10, 10, 180, 180), border2.AbsoluteRenderTransform);
            //Assert.AreEqual(new BoundingRectangle(15, 15, 170, 170), border3.AbsoluteRenderTransform);
        }

        static UIElement HitTest(UIElement element, Vector2 hit)
        {
            if (element.HitTest(hit))
            {
                var container = element as IContainer;
                if (container != null)
                {
                    foreach (var child in container.Children)
                    {
                        var uiElement = child as UIElement;
                        if (uiElement != null && uiElement.HitTest(hit))
                        {
                            return HitTest(uiElement, hit);
                        }
                    }
                }
                return element;
            }
            return null;
        }
    }
}
