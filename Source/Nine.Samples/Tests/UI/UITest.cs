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
    using Nine.Graphics.UI.Controls.Primitives;

    using Microsoft.Xna.Framework.Content;

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
        public override Scene CreateScene(GraphicsDevice graphics, ContentManager content)
        {
            var scene = new Scene();
            var font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");
            //var tooltipBackground = content.Load<Texture2D>("Textures/ToolTip.png");

            Border topBorder, sideBorder1, sideBorder2, border1;
            StackPanel stackPanel, topStackPanel;
            scene.Add(new Window(scene)
            {
                Content = new Grid(
                    new ColumnDefinition[] {
                        new ColumnDefinition(1),
                        new ColumnDefinition(3),
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
                            Content = topStackPanel = new StackPanel(Orientation.Horizontal)
                        },

                        sideBorder1 = new Border() {
                            BorderThickness = 1,
                            BorderBrush = new SolidColorBrush(Color.Black),
                            Margin = new Thickness(10, 0, 10, 0),
                            //Content = new ScrollViewer() {
                                Content = stackPanel = new StackPanel(Orientation.Vertical)
                            //}
                        },

                        sideBorder2 = new Border() {
                            BorderThickness = 1,
                            BorderBrush = new SolidColorBrush(Color.Black),
                            Margin = new Thickness(10, 0, 10, 0),
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
                                Background = Color.SteelBlue
                            }
                        },
                    })
            });

            Grid.SetColumnSpan(topBorder, 3);
            Grid.SetRow(sideBorder1, 1);
            Grid.SetColumn(sideBorder2, 2);
            Grid.SetRow(sideBorder2, 1);
            Grid.SetColumn(border1, 1);
            Grid.SetRow(border1, 1);

            //
            {
                Color defltButtonBackground = new Color(176, 196, 222);
                Color hoverButtonBackground = new Color(111, 159, 222);
                Color clickButtonBackground = new Color(055, 128, 222);

                topStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                topStackPanel.Parent.Background = defltButtonBackground;

                for (int i = 0; i < 3; i++)
                {
                    var button = new Button(font, string.Format("Menu {0}", (i + 1)));
                    button.Background = defltButtonBackground;
                    button.Width = 256;

                    button.Click += (s, e) => System.Diagnostics.Debug.WriteLine(string.Format("Mouse.Click: '{0}'", button.Text));

                    button.MouseDown  += (s, e) => { if (e.Button == MouseButtons.Left) { button.Background = clickButtonBackground; } };
                    button.MouseUp    += (s, e) => { button.Background = hoverButtonBackground; };
                    button.MouseEnter += (s, e) => { button.Background = hoverButtonBackground; };
                    button.MouseLeave += (s, e) => { button.Background = defltButtonBackground; };

                    topStackPanel.Children.Add(button);
                }
            }
            {
                Color defltButtonBackground = new Color(176, 196, 222);
                Color hoverButtonBackground = new Color(111, 159, 222);
                Color clickButtonBackground = new Color(055, 128, 222);

                stackPanel.Parent.Background = defltButtonBackground;

                for (int i = 0; i < 20; i++)
                {
                    var button = new Button(font, string.Format("Hello World! [{0}]", (i + 1).ToString("00")));
                    button.Background = defltButtonBackground;

                    //button.ToolTip = "Hello World?";
                    button.ToolTip = new TextBlock(font, string.Format("Hello World? [{0}]", (i + 1).ToString("00")))
                    {
                        Background = Color.White
                    };

                    //ToolTipService.SetPlacement(button, PlacementMode.Bottom);

                    button.Click += (s, e) => System.Diagnostics.Debug.WriteLine(string.Format("Mouse.Click: '{0}'", button.Text));

                    button.MouseDown  += (s, e) => { if (e.Button == MouseButtons.Left) { button.Background = clickButtonBackground; } };
                    button.MouseUp    += (s, e) => { button.Background = hoverButtonBackground; };
                    button.MouseEnter += (s, e) => { button.Background = hoverButtonBackground; };
                    button.MouseLeave += (s, e) => { button.Background = defltButtonBackground; };

                    stackPanel.Children.Add(button);
                }
            }

            return scene;
        }
    }
}
