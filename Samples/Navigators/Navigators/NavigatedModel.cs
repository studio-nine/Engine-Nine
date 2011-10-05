using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Animations;
using Nine.Graphics;
using Nine.Graphics.ObjectModel;
using Nine.Navigation;

namespace Navigators
{
    public class NavigatedModel : Drawable, ISpatialQueryable, Nine.IUpdateable
    {
        #region Fields

        Navigator navigator;
        Model model;
        AnimationPlayer animations;
        ModelSkeleton skeleton;

        BoundingBox orientedBoundingBox;
        BoundingBox boundingBox;

        float lastSpeed = 0;

        #endregion

        #region Properties

        public Navigator Navigator
        {
            get { return navigator; }
        }

        public override BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        public Matrix OriginalRotation { get; set; }
        public Matrix OriginalScale { get; set; }

        public string IdleAnimationName { get; set; }
        public string RunAnimationName { get; set; }

        #endregion

        #region Initialization

        public NavigatedModel(Model model)
        {
            navigator = new Navigator();
            this.model = model;
            orientedBoundingBox = model.ComputeBoundingBox();
            Navigator.SoftBoundingRadius = 3;
            Navigator.HardBoundingRadius = 2;
            OriginalRotation = Matrix.CreateRotationX(MathHelper.PiOver2);
            OriginalScale = Matrix.CreateScale(0.1f);
            navigator.MaxSpeed = 3;
            IdleAnimationName = "Idle";
            RunAnimationName = "Run";   

        }

        public void LoadContent()
        {
            skeleton = new ModelSkeleton(model);
            animations = new AnimationPlayer();

            PlayAnimation(IdleAnimationName);

            Matrix world = OriginalRotation;
            world.Forward = new Vector3(navigator.Forward.X, navigator.Forward.Y, world.Forward.Z);
            world *= OriginalScale * Matrix.CreateTranslation(navigator.Position);
            Transform = world;
        }

        #endregion

        #region Private Methods and Generic Overrides

        public void PlayAnimation(string animation)
        {
            BoneAnimationController run = new BoneAnimationController(model.GetAnimation(animation));
            if (animation == IdleAnimationName)
                run.Speed = 0.8f;
            else
                run.Speed = 1;
            BoneAnimation blended = new BoneAnimation(skeleton);
            blended.Controllers.Add(run);
            blended.KeyController = run;
            blended.IsSychronized = true;

            animations[2].Play(blended);
        }

        protected override void OnTransformChanged()
        {
            boundingBox = orientedBoundingBox.CreateAxisAligned(Transform);
            base.OnTransformChanged();
        }


        #endregion

        #region Update
       
        public new void Update(TimeSpan elapsedTime)
        {
            animations.Update(elapsedTime);
            navigator.Update(elapsedTime);
            if (navigator.Speed == 0 && navigator.Speed != lastSpeed)
            {
                PlayAnimation(IdleAnimationName);
                lastSpeed = 0;
            }
            else if (navigator.Speed != 0 && lastSpeed == 0)
            {
                PlayAnimation(RunAnimationName);
            }
            if (navigator.Speed > 0)
            {
                Matrix world = OriginalRotation;
                world.Forward = new Vector3(navigator.Forward.X, navigator.Forward.Y, world.Forward.Z);
                world *= OriginalScale * Matrix.CreateTranslation(navigator.Position);
                Transform = world;
            }
            lastSpeed = navigator.Speed;

            base.Update(elapsedTime);
        }

        void Nine.IUpdateable.Update(TimeSpan elapsedTime)
        {
            Update(elapsedTime);
        }

        #endregion

        #region Draw

        public override void Draw(GraphicsContext context, Effect effect)
        {
            Draw(context);
        }

        public override void Draw(GraphicsContext context)
        {
            context.ModelBatch.DrawSkinned(model, Transform, skeleton.GetSkinTransforms(), null);
        }


        #endregion
    }
}
