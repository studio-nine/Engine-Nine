#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles.Components;
using Isles.Graphics;
using Isles.Graphics.Primitives;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class Components
    {
        private class FrameRateSample : SampleGame
        {
            FrameRate component;
            public FrameRateSample()
            {
                component = new FrameRate(this);
                component.Color = Color.Yellow;
                Components.Add(component);
            }

            protected override void LoadContent()
            {
                component.Font = Content.Load<SpriteFont>("Fonts/Arial");

                base.LoadContent();
            }
        }


        //[SampleMethod]
        public static void FrameRate()
        {
            using (Microsoft.Xna.Framework.Game game = new FrameRateSample())
            {
                game.Run();
            }
        }


        //[SampleMethod]
        public static void TimerTest()
        {
            using (Microsoft.Xna.Framework.Game game = new SampleGame())
            {
                Timer timer = new Timer(game, 1000);

                timer.Tick += new EventHandler(timer_Tick);

                game.Components.Add(timer);
                game.Run();
            }
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Tick");
        }


        //[SampleMethod]
        public static void GameConsole()
        {
            using (Microsoft.Xna.Framework.Game game = new SampleGame())
            {
                GameConsole console = new GameConsole(game, "Fonts/Arial");

                console.Enabled = console.Visible = true;
                console.BackgroundColor = Color.Yellow;
                console.ForegroundColor = Color.Black;
                console.WriteLine("Game Console: " + game.Window.Title);
                console.WriteLine("Game Console: " + game.Window.Title);

                game.Components.Add(console);
                game.Run();
            }
        }

        //[SampleMethod]
        public static void ScreenshotTest()
        {
            using (Microsoft.Xna.Framework.Game game = new SampleGame())
            {
                FrameRate component;

                component = new FrameRate(game, "Fonts/Arial");
                component.Color = Color.Yellow;


                ScreenshotCapturer screenshot = new ScreenshotCapturer(game);

                screenshot.Captured += new EventHandler(delegate(object sender, EventArgs args)
                {
                    System.Windows.Forms.MessageBox.Show((sender as ScreenshotCapturer).LastScreenshotFile);
                });

                game.Components.Add(screenshot);
                game.Components.Add(component);
                game.Run();
            }
        }
    }
}
