#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Design.Shell
{
    // System.Drawing and the XNA Framework both define Color and Rectangle
    // types. To avoid conflicts, we specify exactly which ones to use.
    using Color = System.Drawing.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;


    /// <summary>
    /// Custom control uses the XNA Framework GraphicsDevice to render onto
    /// a Windows Form. Derived classes can override the Initialize and Draw
    /// methods to add their own drawing code.
    /// </summary>
    public class GameControl : GraphicsDeviceControl
    {
        /// <summary>
        /// Gets the collection of GameComponents owned by the game.
        /// </summary>
        public GameComponentCollection Components { get; private set; }

        /// <summary>
        /// Gets or sets the current ContentManager.
        /// </summary>
        public ContentManager Content { get; set; }

        /// <summary>
        /// Gets or sets the target time between calls to Update when IsFixedTimeStep
        /// is true. Reference page contains links to related code samples.
        /// </summary>
        public TimeSpan TargetElapsedTime { get; set; }


        public event EventHandler Tick;


        /// <summary>
        /// Initializes a new instance of this class, which provides basic graphics device
        /// initialization, game logic, rendering code, and a game loop.
        /// </summary>
        public GameControl()
        {
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);

            Components = new GameComponentCollection();
        }

        /// <summary>
        /// Updates the game's clock and calls Update and Draw.
        /// </summary>
        void tick()
        {
            timer.Interval = (int)TargetElapsedTime.TotalMilliseconds;

            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - start;

            while (elapsed >= TargetElapsedTime)
            {
                gameTime = new GameTime(total, elapsed);

                Update(gameTime);

                Invalidate();

                total += elapsed;
                elapsed -= TargetElapsedTime;

                if (Tick != null)
                    Tick(this, EventArgs.Empty);
            }

            start = now + elapsed;
        }

        protected virtual void Update(GameTime gameTime)
        {
            if (components == null || components.Length < Components.Count)
                components = new IGameComponent[Components.Count];

            if (Components.Count > 0)
            {
                Components.CopyTo(components, 0);

                foreach (IGameComponent component in components)
                {
                    if (component is Microsoft.Xna.Framework.IUpdateable)
                    {
                        Microsoft.Xna.Framework.IUpdateable update = component as Microsoft.Xna.Framework.IUpdateable;

                        if (update.Enabled)
                            update.Update(gameTime);
                    }
                }
            }
        }

        protected sealed override void Draw() 
        {
            Draw(gameTime);
        }

        protected virtual void Draw(GameTime gameTime)
        {
            if (components == null || components.Length < Components.Count)
                components = new IGameComponent[Components.Count];

            if (Components.Count > 0)
            {
                Components.CopyTo(components, 0);

                foreach (IGameComponent component in components)
                {
                    if (component is IDisplayObject)
                    {
                        IDisplayObject draw = component as IDisplayObject;

                        if (draw.Visible)
                            draw.Draw(gameTime);
                    }
                }
            }
        }

        protected override void Initialize()
        {
            Focus();

            if (components == null || components.Length < Components.Count)
                components = new IGameComponent[Components.Count];

            if (Components.Count > 0)
            {
                Components.CopyTo(components, 0);

                foreach (IGameComponent component in Components)
                {
                    component.Initialize();
                }
            }

            LoadContent();

            timer.Enabled = true;
            timer.Interval = (int)TargetElapsedTime.TotalMilliseconds;
            timer.Tick += delegate { tick(); };
            timer.Start();

            start = DateTime.Now;
        }

        protected virtual void LoadContent() { }

        DateTime start;
        GameTime gameTime = new GameTime();
        TimeSpan total = TimeSpan.Zero;

        IGameComponent[] components;

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
    }
}
