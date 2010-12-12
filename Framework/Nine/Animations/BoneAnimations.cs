#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Animations
{
    #region BoneAnimationClip
    /// <summary>
    /// Defines a bone animation clip.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BoneAnimationClip
    {
        /// <summary>
        /// Gets animation frame rate.
        /// </summary>
        [ContentSerializer]
        public int FramesPerSecond { get; internal set; }

        /// <summary>
        /// Gets total number of frames.
        /// </summary>
        [ContentSerializer]
        public int TotalFrames { get; internal set; }

        /// <summary>
        /// Gets the preferred ending style.
        /// </summary>
        [ContentSerializer]
        public KeyframeEnding PreferredEnding { get; internal set; }

        /// <summary>
        /// Gets all the channels in this animation clip.
        /// </summary>
        [ContentSerializer]
        public Matrix[][] Transforms { get; internal set; }
    }
    #endregion

    #region BoneAnimation
    /// <summary>
    /// Represents skeleton animation for models.
    /// </summary>
    public class BoneAnimation : KeyframeAnimation
    {
        /// <summary>
        /// Gets or sets whether this BoneAnimation should automatically
        /// perform matrix interpolation when the playing speed is less then 1.
        /// </summary>
        public bool InterpolationEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether this BoneAnimation should blend with the
        /// previous bone poses when started playing the animation.
        /// </summary>
        public bool BlendEnabled { get; set; }

        /// <summary>
        /// Gets or sets blend time.
        /// </summary>
        public TimeSpan BlendDuration { get; set; }

        /// <summary>
        /// Gets the animation clip used by this bone animation.
        /// </summary>
        public BoneAnimationClip Clip { get; private set; }

        /// <summary>
        /// Gets the model affected by this bone animation.
        /// </summary>
        public Model Model { get; private set; }

        private double blendTimer = 0;
        private Matrix[] blendTarget;
        private bool shouldLerp;

        /// <summary>
        /// Creates a new instance of BoneAnimation.
        /// </summary>
        public BoneAnimation(Model model, BoneAnimationClip clip)
        {
            if (clip == null || model == null)
                throw new ArgumentNullException();

            this.Clip = clip;
            this.Model = model;
            this.BlendEnabled = true;
            this.Ending = clip.PreferredEnding;
            this.BlendDuration = TimeSpan.FromSeconds(0.5);
            this.InterpolationEnabled = true;
            this.FramesPerSecond = clip.FramesPerSecond;
        }

        protected override int GetTotalFrames()
        {
            return Clip.TotalFrames;
        }

        protected override void OnStarted()
        {
            if (BlendEnabled)
            {
                if (blendTarget == null)
                    blendTarget = new Matrix[Model.Bones.Count];

                blendTimer = 0;
                Model.CopyBoneTransformsTo(blendTarget);
            }

            base.OnStarted();
        }

        public override void Update(GameTime gameTime)
        {
            if (State == Animations.AnimationState.Playing)
            {
                blendTimer += gameTime.ElapsedGameTime.TotalSeconds;
                shouldLerp = (gameTime.ElapsedGameTime.TotalSeconds * Speed) < (1.0 / FramesPerSecond);
            
                if (BlendEnabled && blendTimer < BlendDuration.TotalSeconds)
                {
                    blendLerp = (float)(blendTimer / BlendDuration.TotalSeconds);
                }
            }

            base.Update(gameTime);
        }

        private int startFrame;
        private int endFrame;
        private float percentage;
        private float blendLerp = -1;

        protected override void OnSeek(int startFrame, int endFrame, float percentage)
        {
            this.startFrame = startFrame;
            this.endFrame = endFrame;
            this.percentage = percentage;

            Apply((bone, transform) => { Model.Bones[bone].Transform = transform; });
        }

        /// <summary>
        /// Applies the current local bone transforms of this BoneAnimation.
        /// </summary>
        public void Apply(Action<int, Matrix> setBoneLocalTransform)
        {
            for (int bone = 0; bone < Clip.Transforms.Length; bone++)
            {
                Matrix? transform = OnApply(bone);

                if (transform.HasValue)
                {
                    setBoneLocalTransform(bone, transform.Value);
                }
            }
        }

        protected virtual Matrix? OnApply(int bone)
        {
            if (Clip.Transforms[bone] == null)
                return null;

            Matrix transform;

            if (InterpolationEnabled && shouldLerp)
            {
                transform = LerpHelper.Slerp(
                             Clip.Transforms[bone][startFrame],
                             Clip.Transforms[bone][endFrame], percentage);
            }
            else
            {
                transform = Clip.Transforms[bone][startFrame];
            }

            if (blendLerp >= 0 && blendLerp < 1)
            {
                transform = LerpHelper.Slerp(
                            blendTarget[bone], transform, MathHelper.SmoothStep(0, 1, blendLerp));
            }

            return transform;
        }
    }
    #endregion

    #region WeightedBoneAnimation
    /// <summary>
    /// Represents a BoneAnimation that has a weight associated with each bone.
    /// </summary>
    public class WeightedBoneAnimation : BoneAnimation, IBoneAnimationController
    {
        /// <summary>
        /// Gets or sets the weight applied to the final bone transform.
        /// This parameter has nothing to do with BlendEnabled and BlendDuration.
        /// </summary>
        public float BlendWeight { get; set; }

        /// <summary>
        /// Gets the collection to manipulated the blend weights of each bone.
        /// </summary>
        public WeightedBoneAnimationBoneCollection Bones { get; private set; }

        /// <summary>
        /// Creates a new instance of WeightedBoneAnimation.
        /// </summary>
        public WeightedBoneAnimation(Model model, BoneAnimationClip clip) : base(model, clip)
        {
            List<WeightedBoneAnimationBone> bones = new List<WeightedBoneAnimationBone>(model.Bones.Count);
            for (int i = 0; i < model.Bones.Count; i++)
                bones.Add(new WeightedBoneAnimationBone());

            this.BlendWeight = 1;
            this.Bones = new WeightedBoneAnimationBoneCollection(this, bones);
        }

        /// <summary>
        /// Enables the animation on the target and its child bones.
        /// </summary>
        public void Enabled(int bone, bool enableChildBones)
        {
            SetEnabled(bone, true, enableChildBones);
        }

        /// <summary>
        /// Enables the animation on the target and its child bones.
        /// </summary>
        public void Enabled(string bone, bool enableChildBones)
        {
            SetEnabled(Model.Bones[bone].Index, true, enableChildBones);
        }

        /// <summary>
        /// Disables the animation on the target and its child bones.
        /// </summary>
        public void Disable(int bone, bool disableChildBones)
        {
            SetEnabled(bone, false, disableChildBones);
        }

        /// <summary>
        /// Disables the animation on the target and its child bones.
        /// </summary>
        public void Disable(string bone, bool disableChildBones)
        {
            SetEnabled(Model.Bones[bone].Index, false, disableChildBones);
        }

        private void SetEnabled(int bone, bool enabled, bool recursive)
        {
            Bones[bone].Enabled = enabled;

            if (recursive)
            {
                foreach (ModelBone child in Model.Bones[bone].Children)
                {
                    SetEnabled(child.Index, enabled, true);
                }
            }
        }

        public void Apply(Action<int, Matrix, float> setWeightedBoneLocalTransform)
        {
            Apply((bone, transform) => 
            {
                setWeightedBoneLocalTransform(bone, transform, Bones[bone].BlendWeight * BlendWeight);
            });
        }

        protected override Matrix? OnApply(int bone)
        {
            if (!Bones[bone].Enabled || Bones[bone].BlendWeight <= 0)
                return null;

            return base.OnApply(bone);
        }
    }

    #region WeightedBoneAnimationBone
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WeightedBoneAnimationBone
    {
        internal WeightedBoneAnimationBone()
        {
            BlendWeight = 1;
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets the blend weight of this bone.
        /// </summary>
        public float BlendWeight { get; set; }

        /// <summary>
        /// Gets or sets wether this bone is enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
    #endregion

    #region WeightedBoneAnimationBoneCollection
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WeightedBoneAnimationBoneCollection : ReadOnlyCollection<WeightedBoneAnimationBone>
    {
        WeightedBoneAnimation animation;

        internal WeightedBoneAnimationBoneCollection(WeightedBoneAnimation animation, IList<WeightedBoneAnimationBone> bones)
            : base(bones)
        {
            this.animation = animation;
        }

        public WeightedBoneAnimationBone this[string name]
        {
            get { return this[animation.Model.Bones[name].Index]; }
        }
    }
    #endregion
    #endregion

    #region IBoneAnimationController
    public interface IBoneAnimationController
    {
        void Apply(Action<int, Matrix, float> setWeightedBoneLocalTransform);
    }
    #endregion

    #region LayeredBoneAnimation
    public class LayeredBoneAnimation : LayeredAnimationBase<WeightedBoneAnimation>
    {
        public LayeredBoneAnimation() { }
        public LayeredBoneAnimation(IEnumerable<WeightedBoneAnimation> animations) : base(animations) { }
        public LayeredBoneAnimation(params WeightedBoneAnimation[] animations) : base(animations) { }
        
        public Model Model { get; private set; }

        public IList<IBoneAnimationController> AnimationControllers
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all other animations
        /// should adjust the playing speed to sychronize the pace with 
        /// the KeyAnimation if a valid one is specified.
        /// </summary>
        public bool IsSychronized
        {
            get { return isSychronized; }
            set
            {
                if (State != AnimationState.Stopped)
                    throw new InvalidOperationException(
                        "Cannot modify the collection when the animation is been played.");

                isSychronized = value;
            }
        }

        private bool isSychronized;

        private static LayeredBoneAnimationItem[,] bones = null;
        
        protected override void OnStarted()
        {
            if (Animations.Count != 2)
            {
                throw new NotImplementedException(
                    "LayeredBoneAnimation only supports 2 animations by now.");
            }

            Model = null;

            foreach (BoneAnimation animation in Animations)
            {
                if (Model == null)
                    Model = animation.Model;
                else if (Model != animation.Model)
                    throw new InvalidOperationException(
                        "All BoneAnimations must be applied to the same model when using LayeredBoneAnimation");
            }

            if (Model != null && (bones == null || bones.Length < Animations.Count))
            {
                bones = new LayeredBoneAnimationItem[Animations.Count, Model.Bones.Count];
            }

            if (isSychronized && KeyAnimation != null)
            {
                TimeSpan duration = KeyAnimation.Duration;
                foreach (WeightedBoneAnimation animation in Animations)
                {
                    if (animation == KeyAnimation)
                        continue;

                    animation.Speed = (float)(
                        (double)animation.Duration.Ticks * KeyAnimation.Speed / duration.Ticks);
                }
            }

            base.OnStarted();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (State == AnimationState.Playing && Model != null)
            {
                Array.Clear(bones, 0, bones.Length);

                for (int animationIndex = 0; animationIndex < Animations.Count; animationIndex++)
                {
                    Animations[animationIndex].Apply((bone, transform, weight) =>
                    {
                        bones[animationIndex, bone].BlendWeight = weight;
                        bones[animationIndex, bone].Transform = transform;
                    });
                }

                // Normalize weights
                for (int bone = 0; bone < Model.Bones.Count; bone++)
                {
                    Matrix transform = new Matrix();
                    float totalWeight = 0;

                    for (int animationIndex = 0; animationIndex < Animations.Count; animationIndex++)
                    {
                        totalWeight += bones[animationIndex, bone].BlendWeight;
                    }

                    if (totalWeight <= 0)
                        continue;

                    for (int animationIndex = 0; animationIndex < Animations.Count; animationIndex++)
                    {
                        // TODO: Implement blending from multiple sources
                        //
                        //transform += bones[animationIndex, bone].Transform * (bones[animationIndex, bone].BlendWeight / totalWeight);
                    }

                    transform = LerpHelper.Slerp(bones[0, bone].Transform, bones[1, bone].Transform, bones[1, bone].BlendWeight / totalWeight);

                    Model.Bones[bone].Transform = transform;
                }
            }
        }
    }

    internal struct LayeredBoneAnimationItem
    {
        public float BlendWeight;
        public Matrix Transform;
    }
    #endregion
}
