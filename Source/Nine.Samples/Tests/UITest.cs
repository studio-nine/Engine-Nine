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
            var Font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");

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
                StackPanel.Children.Add(ProgressBar = new ProgressBar() { Value = 50, Height = 50, Margin = new Thickness(2) });

                var TweenA = new Nine.Animations.TweenAnimation<float>()
                {
                    Target = ProgressBar,
                    TargetProperty = "Value",
                    From = 10,
                    To = 100,
                    Duration = new TimeSpan(0, 0, 5),
                    AutoReverse = true,
                    Repeat = 10000000f
                };
                // Currently can only play one Animation at the time
                scene.Animations.Play(TweenA);

                // #ProgressBar
                ProgressBar ProgressBar2;
                StackPanel.Children.Add(ProgressBar2 = new ProgressBar()
                {
                    Orientation = Orientation.Vertical,
                    Value = 50,
                    Height = 100,
                    Margin = new Thickness(20,2,20,2)
                });

                var TweenA2 = new Nine.Animations.TweenAnimation<float>()
                {
                    Target = ProgressBar2,
                    TargetProperty = "Value",
                    From = 10,
                    To = 100,
                    Duration = new TimeSpan(0, 0, 5),
                    AutoReverse = true,
                    Repeat = 10000000f
                };
                scene.Animations.Play(TweenA2);

                ScrollViewer.Content = ScrollContentPresenter;
                ScrollContentPresenter.Content = StackPanel;
                Grid.SetColumn(ScrollViewer, 2);
                MainGrid.Children.Add(ScrollViewer);
            }
            #endregion

            { // #Image
                var Image = new Image(content.Load<Texture2D>("Textures/box.dds"));
                Grid.SetRow(Image, 1);
                Grid.SetColumn(Image, 0);
                MainGrid.Children.Add(Image);
            }

            // #Controls
            var ControlGrid = new Grid()
            {
                RowDefinitions =
                    {
                        new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) }, 
                        new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) },
                        new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) }, 
                        new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) },
                    },
                ColumnDefinitions =
                    {
                        new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) },
                        new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) },
                    },
            };
            Grid.SetRow(ControlGrid, 1);
            Grid.SetColumn(ControlGrid, 1);
            MainGrid.Children.Add(ControlGrid);

            { // #MediaElement
                var MediaElement = new MediaElement(content.Load<Microsoft.Xna.Framework.Media.Video>("test.wmv"))
                {
                    Loop = true
                };
                //MediaElement.Play();
                Grid.SetRow(MediaElement, 0);
                Grid.SetColumn(MediaElement, 1);
                MainGrid.Children.Add(MediaElement);

                var PlayButton = new Button()
                {
                    Margin = new Thickness(8),
                    Background = new SolidColorBrush(Color.LightGray),
                    Content = new TextBlock(Font)
                    {
                        Text = "Play",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                PlayButton.Click += (s, e) => { MediaElement.Play(); };
                Grid.SetColumn(PlayButton, 0);

                var PauseButton = new Button()
                {
                    Margin = new Thickness(8),
                    Background = new SolidColorBrush(Color.LightGray),
                    Content = new TextBlock(Font)
                    {
                        Text = "Pause", 
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                PlayButton.Click += (s, e) => { MediaElement.Pause(); };
                Grid.SetColumn(PauseButton, 1);

                ControlGrid.Children.Add(PlayButton);
                ControlGrid.Children.Add(PauseButton);
            }
            

            window.Content = MainGrid;
            scene.Add(window);
            return scene;
        }
    }
}
