#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Animations;
using Nine.Graphics;
using Nine.Navigation;
#endregion

namespace NavigationSample
{
    /// <summary>
    /// A simple implementation of movable and controllable unit.
    /// </summary>
    public class Unit : IPickable
    {
        public Navigator Navigator { get; private set; }
        public Model Model { get; private set; }

        ModelSkeleton skeleton;
        AnimationPlayer animations;
        BoneAnimation idleAnimation;
        BoneAnimation runAnimation;

        public static Random Random = new Random();
        public static Matrix WorldTransform = Matrix.CreateScale(0.016f) * Matrix.CreateRotationX(MathHelper.PiOver2);

        public Unit(Model model, ISurface ground, ISpatialQuery<Navigator> friends, ISpatialQuery<LineSegment> walls)
        {
            Model = model;

            skeleton = new ModelSkeleton(model);
            idleAnimation = new BoneAnimation(skeleton, model.GetAnimation("Idle"));
            runAnimation = new BoneAnimation(skeleton, model.GetAnimation("Run"));

            idleAnimation.BlendDuration = TimeSpan.FromSeconds(0.25f);
            runAnimation.BlendDuration = TimeSpan.FromSeconds(0.25f);

            animations = new AnimationPlayer();
            animations.Play(idleAnimation);

            //float bound = 0.3f + (float)Random.NextDouble() * 1f;
            float bound = 0.5f;

            Navigator = new Navigator();
            Navigator.IsMachinery = false;
            Navigator.MaxSpeed = 6;
            Navigator.SoftBoundingRadius = 0.5f;
            Navigator.HardBoundingRadius = 0.5f;
            Navigator.Acceleration = 60;
            Navigator.Ground = ground;
            Navigator.Friends = friends;
            Navigator.Walls = walls;
            Navigator.Started += new EventHandler<EventArgs>(Navigator_Started);
            Navigator.Stopped += new EventHandler<EventArgs>(Navigator_Stopped);
        }

        void Navigator_Stopped(object sender, EventArgs e)
        {
            if (animations.Current != idleAnimation)
                animations.Play(idleAnimation);
        }

        void Navigator_Started(object sender, EventArgs e)
        {
            if (animations.Current != runAnimation)
                animations.Play(runAnimation);
        }

        public bool Contains(Vector3 point)
        {
            return Model.Contains(WorldTransform * Navigator.Transform, point);
        }

        public float? Intersects(Ray ray)
        {
            return Model.Intersects(WorldTransform * Navigator.Transform, ray);
        }

        public void Update(GameTime gameTime)
        {
            Navigator.Update(gameTime.ElapsedGameTime);

            //animations.Update(gameTime);
        }

        public void Draw(GameTime gameTime, ModelBatch modelBatch, PrimitiveBatch primitiveBatch)
        {
            BoneAnimation animation = (BoneAnimation)(animations.Current);

            modelBatch.DrawSkinned(Model, WorldTransform * Navigator.Transform, skeleton.GetSkinTransforms(), null);
            //modelBatch.Draw(Model, WorldTransform * Navigator.Transform, null);
        }
    }
}
