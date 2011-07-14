#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Animations;
using Nine.Components;
#endregion

namespace AvatarAnimationGame
{
    [Category("Graphics")]
    [DisplayName("Avatar Animation")]
    [Description("This sample demenstrates how to play avatar animations.")]
    public class AvatarAnimationGame : Microsoft.Xna.Framework.Game
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

        Model hammer;
        Input input;
        PrimitiveBatch primitiveBatch;
        Random random = new Random();
        ModelViewerCamera camera;

        BoneAnimationClip walk;
        LookAtController lookAtController;

        AvatarRenderer avatar1;
        AvatarRenderer avatar2;
        AvatarRenderer avatar3;
        
        AvatarSkeleton skeleton1;
        AvatarSkeleton skeleton2;
        AvatarSkeleton skeleton3;

        AvatarBoneAnimation animation1;
        AvatarBoneAnimation animation2;
        AvatarBoneAnimation animation3;

        Matrix world1 = Matrix.CreateTranslation(-1, -0.5f, 0) * Matrix.CreateScale(20f);
        Matrix world2 = Matrix.CreateTranslation(0, -0.5f, 0) * Matrix.CreateScale(20f);
        Matrix world3 = Matrix.CreateTranslation(1, -0.5f, 0) * Matrix.CreateScale(20f);

        public AvatarAnimationGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            Components.Add(new GamerServicesComponent(this));
            SignedInGamer.SignedIn += new EventHandler<SignedInEventArgs>(SignedInGamer_SignedIn);
        }

        void SignedInGamer_SignedIn(object sender, SignedInEventArgs e)
        {
            // Only load the avatar for player one
            if (e.Gamer.PlayerIndex == PlayerIndex.One)
            {
                // Load the player one avatar
                AvatarDescription.BeginGetFromGamer(Gamer.SignedInGamers[0], LoadAvatarDescription, null);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

            // Create a model viewer camera to help us visualize the scene
            camera = new ModelViewerCamera(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);

            hammer = Content.Load<Model>("hammer");

            walk = Content.Load<BoneAnimationClip>("walk");

            avatar1 = new AvatarRenderer(AvatarDescription.CreateRandom());
            avatar2 = new AvatarRenderer(AvatarDescription.CreateRandom());
            avatar3 = new AvatarRenderer(AvatarDescription.CreateRandom());

            skeleton1 = new AvatarSkeleton(avatar1);
            skeleton2 = new AvatarSkeleton(avatar2);
            skeleton3 = new AvatarSkeleton(avatar3);

            // Handle animations.
            PlayStandAndWave();
            PlayLookAt();
            PlayAnimation(0);

            input = new Input();
            input.ButtonDown +=new EventHandler<GamePadEventArgs>(input_ButtonDown);
        }

        private void LoadAvatarDescription(IAsyncResult result)
        {
            // Get the AvatarDescription for the gamer
            var avatarDescription = AvatarDescription.EndGetFromGamer(result);

            // Load the AvatarRenderer if description is valid
            if (avatarDescription.IsValid)
            {
                avatar2 = new AvatarRenderer(avatarDescription);
                skeleton2 = new AvatarSkeleton(avatar2);
                PlayLookAt();
            }
        }

        private void input_ButtonDown(object sender, GamePadEventArgs e)
        {
            if (e.Button == Buttons.A)
            {
                PlayAnimation((AvatarAnimationPreset)(random.Next(30)));
            }
        }

        private void PlayAnimation(AvatarAnimationPreset preset)
        {
            animation1 = new AvatarBoneAnimation(skeleton1, preset);
            animation1.Play();
        }

        private void PlayLookAt()
        {
            // Blend between 3 animation channels
            AvatarAnimationController idle = new AvatarAnimationController(avatar2, walk);

            AvatarBoneAnimation blended = new AvatarBoneAnimation(skeleton2);
            blended.Controllers.Add(idle);

            lookAtController = new LookAtController(blended.Skeleton, world2, (int)AvatarBone.Head);
            lookAtController.Up = Vector3.UnitY;
            lookAtController.Forward = Vector3.UnitZ;
            lookAtController.HorizontalRotation = new Range<float>(-MathHelper.PiOver2, MathHelper.PiOver2);
            lookAtController.VerticalRotation = new Range<float>(-MathHelper.PiOver2, MathHelper.PiOver2);
            blended.Controllers.Add(lookAtController);

            // Give the look at controller a huge blend weight so it will dominate the other controllers.
            // All the weights will be normalized during blending.
            blended.Controllers[lookAtController].BlendWeight = 10;

            animation2 = blended;
            animation2.Play();
        }

        private void PlayStandAndWave()
        {
            AvatarAnimationController attack = new AvatarAnimationController(avatar3, AvatarAnimationPreset.Stand0);
            AvatarAnimationController run = new AvatarAnimationController(avatar3, AvatarAnimationPreset.Wave);

            AvatarBoneAnimation blended = new AvatarBoneAnimation(skeleton3);
            blended.Controllers.Add(run);
            blended.Controllers.Add(attack);

            blended.KeyController = run;
            blended.IsSychronized = true;

            animation3 = blended;
            animation3.Play();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Makes the model looks at the camera
            lookAtController.Target = Matrix.Invert(camera.View).Translation;

            animation1.Update(gameTime.ElapsedGameTime);
            animation2.Update(gameTime.ElapsedGameTime);
            animation3.Update(gameTime.ElapsedGameTime);

            GraphicsDevice.Clear(Color.DarkSlateGray);

            if (!GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B))
            {
                avatar1.World = world1;
                avatar1.View = camera.View;
                avatar1.Projection = camera.Projection;
                avatar1.Draw(animation1);

                avatar2.World = world2;
                avatar2.View = camera.View;
                avatar2.Projection = camera.Projection;
                avatar2.Draw(animation2);

                avatar3.World = world3;
                avatar3.View = camera.View;
                avatar3.Projection = camera.Projection;
                avatar3.Draw(animation3);

                // Attach the hammer model to the character
                if (avatar2.State == AvatarRendererState.Ready)
                {
                    Matrix hammerTransform = skeleton2.GetAbsoluteBoneTransform((int)AvatarBone.FingerIndex2Right) * world2;
                    Matrix hammerBaseTransform = Matrix.CreateScale(0.005f) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(0, -0.05f, 0);
                    hammer.Draw(hammerBaseTransform * hammerTransform, camera.View, camera.Projection);
                }
            }
            else
            {
                primitiveBatch.Begin(camera.View, camera.Projection);
                {
                    primitiveBatch.DrawSkeleton(skeleton1, world1, Color.White);
                    primitiveBatch.DrawSkeleton(skeleton2, world2, Color.White);
                    primitiveBatch.DrawSkeleton(skeleton3, world3, Color.White);

                    primitiveBatch.DrawAxis(skeleton2.GetAbsoluteBoneTransform((int)AvatarBone.FingerIndex2Right) * world2, 5);
                    primitiveBatch.DrawAxis(skeleton2.GetAbsoluteBoneTransform((int)AvatarBone.Head) * world2, 5);
                }
                primitiveBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
