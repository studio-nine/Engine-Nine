namespace Nine.Samples
{
    using System;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.UI;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Controls.DataVisualization;
    using Nine.Graphics.UI.Media;
    using Nine.Graphics.UI.Renderer;

    public class UIScrollViewerTest : Sample
    {
        public override string Title { get { return "[UI] ScrollViewer Test"; } }
        public override Scene CreateScene(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics, Serialization.ContentLoader content)
        {
            var scene = new Scene();
            var font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");

            Border border = new Border {
                Background = content.Load<Texture2D>("textures/checker.bmp"),
                Height = 500,
            };

            ScrollContentPresenter scrollPresenter = new ScrollContentPresenter();
            scrollPresenter.Content = border;

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.CanVerticallyScroll = true;
            scrollViewer.Content = scrollPresenter;
            
            Grid grid = new Grid(
                new ColumnDefinition[] {
                    new ColumnDefinition(),
                    new ColumnDefinition(400, GridUnitType.Pixel),
                    new ColumnDefinition(),
                }, new RowDefinition[] {
                    new RowDefinition(),
                    new RowDefinition(300, GridUnitType.Pixel),
                    new RowDefinition(),
                });
            grid.ShowGridLines = true;
            grid.Background = new SolidColorBrush(Color.Black);

            Grid.SetColumn(scrollViewer, 1);
            Grid.SetRow(scrollViewer, 1);
            grid.Children.Add(scrollViewer);

            Window window = new Window();
            window.Content = grid;
            scene.Add(window);

            scrollPresenter.SetVerticalOffset(0.5f);

            return scene;
        }
    }
}
