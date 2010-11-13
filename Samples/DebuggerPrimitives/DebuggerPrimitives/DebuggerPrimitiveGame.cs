#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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
using Nine;
using Nine.Graphics;
#endregion

namespace DebuggerPrimitives
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DebuggerPrimitiveGame : Microsoft.Xna.Framework.Game
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

        ModelViewerCamera camera;

        public DebuggerPrimitiveGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Components.Add(new FrameRate(this, "Consolas"));
            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            camera = new ModelViewerCamera(GraphicsDevice);

            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;


            DebugVisual.View = camera.View;
            DebugVisual.Projection = camera.Projection;
            DebugVisual.Alpha = 0.4f;


            DebugVisual.DrawBox(GraphicsDevice, new BoundingBox(-Vector3.One, Vector3.One), Matrix.Identity, Color.Yellow);
            DebugVisual.DrawSphere(GraphicsDevice, new BoundingSphere(Vector3.Zero, 1), Color.Red);
            DebugVisual.DrawAxis(GraphicsDevice, Matrix.Identity);
            DebugVisual.DrawArrow(GraphicsDevice, Vector3.Zero, Vector3.One, 1.0f, Color.White);
            DebugVisual.DrawPoint(GraphicsDevice, Vector3.One, Color.Black, 0.2f);
            DebugVisual.DrawLine(GraphicsDevice, Vector3.Zero, Vector3.UnitZ, 1, Color.Cornsilk);


            base.Draw(gameTime);
        }
    }
}