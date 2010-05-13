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
using Isles.Graphics.ScreenEffects;
using Isles.Graphics.ParticleEffects;
using Isles.Transitions;
#endregion

namespace ParticleSystem
{
    /// <summary>
    /// Demonstrate how to use ParticleBatch to draw point sprites, textured lines and particle effects.
    /// </summary>
    public class ParticleSystemGame : Microsoft.Xna.Framework.Game
    {
        TopDownEditorCamera camera;
        ParticleBatch particles;

        SpriteAnimation fireball;
        Texture2D lightning;
        Tweener<Vector2> lightningOffset;

        ParticleEffect snow;

        public ParticleSystemGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            camera = new TopDownEditorCamera(this);

            particles = new ParticleBatch(GraphicsDevice, 1024);

            fireball = new SpriteAnimation(Content.Load<ImageList>("fireball"));

            lightning = Content.Load<Texture2D>("lightning");

            // This tweener is used to animate the lightning texture.
            lightningOffset = new Tweener<Vector2>
            {
                Curve = new Linear(),
                Style = LoopStyle.Loop,
                To = Vector2.UnitY,
            };


            snow = new ParticleEffect(600);
            snow.Texture = Content.Load<Texture2D>("flake");
            snow.Duration = TimeSpan.FromSeconds(1.5f);
            snow.Gravity = Vector3.Zero;
            snow.Emitter.Emission = 400;
            snow.StartSize = 0.8f;
            snow.EndSize = 0.4f;
            snow.EndVelocity = 1.0f;
            snow.HorizontalVelocity = 0;
            snow.VerticalVelocity = -40.0f;
            
            snow.SpacialEmitter = new BoxEmitter()
            {
                Box = new BoundingBox(new Vector3(-30, -30, 30), new Vector3(30, 30, 30))
            };
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Prepare render states used to draw particles.
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;


            GraphicsDevice.SetVertexBuffer(null);

            // Update effects
            fireball.Update(gameTime);

            snow.Update(gameTime);


            particles.Begin(camera.View, camera.Projection);

            // Draw lightning
            particles.DrawLine(lightning, Vector3.Zero, Vector3.UnitX * 20, 10, Vector2.One, lightningOffset.Update(gameTime), Color.White);

            // Draw fireball
            particles.Draw(fireball.Texture, Vector3.Zero, 10, 0, Color.White);

            // Draw snow flake
            particles.Draw(snow, gameTime);

            particles.End();


            base.Draw(gameTime);
        }
    }
}
