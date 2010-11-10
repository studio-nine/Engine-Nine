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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using Nine.Animations;
#endregion

namespace SkinnedModel
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class SkinnedModelGame : Microsoft.Xna.Framework.Game
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
        BoneAnimation animation;
        ModelBatch modelBatch;
        Input input;
        ModelViewerCamera camera;


        public SkinnedModelGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a model viewer camera to help us visualize the scene
            camera = new ModelViewerCamera(GraphicsDevice);

            // Model batch makes drawing models easier
            modelBatch = new ModelBatch(GraphicsDevice);

            // Load our model assert.
            // If the model is processed by our ExtendedModelProcesser,
            // we will try to add model animation and skinning data.
            model = Content.Load<Model>("peon");

            PlayAnimation(0);
            //animation.Seek(animation.Duration);

            input = new Input();
            input.MouseDown += (o, e) => 
            {
                if (e.Button == MouseButtons.Left)
                {
                    Random random = new Random();
                    PlayAnimation(random.Next(model.GetAnimations().Count));
                }
            };
        }

        private void PlayAnimation(int i)
        {
            if (animation != model.GetAnimation(i))
            {
                // Now load our model animation and skinning using extension method.
                animation = model.GetAnimation(i);
                //animation.Speed = 0.04f;
                //animation.Ending = KeyframeEnding.Clamp;
                //animation.BlendEnabled = false;
                //animation.BlendDuration = TimeSpan.FromSeconds(1);
                //animation.InterpolationEnabled = false;
                //animation.Repeat = 1.5f;
                animation.AutoReverse = true;
                //animation.StartupDirection = AnimationDirection.Backward;
                animation.Disable(1, false);
                animation.Play();
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update model animation.
            // Note how animations and skinning are seperated.
            if (animation != null)
                animation.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            Matrix world = Matrix.CreateTranslation(0, -60, 0) *
                           Matrix.CreateScale(0.1f);           
            

            // Gets the pick ray from current mouse cursor
            Ray ray = GraphicsDevice.Viewport.CreatePickRay(Mouse.GetState().X,
                                                            Mouse.GetState().Y,
                                                            camera.View,
                                                            camera.Projection);
            // Do ray model intersection test
            float? distance = model.Intersects(world, ray);

            //Window.Title = distance.HasValue ? "Picked" : "Nothing";

            // To draw skinned models, first compute bone transforms
            Matrix[] bones = model.GetBoneTransforms();

            // Pass bone transforms to model batch to draw skinned models
            modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
            modelBatch.Draw(model, world, bones, null);
            modelBatch.End();

            // Draw collision tree
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                DrawCollisionTree(model, world);

            base.Draw(gameTime);
        }

        private void DrawCollisionTree(Model model, Matrix transform)
        {
            Matrix[] bones = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(bones);

            transform = bones[model.Meshes[0].ParentBone.Index] * transform;

            Octree<bool> tree = (model.Tag as ModelTag).Collision.CollisionTree;

            DebugVisual.View = camera.View;
            DebugVisual.Projection = camera.Projection;

            foreach (OctreeNode<bool> node in tree.Traverse((o) => { return true; }))
            {
                if (!node.HasChildren && node.Value)
                    DebugVisual.DrawBox(GraphicsDevice, node.Bounds, transform, Color.Yellow);
            }
        }
    }
}
