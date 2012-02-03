#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Components;
using Nine.Graphics;
#endregion

namespace CustomEffects
{
    [Category("Graphics")]
    [DisplayName("Custom Effects")]
    [Description("This sample demenstrates the usage of standard effect annotations and semantics.")]
    public class CustomEffectGame : Microsoft.Xna.Framework.Game
    {
#if WINDOWS_PHONE
        private const int TargetFrameRate = 30;
        private const int BackBufferWidth = 800;
        private const int BackBufferHeight = 480;
#elif XBOX
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 720;
#else
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 900;
        private const int BackBufferHeight = 600;
#endif

        Model model;
        Input input;
        string[] effectFiles;
        Effect[] effects;
        int currentEffect;
        ModelBatch modelBatch;
        SpriteBatch spriteBatch;
        SpriteFont font;
        ModelViewerCamera camera;

        public CustomEffectGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

            camera = new ModelViewerCamera(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Consolas");

            model = Content.Load<Model>("tank");

            effectFiles = Content.Load<string[]>(@"Shaders\Shaders");
            effects = effectFiles.Select(file => Content.Load<Effect>(file)).ToArray();

            var random = new Random();
            currentEffect = random.Next(effects.Length);

            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(input_MouseDown);
            input.ButtonDown += new EventHandler<GamePadEventArgs>(input_ButtonDown);
        }

        void input_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                currentEffect = (currentEffect + 1) % effects.Length;
        }

        private void input_ButtonDown(object sender, GamePadEventArgs e)
        {
            if (e.Button == Buttons.A)
                currentEffect = (currentEffect + 1) % effects.Length;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            Nine.IUpdateable updateable = effects[currentEffect] as Nine.IUpdateable;
            if (updateable != null)
                updateable.Update(gameTime.ElapsedGameTime);

            // Draw sprites first in case the custom effect does not have both pixel shader and vertex shader.
            spriteBatch.Begin();
            spriteBatch.DrawString(font, effectFiles[currentEffect], new Vector2(0, 30), Color.Yellow);
            spriteBatch.End();

            modelBatch.Begin(camera.View, camera.Projection);
            modelBatch.Draw(model, Matrix.Identity, effects[currentEffect]);
            modelBatch.End();

            base.Draw(gameTime);
        }
    }
}
