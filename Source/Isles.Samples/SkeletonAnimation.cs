#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles.Components;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Graphics.Primitives;
using Isles.Graphics.Models;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class SkeletonAnimationGame : BasicModelViewerGame
    {
        Model model;
        bool showSkeleton = false;
        Skeleton skeleton;
        BasicEffect basicEffect;
        ModelAnimation animation;
        ModelSkinning skinner;
        Matrix world = Matrix.Identity;
        ModelBatch batch;

        public SkeletonAnimationGame()
        {
            Camera.Input.KeyDown += new EventHandler<KeyboardEventArgs>(Input_KeyDown);
        }

        void Input_KeyDown(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.S)
                showSkeleton = !showSkeleton;
        }

        protected override void LoadContent()
        {
            skeleton = new Skeleton(GraphicsDevice);

            basicEffect = new BasicEffect(GraphicsDevice, null);

            model = Content.Load<Model>("Dude/Dude");
            batch = new ModelBatch();

            animation = new ModelAnimation(model, null);

            animation.AnimationClip = ModelExtensions.GetAnimation(model, 0);
            animation.Play();

            skinner = ModelExtensions.GetSkinning(model);

            BoundingSphere sphere = new BoundingSphere();

            if (model.Meshes.Count > 0)
                sphere = model.Meshes[0].BoundingSphere;

            world = Matrix.CreateTranslation(-sphere.Center) *
                    Matrix.CreateScale(10.0f / sphere.Radius);
            
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (animation != null)
                animation.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);


            if (showSkeleton)
                skeleton.Draw(model.Bones[0], world, Camera.View, Camera.Projection, Color.Blue);

            batch.Begin();
            batch.Draw(model, skinner.GetBoneTransform(model, world), Camera.View, Camera.Projection);
            batch.End();


            if (model != null && model.Meshes.Count > 0)
            {
                FrameRate.SpriteBatch.Begin();
                FrameRate.SpriteBatch.DrawString(FrameRate.Font, "Primitive Count: " + model.Meshes[0].IndexBuffer.SizeInBytes / 6, new Vector2(0, 400), Color.Yellow);
                FrameRate.SpriteBatch.DrawString(FrameRate.Font, "Mesh Size: " + model.Meshes[0].BoundingSphere.Radius, new Vector2(0, 425), Color.Yellow);
                FrameRate.SpriteBatch.End();
            }
        }
    
        [SampleMethod]
        public static void Test()
        {
            using (SkeletonAnimationGame game = new SkeletonAnimationGame())
            {
                game.Run();
            }
        }
    }
}
