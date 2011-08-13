using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Animations;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ScreenEffects;

namespace KinectMagician
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KinectMagicianGame : Microsoft.Xna.Framework.Game
    {
        ScreenCamera camera;
        ScreenEffect postProcess;
        ScreenshotCapturer screenshot;
        ModelViewerCamera modelViewerCamera;
        ParticleBatch particlePatch;
        ModelBatch modelBatch;
        PrimitiveBatch primitiveBatch;
        SpriteBatch spriteBatch;
        Kinect kinect;
        SpriteFont font;
        List<Magic> allMagic;


        public KinectMagicianGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            Window.Title = "Kinect Magician";
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(screenshot = new ScreenshotCapturer(GraphicsDevice));
            Components.Add(new InputComponent(Window.Handle));

            font = Content.Load<SpriteFont>("Consolas");

            modelViewerCamera = new ModelViewerCamera(GraphicsDevice);
            camera = new ScreenCamera(GraphicsDevice, ScreenCameraCoordinate.TwoDimension);
            camera.Input.Enabled = false;
            kinect = new Kinect(GraphicsDevice);

            particlePatch = new ParticleBatch(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // FIXME: Hdr post processing conflicts with kinect skeleton recognition...
            postProcess = ScreenEffect.CreateHighDynamicRange(GraphicsDevice, 0.8f, 1f, 4f, 5f, 1);
            //postProcess = ScreenEffect.CreateEffect(new ColorMatrixEffect(GraphicsDevice) { Transform = ColorMatrix.CreateSaturation(.5f) });
            postProcess.Enabled = false;

            allMagic = new List<Magic>();
            allMagic.Add(new Fireball(Content));
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            kinect.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            postProcess.Begin();

            spriteBatch.Begin();
            spriteBatch.Draw(kinect.ColorTexture, GraphicsDevice.Viewport.Bounds, Color.White);
            //spriteBatch.Draw(kinect.DepthTexture, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.End();

            GraphicsDevice.Textures[0] = null;

            particlePatch.Begin(camera.View, camera.Projection, null, null, RasterizerState.CullNone);
            primitiveBatch.Begin(0, modelViewerCamera.View, modelViewerCamera.Projection, null, null, DepthStencilState.None, null);

            if (gameTime.ElapsedGameTime != TimeSpan.Zero)
            {
                // Draw particle system using ParticleBatch
                foreach (var skeleton in kinect.Skeletons)
                {
                    Magic firstActiveMagic = null;

                    foreach (var magic in allMagic)
                    {
                        bool isActive = magic.Update(gameTime.ElapsedGameTime, skeleton);
                        if (isActive && firstActiveMagic == null)
                            firstActiveMagic = magic;
                    }

                    foreach (var magic in allMagic)
                    {
                        magic.Disable(skeleton);
                    }

                    if (firstActiveMagic != null && skeleton.IsTracked)
                    {
                        firstActiveMagic.Enable(skeleton);
                    }

                    if (skeleton.IsTracked)
                        primitiveBatch.DrawSkeleton(skeleton, Matrix.CreateScale(4), Color.Yellow);
                }

                foreach (var magic in allMagic)
                {
                    magic.Draw(gameTime.ElapsedGameTime, particlePatch);
                }
            }

            primitiveBatch.End();
            particlePatch.End();
            
            postProcess.End();

            DrawStatus();

            base.Draw(gameTime);
        }

        int currentFrame = 0;

        [Conditional("DEBUG")]
        private void DrawStatus()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Tracked: " + kinect.Skeletons.Count(s => s.IsTracked).ToString(), new Vector2(0, GraphicsDevice.Viewport.Bounds.Bottom - 20), Color.Yellow);

            var activeSkeleton = kinect.Skeletons.FirstOrDefault(s => s.IsTracked);
            if (activeSkeleton != null)
            {
                float speed = ((KinectSkeletonTag)activeSkeleton.Tag).LeftHand.Velocity.Length();
                
                //System.Diagnostics.Trace.WriteLine(((KinectSkeletonTag)activeSkeleton.Tag).LeftHand.Velocity.Length().ToString());
                spriteBatch.DrawString(font, ((KinectSkeletonTag)activeSkeleton.Tag).LeftHand.Velocity.Length().ToString("00.00"),
                    new Vector2(0, 200), ((KinectSkeletonTag)activeSkeleton.Tag).LeftHand.IsSwiping ? Color.Red : Color.Black, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }
    }
}
