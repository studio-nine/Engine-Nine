﻿namespace Microsoft.Xna.Framework
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Graphics;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Input;

    /// <summary>
    /// Provides basic graphics device initialization, game logic, and rendering code.
    /// </summary>
    public class Game : ContentControl
    {
        internal DrawingSurface Surface;

        private bool surfaceLoaded = false;
        private bool suppressDraw = false;        
        private GameTime gameTime = new GameTime();

        public GraphicsDevice GraphicsDevice { get; private set; }
        public GameWindow Window { get; private set; }        
        public GameComponentCollection Components { get; private set; }
        public new ContentManager Content { get; set; }
        public GameServiceContainer Services { get; private set; }
        
        public TimeSpan InactiveSleepTime { get; set; }
        public bool IsActive { get { return true; } }
        public bool IsFixedTimeStep { get; set; }
        public bool IsMouseVisible { get; set; }

        public TimeSpan TargetElapsedTime { get; set; }
        
        public Game()
        {
            IsTabStop = true;
            Services = new GameServiceContainer();
            Components = new GameComponentCollection();
            Content = new ContentManager(Services);
            base.Content = (Surface = new DrawingSurface());

            Window = new GameWindow(this);

            Loaded += new RoutedEventHandler(Game_Loaded);            
            Surface.Draw += new EventHandler<DrawEventArgs>(Surface_Draw);
        }

        /// <summary>
        /// This is used to display an error message if there is no suitable graphics device or sound card.
        /// </summary>
        protected virtual bool ShowMissingRequirementMessage(Exception exception)
        {
            MessageBox.Show(exception.Message, "Your computer does not meet the minimum system requirement.", MessageBoxButton.OK);
            return false;
        }

        void Game_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.RootControl = this;
            Keyboard.RootControl = this;
            Focus();

            surfaceLoaded = true;

            try
            {
                GraphicsDevice = GraphicsDeviceManager.Current.GraphicsDevice;                
                if (GraphicsDevice == null)
                    throw new InvalidOperationException(string.Format(
                        "Failed creating graphics device: {0}", GraphicsDeviceManager.Current.RenderModeReason));

                // Check if GPU is on
                if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
                    throw new InvalidOperationException("Please activate enableGPUAcceleration=true on your Silverlight plugin page.");
            }
            catch (Exception exception)
            {
                if (!ShowMissingRequirementMessage(exception))
                    throw;
            }

            Initialize();
            LoadContent();
            Components.Initialize();
        }

        private void Surface_Draw(object sender, DrawEventArgs e)
        {
            if (surfaceLoaded)
            {
                gameTime.ElapsedGameTime = e.DeltaTime;
                gameTime.TotalGameTime = e.TotalTime;
                
                Update(gameTime);
                Components.Update(gameTime);
                
                if (!suppressDraw && BeginDraw())
                {
                    Draw(gameTime);
                    Components.Draw(gameTime);
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
