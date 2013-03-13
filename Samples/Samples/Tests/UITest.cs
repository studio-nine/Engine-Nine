namespace Samples
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Media;
    using Nine.Serialization;

    public class ________________UITest : Sample
    {
        public override Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();

            scene.Add(new Camera2D(graphics)
            {
                InputEnabled = true
            });

            var Font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");

            Window window = new Window();

            var grid = new Grid()
                {
                    RowDefinitions =
                        {
                            new RowDefinition { Height = new GridLength(100) }, 
                            new RowDefinition { Height = new GridLength(100) }, 
                        },
                    ColumnDefinitions =
                        {
                            new ColumnDefinition() { Width = new GridLength(100) },
                            new ColumnDefinition() { Width = new GridLength(100) },
                        },
                };
            window.Content = grid;

            Color[] Colors = new Color[]
            {
                Color.Red, Color.Green, Color.Blue, Color.Orange
            };

            for (int i = 0; i < 4; ++i)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Colors[i]),
                    BorderThickness = new Thickness(4),
                };
                Grid.SetRow(border, i % 2);
                Grid.SetColumn(border, (i + 4) % 2);
                grid.Children.Add(border);
            }

            var border1 = new Border { Background = new SolidColorBrush(Color.Red), BorderThickness = new Thickness(4), };
            Grid.SetRow(border1, 0);
            Grid.SetColumn(border1, 0);
            grid.Children.Add(border1);

            var border2 = new Border { Background = new SolidColorBrush(Color.Green), BorderThickness = new Thickness(4), };
            Grid.SetRow(border2, 1);
            Grid.SetColumn(border2, 0);
            grid.Children.Add(border2);

            var border3 = new Border { Background = new SolidColorBrush(Color.Blue), BorderThickness = new Thickness(4), };
            Grid.SetRow(border3, 0);
            Grid.SetColumn(border3, 1);
            grid.Children.Add(border3);

            var border4 = new Border { Background = new SolidColorBrush(Color.Orange), BorderThickness = new Thickness(4), };
            Grid.SetRow(border4, 1);
            Grid.SetColumn(border4, 1);
            grid.Children.Add(border4);

            scene.Add(window);
            return scene;
        }
    }
}
