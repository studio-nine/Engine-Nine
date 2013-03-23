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
            scene.Add(new Camera2D(graphics) { InputEnabled = true });

            Window window = new Window();

            var MainGrid = new Grid()
                {
                    RowDefinitions =
                        {
                            new RowDefinition() { Height = new GridLength(400) },
                            new RowDefinition() { Height = new GridLength(20) },
                            new RowDefinition() { Height = new GridLength(400) }, 
                            new RowDefinition() { Height = new GridLength(20) },
                            new RowDefinition() { Height = new GridLength(400) }, 
                        },
                    ColumnDefinitions =
                        {
                            new ColumnDefinition() { Width = new GridLength(400) },
                            new ColumnDefinition() { Width = new GridLength(20) },
                            new ColumnDefinition() { Width = new GridLength(400) }, 
                            new ColumnDefinition() { Width = new GridLength(20) },
                            new ColumnDefinition() { Width = new GridLength(400) }, 
                            new ColumnDefinition() { Width = new GridLength(20) },
                            new ColumnDefinition() { Width = new GridLength(400) }, 
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
                StackPanel.Children.Add(new Border() { Margin = new Thickness(2), Height = 50, BorderBrush = new SolidColorBrush(Color.Red), BorderThickness = new Thickness(2) });
                StackPanel.Children.Add(new Border() 
                { 
                    Margin = new Thickness(2), 
                    Height = 50, 
                    BorderBrush = new SolidColorBrush(Color.Green), 
                    BorderThickness = new Thickness(2),
                    Content = new Button()
                    {
                        Margin = new Thickness(2),
                        Background = new SolidColorBrush(Color.LightGray),
                    }
                });

                // #ProgressBar
                ProgressBar ProgressBar;
                StackPanel.Children.Add(new Border()
                {
                    Margin = new Thickness(2),
                    Height = 50,
                    BorderBrush = new SolidColorBrush(Color.Blue),
                    BorderThickness = new Thickness(2),
                    Content = ProgressBar = new ProgressBar()
                    {
                        Value = 50,
                        Margin = new Thickness(2)
                    }
                });

                // This is mostly just to test it out :D
                var TweenA = new Nine.Animations.TweenAnimation<float>((Interpolate<float>)MathHelper.Lerp, (Operator<float>)AddHelper.Add);
                TweenA.Target = ProgressBar;
                TweenA.TargetProperty = "Value";
                TweenA.From = 10;
                TweenA.To = 90;
                TweenA.Duration = new TimeSpan(0, 0, 5);
                TweenA.AutoReverse = true;
                TweenA.Repeat = 10000000f;
                scene.Add(TweenA);
                TweenA.Play();

                ScrollViewer.Content = ScrollContentPresenter;
                ScrollContentPresenter.Content = StackPanel;
                Grid.SetColumn(ScrollViewer, 2);
                MainGrid.Children.Add(ScrollViewer);
            }
            #endregion

            { // #Image
                var Image = new Image(content.Load<Texture2D>("Textures/box.dds"));
                Grid.SetRow(Image, 2);
                Grid.SetColumn(Image, 0);
                MainGrid.Children.Add(Image);
            }

            { // #Canvas
                var Canvas = new Canvas();
                Grid.SetRow(Canvas, 2);
                Grid.SetColumn(Canvas, 2);
                MainGrid.Children.Add(Canvas);
            }

            /* Nine Content loader dont support video
            { // #MediaElement
                var MediaElement = new MediaElement(content.Load<Microsoft.Xna.Framework.Media.Video>("TEST.wmv"));
                MediaElement.Play();
                Grid.SetRow(MediaElement, 2);
                Grid.SetColumn(MediaElement, 4);
                MainGrid.Children.Add(MediaElement);
            }
            */

            window.Content = MainGrid;
            scene.Add(window);
            return scene;
        }
    }
}
