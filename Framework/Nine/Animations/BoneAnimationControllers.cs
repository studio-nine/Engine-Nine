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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Animations
{
    #region IBoneAnimationController
    /// <summary>
    /// Represents a controller that manipulates the bone transforms of a model.
    /// </summary>
    public interface IBoneAnimationController
    {
        /// <summary>
        /// Tries to get the local transform and blend weight of the specified bone.
        /// </summary>
        bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight);
    }
    #endregion

    #region BoneAnimationController
    /// <summary>
    /// Controls the bone transforms of a model based on predefined skeleton animation tracks.
    /// </summary>
    public class BoneAnimationController : KeyframeAnimation, IBoneAnimationController
    {
        /// <summary>
        /// Gets or sets whether this BoneAnimation should automatically
        /// perform matrix interpolation when the playing speed is less then 
        /// current frame rate.
        /// </summary>
        public bool InterpolationEnabled { get; set; }

        /// <summary>
        /// Gets the animation clip used by this bone animation.
        /// </summary>
        public BoneAnimationClip AnimationClip { get; private set; }

        /// <summary>
        /// Creates a new instance of BoneAnimation.
        /// </summary>
        public BoneAnimationController(BoneAnimationClip animationClip)
        {
            if (animationClip == null)
                throw new ArgumentNullException("animationClip");

            this.AnimationClip = animationClip;
            this.Ending = animationClip.PreferredEnding;
            this.InterpolationEnabled = true;
            this.FramesPerSecond = animationClip.FramesPerSecond;
        }

        protected override int GetTotalFrames()
        {
            return AnimationClip.TotalFrames;
        }

        private int startFrame;
        private int endFrame;
        private float percentage;
        private bool shouldLerp;

        public override void Update(GameTime gameTime)
        {
            if (State == Animations.AnimationState.Playing)
            {
                shouldLerp = (gameTime.ElapsedGameTime.TotalSeconds * Speed) < (1.0 / FramesPerSecond);
            }

            base.Update(gameTime);
        }

        protected override void OnSeek(int startFrame, int endFrame, float percentage)
        {
            this.startFrame = startFrame;
            this.endFrame = endFrame;
            this.percentage = percentage;
        }

        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            blendWeight = 1;

            if (AnimationClip.Transforms[bone] == null)
            {
                transform = Matrix.Identity;
                return false;
            }

            if (InterpolationEnabled && shouldLerp)
            {
                transform = LerpHelper.Slerp(
                             AnimationClip.Transforms[bone][startFrame],
                             AnimationClip.Transforms[bone][endFrame], percentage);
            }
            else
            {
                transform = AnimationClip.Transforms[bone][startFrame];
            }

            return true;
        }
    }
    #endregion

    #region BoneSnapshotController
    /// <summary>
    /// Provides a snapshot of the original bone transforms of a model.
    /// </summary>
    public class BoneSnapshotController : IBoneAnimationController
    {
        Matrix[] transforms;

        public BoneSnapshotController(Model model)
        {
            transforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(transforms);
        }

        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            blendWeight = 1;

            if (bone >= 0 && bone < transforms.Length)
            {
                transform = transforms[bone];
                return true;
            }

            transform = Matrix.Identity;
            return false;
        }
    }
    #endregion

    #region LookAtController
    /// <summary>
    /// Controls the target bone to make it always look at the specified target.
    /// </summary>
    public class LookAtController : IUpdateObject, IBoneAnimationController
    {
        /// <summary>
        /// Gets or sets the index of the controlled bone.
        /// </summary>
        public int Bone { get; set; }

        /// <summary>
        /// Gets or sets the target to look at.
        /// </summary>
        public Vector3? Target { get; set; }

        /// <summary>
        /// Gets or sets the up axis.
        /// </summary>
        public Vector3 Up { get; set; }

        /// <summary>
        /// Gets or sets the forward axis.
        /// </summary>
        public Vector3 Forward { get; set; }

        /// <summary>
        /// Gets or sets the base world transform of the parent BoneAnimation.
        /// </summary>
        public Matrix Transform { get; set; }

        /// <summary>
        /// Gets or sets the range of horizontal rotation of the controlled bone.
        /// </summary>
        public Range<float> HorizontalRotation { get; set; }

        /// <summary>
        /// Gets or sets the range of vertical rotation of the controlled bone.
        /// </summary>
        public Range<float> VerticalRotation { get; set; }

        /// <summary>
        /// Gets or sets the max rotation speed.
        /// </summary>
        public float RotationSpeed { get; set; }

        /// <summary>
        /// Gets the parent animation that this controller is affecting.
        /// </summary>
        public BoneAnimation ParentAnimation { get; private set; }

        private Vector3 currentLookAt;
        private Matrix desiredTransform;
        private float desiredBlendWeight = 1;

        /// <summary>
        /// Creates a new LookAtController.
        /// </summary>
        public LookAtController(BoneAnimation parentAnimation, Matrix transform, int bone)
        {
            if (parentAnimation == null)
                throw new ArgumentNullException("parentAnimation");

            Transform = transform;
            ParentAnimation = parentAnimation;
            RotationSpeed = MathHelper.Pi;
            HorizontalRotation = VerticalRotation = new Range<float>(-MathHelper.Pi, MathHelper.Pi);
            Up = Vector3.Up;
            Forward = Vector3.Forward;
            Bone = bone;

            currentLookAt = Forward;
            desiredTransform = Matrix.Identity;
        }

        public void Update(GameTime gameTime)
        {
            bool hasTarget = Target.HasValue;

            if (hasTarget)
            {
                // Transform target to the local space of parent bone
                Matrix parentWorldTransform = ParentAnimation.GetAbsoluteBoneTransform(ParentAnimation.GetParentBone(Bone)) * Transform;
                Vector3 parentLocal = Vector3.Transform(Target.Value, Matrix.Invert(parentWorldTransform));

                // Compute the rotation transform to the target position
                Matrix boneTransform = ParentAnimation.GetBoneTransform(Bone);
                Vector3 desiredLookAt = Vector3.Normalize(parentLocal - boneTransform.Translation);

                if (desiredLookAt == Vector3.Zero)
                {
                    hasTarget = false;
                    desiredLookAt = Forward;
                }

                // Clamp rotation
                float horizontalAngle = (float)Math.Acos(Vector3.Dot(desiredLookAt, Forward));
                float verticalAngle = (float)Math.Acos(Vector3.Dot(desiredLookAt, Up));

                if (horizontalAngle > HorizontalRotation.Max || horizontalAngle < HorizontalRotation.Min ||
                    verticalAngle > VerticalRotation.Max || verticalAngle < VerticalRotation.Min)
                {
                    hasTarget = false;
                    desiredLookAt = Forward;
                }

                float maxRotation = (float)(RotationSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                float currentRotation = (float)Math.Acos(Vector3.Dot(desiredLookAt, currentLookAt));
                if (currentRotation > maxRotation)
                {
                    // Use cheep lerp
                    desiredLookAt = Vector3.Lerp(currentLookAt, desiredLookAt, maxRotation / currentRotation);
                    desiredLookAt.Normalize();
                    currentLookAt = desiredLookAt;
                }

                desiredTransform = Matrix.Invert(Matrix.CreateWorld(Vector3.Zero, Forward, Up)) *
                                   Matrix.CreateWorld(boneTransform.Translation, desiredLookAt, Up);
            }

            if (hasTarget)
                desiredBlendWeight += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
                desiredBlendWeight -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (desiredBlendWeight < 0)
                desiredBlendWeight = 0;
            else if (desiredBlendWeight > 1)
                desiredBlendWeight = 1;
        }

        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            transform = desiredTransform;
            blendWeight = desiredBlendWeight;

            return bone == Bone;
        }
    }
    #endregion
}