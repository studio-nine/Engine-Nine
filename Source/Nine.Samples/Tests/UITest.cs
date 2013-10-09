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

    public class UITest : Sample
    {
        public override Scene CreateScene(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics, Serialization.ContentLoader content)
        {
        
            var font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");

            var scene = new Scene();
            var manager = scene.GetWindowManager();

            scene.Add(new DialogWindow()
            {
                Viewport = new Rectangle(20, 50, 340, 195),
                WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246)),
                Background = new SolidColorBrush(new Color(255, 255, 255)),
                TitleFont = font,
                Title = "FPS Graph",
                Content = new FPSGraph()
                {
                    Margin = new Thickness(10)
                },
            });

            scene.Add(new DialogWindow()
            {
                Viewport = new Rectangle(20, 255, 340, 195),
                WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246)),
                Background = new SolidColorBrush(new Color(255, 255, 255)),
                TitleFont = font,
                Title = "Memory Graph",
                Content = new MemoryGraph()
                {
                    Margin = new Thickness(10)
                },
            });

            scene.Add(new DialogWindow()
            {
                Viewport = new Rectangle(370, 50, 620, 400),
                WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246)),
                Background = new SolidColorBrush(new Color(255, 255, 255)),
                TitleFont = font,
                Title = "Tab Control",
                Content = new TabControl()
                {
                    // need to set this a better way
                    //ItemsPanel = new StackPanel(
                    //        new TabItem[] 
                    //         {
                    //             new TabItem()
                    //             {
                    //                 Header = "Tab 1",
                    //                 Content = new TextBlock(font)
                    //                 {
                    //                     Text = "Test 1",
                    //                     VerticalAlignment = VerticalAlignment.Center,
                    //                     HorizontalAlignment = HorizontalAlignment.Center,
                    //                 }
                    //             },
                    //             new TabItem()
                    //             {
                    //                 Header = "Tab 2",
                    //                 Content = new TextBlock(font)
                    //                 {
                    //                     Text = "Test 2",
                    //                     VerticalAlignment = VerticalAlignment.Center,
                    //                     HorizontalAlignment = HorizontalAlignment.Center,
                    //                 }
                    //             },
                    //         }
                    //    ),
                },
            });

            scene.Add(new DialogWindow()
            {
                Viewport = new Rectangle(1000, 50, 270, 710),
                WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246)),
                Background = new SolidColorBrush(new Color(255, 255, 255)),
                TitleFont = font,
                Title = "",
                // Content = 
            });

            scene.Add(new DialogWindow()
            {
                Viewport = new Rectangle(20, 460, 970, 300),
                WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246)),
                Background = new SolidColorBrush(new Color(255, 255, 255)),
                TitleFont = font,
                Title = "Console",
                ResizeMode = Graphics.UI.ResizeMode.NoResize,
                // LockPosition = true,
                Content = new TextBox(font)
                {
                    Text = ">",
                    Foreground = Color.Black,
                }
            });

            return scene;
        }
    }

    class FPSGraph : LineGraph
    {
        public int CurrentFrame
        {
            get { return currentFrame; }
        }

        public float OverallFramesPerSecond
        {
            get { return overallFps; }
        }

        public float FramesPerSecond
        {
            get { return fps; }
        }

        public TimeSpan UpdateFrequency { get; set; }

        private int updateCount = 0;
        private int currentFrame = 0;
        private int counter = 0;
        private float elapsedTimeSinceLastUpdate = 0;
        private float fps = 0;
        private float overallFps = 0;

        public FPSGraph()
        {
            this.UpdateFrequency = TimeSpan.FromSeconds(0.1f);
        }

        protected override void OnRender(Renderer renderer)
        {
            UpdateFPS(renderer.ElapsedTime);
            base.OnRender(renderer);
        }

        private void UpdateFPS(float elapsedTime)
        {
            counter++;
            currentFrame++;

            elapsedTimeSinceLastUpdate += elapsedTime;
            if (elapsedTimeSinceLastUpdate >= UpdateFrequency.TotalSeconds)
            {
                fps = (float)counter / elapsedTimeSinceLastUpdate;
                counter = 0;
                elapsedTimeSinceLastUpdate -= (float)UpdateFrequency.TotalSeconds;

                overallFps = (overallFps * updateCount + fps) / (updateCount + 1);
                updateCount++;

                Add(FramesPerSecond * 10);
            }
        }
    }

    class MemoryGraph : LineGraph
    {
        public TimeSpan UpdateFrequency { get; set; }
        
        private float elapsedTimeSinceLastUpdate = 0;

        public MemoryGraph()
        {
            this.UpdateFrequency = TimeSpan.FromSeconds(0.1f);
        }

        protected override void OnRender(Renderer renderer)
        {
            elapsedTimeSinceLastUpdate += renderer.ElapsedTime;
            if (elapsedTimeSinceLastUpdate >= UpdateFrequency.TotalSeconds)
            {
                Add(GC.GetTotalMemory(false));
                elapsedTimeSinceLastUpdate -= (float)UpdateFrequency.TotalSeconds;
            }
            base.OnRender(renderer);
        }
    }
}
