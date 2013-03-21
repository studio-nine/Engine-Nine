namespace Nine.Samples
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

    public class UITest : Sample
    {
        public override Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();

            scene.Add(new Camera2D(graphics)
            {
                InputEnabled = true
            });

            Window window = new Window();

            var grid = new Grid()
                {
                    RowDefinitions =
                        {
                            new RowDefinition { Height = new GridLength(200) }, 
                            new RowDefinition { Height = new GridLength(200) }, 
                        },
                    ColumnDefinitions =
                        {
                            new ColumnDefinition() { Width = new GridLength(200) },
                            new ColumnDefinition() { Width = new GridLength(200) },
                        },
                };
            window.Content = grid;

            var border1 = new Border
            {
                BorderBrush = new SolidColorBrush(Color.Red),
                BorderThickness = new Thickness(4),
                Child = new Border()
                {
                    Background = new SolidColorBrush(Color.Gold)
                }

                //new TextBlock(content.Load<SpriteFont>("Fonts/Consolas.spritefont"))
                //    { Margin = new Thickness(10), Text = "HELLO WORLD" }
            };
            Grid.SetRow(border1, 0);
            Grid.SetColumn(border1, 0);
            grid.Children.Add(border1);

            var border2 = new Border 
            {
                BorderBrush = new SolidColorBrush(Color.Green),
                BorderThickness = new Thickness(4),
                Child = new Border()
                {
                    Background = new SolidColorBrush(Color.GhostWhite)
                }

                //Child = new Image() 
                //    { Source = content.Load<Texture2D>("Textures/checker.bmp") }
            };
            Grid.SetRow(border2, 1);
            Grid.SetColumn(border2, 0);
            grid.Children.Add(border2);

            var border3 = new Border 
            {
                BorderBrush = new SolidColorBrush(Color.Blue), 
                BorderThickness = new Thickness(4), 
            };
            Grid.SetRow(border3, 0);
            Grid.SetColumn(border3, 1);
            grid.Children.Add(border3);

            var border4 = new Border 
            { 
                BorderBrush = new SolidColorBrush(Color.Orange), 
                BorderThickness = new Thickness(4), 
            };
            Grid.SetRow(border4, 1);
            Grid.SetColumn(border4, 1);
            grid.Children.Add(border4);

            scene.Add(window);
            return scene;
        }
    }
}
