namespace Nine.Animations
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    
    /// <summary>
    /// Controls the bone transforms of a model based on predefined skeleton animation tracks.
    /// </summary>
    [Nine.Serialization.NotBinarySerializable]
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
        /// Initializes a new instance of the <see cref="BoneAnimationController"/> class.
        /// </summary>
        public BoneAnimationController(BoneAnimationClip animationClip)
        {
            if (animationClip == null)
                throw new ArgumentNullException("animationClip");

            this.AnimationClip = animationClip;
            this.Ending = animationClip.PreferredEnding;
            this.InterpolationEnabled = false;
            this.FramesPerSecond = animationClip.FramesPerSecond;
            this.TotalFrames = animationClip.TotalFrames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoneAnimationController"/> class.
        /// </summary>
        public BoneAnimationController(BoneAnimationClip animationClip, int beginFrame, int count)
            : this(animationClip)
        {
            base.BeginFrame = beginFrame;
            base.EndFrame = beginFrame + count;
        }

        private int startFrame;
        private int endFrame;
        private float percentage;
        private bool shouldLerp;

        /// <summary>
        /// Update the animation by a specified amount of elapsed time.
        /// Handle playing either forwards or backwards.
        /// Determines whether animation should terminate or continue.
        /// Signals related events.
        /// </summary>
        public override void Update(float elapsedTime)
        {
            if (State == Animations.AnimationState.Playing)
            {
                shouldLerp = (elapsedTime * Speed) < (1.0 / FramesPerSecond);
            }

            base.Update(elapsedTime);
        }

        /// <summary>
        /// Moves the animation at the position between start frame and end frame
        /// specified by percentage.
        /// </summary>
        protected override void OnSeek(int startFrame, int endFrame, float percentage)
        {
            this.startFrame = startFrame;
            this.endFrame = endFrame;
            this.percentage = percentage;
        }

        /// <summary>
        /// Tries to get the local transform and blend weight of the specified bone.
        /// </summary>
        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            blendWeight = 1;

            if (bone >= AnimationClip.Transforms.Length || AnimationClip.Transforms[bone] == null)
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

    /// <summary>
    /// Provides a snapshot of the original bone transforms of a model.
    /// </summary>
    public class BoneSnapshotController : IBoneAnimationController
    {
        Matrix[] transforms;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoneSnapshotController"/> class.
        /// </summary>
        public BoneSnapshotController(Microsoft.Xna.Framework.Graphics.Model model)
        {
            transforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(transforms);
        }

        /// <summary>
        /// Tries to get the local transform and blend weight of the specified bone.
        /// </summary>
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
}