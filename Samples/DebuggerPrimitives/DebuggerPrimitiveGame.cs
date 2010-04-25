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
using Isles;
using Isles.Graphics;
using Isles.Graphics.Cameras;
#endregion


namespace DebuggerPrimitives
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DebuggerPrimitiveGame : Microsoft.Xna.Framework.Game
    {
        ModelViewerCamera camera;

        public DebuggerPrimitiveGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            camera = new ModelViewerCamera(this);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            GraphicsDevice.RenderState.SetSpriteBlendMode(SpriteBlendMode.AlphaBlend);


            DebugVisual.View = camera.View;
            DebugVisual.Projection = camera.Projection;
            DebugVisual.Alpha = 0.8f;


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
