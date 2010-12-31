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
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Graphics;
using Nine.Navigation;
using Nine.Animations;
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

        AnimationPlayer animations;
        BoneAnimation idleAnimation;
        BoneAnimation runAnimation;
        
        public static Matrix WorldTransform = Matrix.CreateScale(0.01f) * Matrix.CreateRotationX(MathHelper.PiOver2);

        public Unit(Model model, ISurface ground, ISpatialQuery<Navigator> friends)
        {
            Model = model;

            idleAnimation = new BoneAnimation(model, this, model.GetAnimation("Idle"));
            runAnimation = new BoneAnimation(model, this, model.GetAnimation("Run"));

            idleAnimation.BlendDuration = TimeSpan.FromSeconds(0.25f);
            runAnimation.BlendDuration = TimeSpan.FromSeconds(0.25f);

            animations = new AnimationPlayer();
            animations.Play(idleAnimation);

            Navigator = new Navigator();
            Navigator.IsMachinery = true;
            Navigator.MaxSpeed = 2.5f;
            Navigator.BoundingRadius = 0.5f;
            Navigator.Acceleration = float.MaxValue;
            Navigator.Ground = ground;
            Navigator.Friends = friends;
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
            Navigator.Update(gameTime);

            animations.Update(gameTime);
        }

        public void Draw(GameTime gameTime, ModelBatch modelBatch, PrimitiveBatch primitiveBatch)
        {
            BoneAnimation animation = (BoneAnimation)(animations.Current);

            modelBatch.DrawSkinned(Model, WorldTransform * Navigator.Transform, animation.GetBoneTransforms(), null);
            //modelBatch.Draw(Model, WorldTransform * Navigator.Transform, null);
        }
    }
}
