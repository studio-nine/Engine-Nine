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
            var scene = new Scene();
            var font = content.Load<SpriteFont>("Fonts/Consolas.spritefont");

            var window1 = new DialogWindow();
            window1.Viewport = new Rectangle(20, 50, 340, 195);
            window1.WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246));
            window1.Background = new SolidColorBrush(new Color(255, 255, 255));
            window1.TitleFont = font;
            window1.Title = "FPS Graph";
            window1.Content = new FPSGraph { Margin = new Thickness(10) };

            var window2 = new DialogWindow();
            window2.Viewport = new Rectangle(20, 255, 340, 195);
            window2.WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246));
            window2.Background = new SolidColorBrush(new Color(255, 255, 255));
            window2.TitleFont = font;
            window2.Title = "Memory Graph";
            window2.Content = new MemoryGraph() { Margin = new Thickness(10) };

            var window3 = new DialogWindow();
            window3.Viewport = new Rectangle(370, 50, 620, 400);
            window3.WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246));
            window3.Background = new SolidColorBrush(new Color(255, 255, 255));
            window3.TitleFont = font;
            window3.Title = "Tab Control";
            window3.Content = new TabControl();

            var window4 = new DialogWindow();
            window4.Viewport = new Rectangle(1000, 50, 270, 710);
            window4.WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246));
            window4.Background = new SolidColorBrush(new Color(255, 255, 255));
            window4.TitleFont = font;
            window4.Title = "";

            var window5 = new DialogWindow();
            window5.Viewport = new Rectangle(20, 460, 970, 300);
            window5.WindowBorderBrush = new SolidColorBrush(new Color(106, 172, 246));
            window5.Background = new SolidColorBrush(new Color(255, 255, 255));
            window5.TitleFont = font;
            window5.Title = "Console";
            window5.ResizeMode = Graphics.UI.ResizeMode.NoResize;
            window5.Content = new TextBox(font)
            {
                Text = ">",
                Foreground = Color.Black,
            };

            var manager = new WindowManager();
            manager.Windows.Add(window1);
            manager.Windows.Add(window2);
            manager.Windows.Add(window3);
            manager.Windows.Add(window4);
            manager.Windows.Add(window5);
            scene.Add(manager);

            return scene;
        }
    }

    #region Graphs

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

        private int updateCount = 0;
        private int currentFrame = 0;
        private int counter = 0;
        private float fps = 0;
        private float overallFps = 0;

        public FPSGraph()
        {
            this.UpdateFrequency = TimeSpan.FromSeconds(0.1f);
        }

        protected override void Update(float elapsedTime)
        {
            fps = (float)counter / ElapsedTimeSinceLastUpdate;
            counter = 0;

            overallFps = (overallFps * updateCount + fps) / (updateCount + 1);
            updateCount++;

            Add(FramesPerSecond * 10);
        }

        protected override void OnRender(Renderer renderer)
        {
            counter++;
            currentFrame++;
            base.OnRender(renderer);
        }
    }

    class MemoryGraph : LineGraph
    {
        public MemoryGraph()
        {
            this.UpdateFrequency = TimeSpan.FromSeconds(0.1f);
        }

        protected override void Update(float elapsedTime)
        {
            Add(GC.GetTotalMemory(false));
        }
    }

    #endregion
}
