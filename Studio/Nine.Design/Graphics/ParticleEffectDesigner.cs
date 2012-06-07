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
using System.ComponentModel.Composition;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Components;
using Nine.Studio;
using Nine.Studio.Extensibility;
using Nine.Studio.Visualizers;
#endregion

namespace Nine.Graphics.ParticleEffects.Design
{
    [Export(typeof(IFactory))]
    [LocalizedCategory("Graphics")]
    [LocalizedDisplayName("ParticleEffect")]
    public class ParticleEffectDocumentFactory : Factory<ParticleEffect, object>
    {

    }

    [Export(typeof(IVisualizer))]
    public class AnotherParticleEffectVisualizer : GameVisualizer<ParticleEffect>
    {
        [EditorBrowsable()]
        [LocalizedDisplayName("ShowWireframe")]
        public bool ShowWireframe { get; set; }

        public AnotherParticleEffectVisualizer()
        {
            DisplayName = string.Format(Strings.ViewFormat, Strings.ParticleEffect);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            base.Draw(gameTime);
        }
    }

    [Export(typeof(IVisualizer))]
    public class ParticleEffectVisualizer : GameVisualizer<ParticleEffect>
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

        [EditorBrowsable()]
        [LocalizedDisplayName("ShowWireframe")]
        public bool ShowWireframe { get; set; }

        [EditorBrowsable()]
        [LocalizedDisplayName("ParticleEffect")]
        public GraphicsProfile GraphicsProfile { get; set; }

        public ParticleEffectVisualizer()
        {
            DisplayName = string.Format(Strings.ViewFormat, Strings.ParticleEffect);

            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            GraphicsProfile = Microsoft.Xna.Framework.Graphics.GraphicsProfile.HiDef;

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, null));
            Components.Add(new InputComponent());

            camera = new ModelViewerCamera(GraphicsDevice);
            
            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            BoundingFrustum frustum = new BoundingFrustum(
                Matrix.CreateLookAt(new Vector3(0, 15, 15), Vector3.Zero, Vector3.UnitZ) *
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 10));

            base.Draw(gameTime);
        }
    }
}
