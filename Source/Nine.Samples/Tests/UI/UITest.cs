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

    class ScrollViewerComponent : Component
    {
        private ScrollContentPresenter scrollviewer;
        private bool plus;

        public ScrollViewerComponent(ScrollContentPresenter scrollViewer)
        {
            this.scrollviewer = scrollViewer;
            this.plus = true;
        }

        protected override void Update(float elapsedTime)
        {
            float toValue = plus ? 1.0f : 0.0f;
            float result = MathHelper.Lerp(scrollviewer.Offset.Y, toValue, elapsedTime);
            scrollviewer.SetVerticalOffset(result);
            if ((float)Math.Round(result, 1) == toValue)
                plus = !plus;

            base.Update(elapsedTime);
        }
    }

    public class UITest : Sample
    {
        public override string Title { get { return "[UI] Window Test"; } }
        public override Scene CreateScene(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics, Serialization.ContentLoader content)
        {
            var scene = new Scene();
            var font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");

            Border topBorder, sideBorder1, sideBorder2, border1;
            StackPanel stackPanel;
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
                        new RowDefinition(8), 
                        new RowDefinition(1), 
                    },
                    new UIElement[] {
                        topBorder = new Border() {
                            BorderThickness = 1,
                            BorderBrush = new SolidColorBrush(Color.Black),
                            Margin = new Thickness(10, 20, 10, 30),
                        },

                        sideBorder1 = new Border() {
                            BorderThickness = 1,
                            BorderBrush = new SolidColorBrush(Color.Black),
                            Margin = new Thickness(10, 0, 40, 0),
                            //Content = new ScrollViewer() {
                                Content = stackPanel = new StackPanel(Orientation.Vertical)
                            //}
                        },

                        sideBorder2 = new Border() {
                            BorderThickness = 1,
                            BorderBrush = new SolidColorBrush(Color.Black),
                            Margin = new Thickness(40, 0, 10, 0),
                            Content = new Border() {
                                BorderThickness = 1,
                                BorderBrush = new SolidColorBrush(Color.Black),
                                Margin = new Thickness(10),
                            }
                        },

                        border1 = new Border() {
                            BorderThickness = 1,
                            BorderBrush = new SolidColorBrush(Color.Black),
                            Content = new TabControl() {
                                
                            }
                        },
                    })
            };

            Grid.SetColumnSpan(topBorder, 3);
            Grid.SetRow(sideBorder1, 1);
            Grid.SetColumn(sideBorder2, 2);
            Grid.SetRow(sideBorder2, 1);
            Grid.SetColumn(border1, 1);
            Grid.SetRow(border1, 1);

            Color defaultButtonBackground = Color.WhiteSmoke;
            Color hoverButtonBackground = Color.Gray;

            for (int i = 0; i < 12; i++)
            {
                var button = new Button(font, string.Format("Hello World! [{0}]", (i + 1).ToString("00")));
                button.Background = defaultButtonBackground;

                button.Click += (s, e) => {
                    button.Background = Color.Red; 
                };

                button.MouseEnter += (s, e) => {
                    button.Background = hoverButtonBackground; 
                };

                button.MouseLeave += (s, e) => {
                    button.Background = defaultButtonBackground;
                };

                stackPanel.Children.Add(button);
            }


            WindowManager manager = new WindowManager();
            scene.Add(manager);
            manager.Windows.Add(window);

            return scene;
        }
    }
}
