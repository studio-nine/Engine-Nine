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
    using Nine.Graphics.Primitives;

    public class UITest : Sample
    {
        public override string Title { get { return "[UI] Window Test"; } }
        public override Scene CreateScene(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics, Serialization.ContentLoader content)
        {
            var scene = new Scene();
            var font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");

            Border border1;
            TextBlock helloWorldText;

            Window window = new Window()
            {
                Content = new Grid(
                    new ColumnDefinition[] {
                        new ColumnDefinition(1),
                        new ColumnDefinition(2),
                        new ColumnDefinition(1),
                    },
                    new RowDefinition[] {
                        new RowDefinition(1), 
                        new RowDefinition(3), 
                        new RowDefinition(1), 
                    },
                    new UIElement[] {
                        border1 = new Border() {
                            BorderThickness = 1,
                            BorderBrush = new SolidColorBrush(Color.Black),
                            Content = helloWorldText = new TextBlock(font, "Hello World!") {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                            }
                        },
                    })
            };

            Grid.SetColumn(border1, 1);
            Grid.SetRow(border1, 1);

            WindowManager manager = new WindowManager();
            scene.Add(manager);
            manager.Windows.Add(window);


            return scene;
        }
    }
}
