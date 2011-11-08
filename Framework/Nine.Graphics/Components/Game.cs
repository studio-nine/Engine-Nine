#region File Description
//-----------------------------------------------------------------------------
// GameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Provides basic graphics device initialization, game logic, and rendering code.
    /// </summary>
    public class Game : UserControl
    {
        private bool surfaceLoaded = false;
        private bool suppressDraw = false;
        private DrawingSurface Surface;
        private GameTime gameTime = new GameTime();

        public GraphicsDevice GraphicsDevice { get; private set; }
        public GameWindow Window { get; private set; }        
        public GameComponentCollection Components { get; private set; }
        public new ContentManager Content { get; private set; }

        public TimeSpan InactiveSleepTime { get; set; }
        public bool IsActive { get { return true; } }
        public bool IsFixedTimeStep { get; set; }
        public bool IsMouseVisible { get; set; }

        public TimeSpan TargetElapsedTime { get; set; }
        
        public Game()
        {
            Width = 800;
            Height = 600;

            Mouse.RootControl = this;
            Keyboard.RootControl = this;

            Window = new GameWindow();
            Components = new GameComponentCollection();
            Content = new ContentManager(null);
            base.Content = (Surface = new DrawingSurface());

            Loaded += new RoutedEventHandler(Game_Loaded);
            Surface.Draw += new EventHandler<DrawEventArgs>(Surface_Draw);
        }

        void Game_Loaded(object sender, RoutedEventArgs e)
        {
            surfaceLoaded = true;
            
            GraphicsDevice = GraphicsDeviceManager.Current.GraphicsDevice;
            if (GraphicsDevice == null)
                throw new InvalidOperationException("Failed creating graphics device.");

            // Check if GPU is on
            if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
                MessageBox.Show("Please activate enableGPUAcceleration=true on your Silverlight plugin page.", "Warning", MessageBoxButton.OK);

            Initialize();
            LoadContent();
        }

        private void Surface_Draw(object sender, DrawEventArgs e)
        {
            if (surfaceLoaded)
            {
                gameTime.ElapsedGameTime = e.DeltaTime;
                gameTime.TotalGameTime = e.TotalTime;
                
                Update(gameTime);

                if (!suppressDraw && BeginDraw())
                {
                    Draw(gameTime);
                    EndDraw();
                }
                suppressDraw = false;

                e.InvalidateSurface();
            }
        }

        protected virtual void Initialize() { }
        protected virtual void LoadContent() { }

        protected virtual bool BeginDraw() { return true; }
        protected virtual void Update(GameTime gameTime) { }
        protected virtual void Draw(GameTime gameTime) { }
        protected virtual void EndDraw() { }

        public void SuppressDraw() { suppressDraw = true; }
    }
}
