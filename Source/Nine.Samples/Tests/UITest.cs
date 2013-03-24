namespace Nine.Samples
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Serialization;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Controls.Shapes;
    using Nine.Graphics.UI.Media;

    public class UITest : Sample
    {
        public override Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();
            scene.Add(new TMPCamera2D(graphics));

            Window window = new Window();

            var MainGrid = new Grid()
                {
                    RowDefinitions =
                        {
                            new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) },
                            new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) }, 
                        },
                    ColumnDefinitions =
                        {
                            new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) },
                            new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) }, 
                            new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) }, 
                        },
                };

            #region Borders
            { // #Borders
                var grid = new Grid()
                    {
                        RowDefinitions =
                        {
                            new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) }, 
                            new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) },
                        },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) },
                            new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) },
                        },
                    };
                Grid.SetRow(grid, 0);
                Grid.SetColumn(grid, 0);
                MainGrid.Children.Add(grid);

                var border1 = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.Red),
                    BorderThickness = new Thickness(4),
                };
                grid.Children.Add(border1);

                var border2 = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.Green),
                    BorderThickness = new Thickness(4),
                    Content = new Border()
                    {
                        Background = new ImageBrush(content.Load<Texture2D>("Textures/checker.bmp"))
                    }
                };
                Grid.SetRow(border2, 1);
                grid.Children.Add(border2);

                var border3 = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.Blue),
                    BorderThickness = new Thickness(4),
                };
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
            }
            #endregion

            #region StackPanel & ScrollViewer
            { // #StackPanel & #ScrollViewer
                var ScrollViewer = new ScrollViewer();
                var ScrollContentPresenter = new ScrollContentPresenter();
                var StackPanel = new StackPanel() { Orientation = Orientation.Vertical, Background = new ImageBrush(content.Load<Texture2D>("Textures/checker.bmp")) };
                StackPanel.Children.Add(new Border() { Margin = new Thickness(8), Height = 50, BorderBrush = new SolidColorBrush(Color.Red), BorderThickness = new Thickness(6) });
                StackPanel.Children.Add(new Button()
                {
                    Height = 50,
                    Margin = new Thickness(4),
                    Background = new SolidColorBrush(Color.LightGray),
                });

                // #ProgressBar
                ProgressBar ProgressBar;
                StackPanel.Children.Add(ProgressBar = new ProgressBar()
                {
                    Value = 50,
                    Height = 50,
                    Margin = new Thickness(2)
                });

                // This is mostly just to test it out :D
                var TweenA = new Nine.Animations.TweenAnimation<float>((Interpolate<float>)MathHelper.Lerp, (Operator<float>)AddHelper.Add);
                TweenA.Target = ProgressBar;
                TweenA.TargetProperty = "Value";
                TweenA.From = 10;
                TweenA.To = 100;
                TweenA.Duration = new TimeSpan(0, 0, 5);
                TweenA.AutoReverse = true;
                TweenA.Repeat = 10000000f;
                scene.Add(TweenA);
                TweenA.Play();

                // #ProgressBar
                ProgressBar ProgressBar2;
                StackPanel.Children.Add(ProgressBar2 = new ProgressBar()
                {
                    Orientation = Orientation.Vertical,
                    Value = 50,
                    Height = 100,
                    Margin = new Thickness(20,2,20,2)
                });

                // This is mostly just to test it out :D
                var TweenA2 = new Nine.Animations.TweenAnimation<float>((Interpolate<float>)MathHelper.Lerp, (Operator<float>)AddHelper.Add);
                TweenA2.Target = ProgressBar2;
                TweenA2.TargetProperty = "Value";
                TweenA2.From = 10;
                TweenA2.To = 100;
                TweenA2.Duration = new TimeSpan(0, 0, 5);
                TweenA2.AutoReverse = true;
                TweenA2.Repeat = 10000000f;
                scene.Add(TweenA2);
                TweenA2.Play();

                ScrollViewer.Content = ScrollContentPresenter;
                ScrollContentPresenter.Content = StackPanel;
                Grid.SetColumn(ScrollViewer, 1);
                MainGrid.Children.Add(ScrollViewer);
            }
            #endregion

            { // #Image
                var Image = new Image(content.Load<Texture2D>("Textures/box.dds"));
                Grid.SetRow(Image, 1);
                Grid.SetColumn(Image, 0);
                MainGrid.Children.Add(Image);
            }

            window.Content = MainGrid;
            scene.Add(window);
            return scene;
        }
    }
}
