namespace Nine.Animations
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.GamerServices;
    using Nine.Graphics;

    #region AvatarSkeleton
    /// <summary>
    /// Defines the skeleton for avatar.
    /// </summary>
    public class AvatarSkeleton : Skeleton
    {
        Matrix[] boneTransforms;
        AvatarRendererState previousState = AvatarRendererState.Loading;

        public AvatarRenderer Renderer { get; private set; }
        public AvatarRendererState State { get { return Renderer.State; } }

        public override Matrix[] BoneTransforms
        {
            get
            {
                ValidateAvatarState();
                return boneTransforms;
            }
        }

        public override ReadOnlyCollection<int> ParentBones { get { return Renderer.ParentBones; } }

        private void ValidateAvatarState()
        {
            if (previousState != Renderer.State)
            {
                previousState = Renderer.State;
                if (previousState == AvatarRendererState.Ready)
                {
                    for (int i = 0; i < AvatarRenderer.BoneCount; i++)
                        boneTransforms[i] = Renderer.BindPose[i];
                }
            }
        }

        public AvatarSkeleton(AvatarRenderer renderer)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            this.Renderer = renderer;
            this.boneTransforms = new Matrix[AvatarRenderer.BoneCount];
            this.BoneNames = new ReadOnlyCollection<string>(
                Enumerable.Range(0, AvatarRenderer.BoneCount)
                          .Select(i => ((AvatarBone)i).ToString()).ToArray());
        }

        public int GetBone(AvatarBone avatarBone)
        {
            return (int)avatarBone;
        }

        public Matrix GetAbsoluteBoneTransform(AvatarBone avatarBone)
        {
            return this.GetAbsoluteBoneTransform((int)avatarBone);
        }

        public Matrix GetBoneTransform(AvatarBone avatarBone)
        {
            return this.GetBoneTransform((int)avatarBone);
        }
    }
    #endregion

    #region AvatarAnimationController
    /// <summary>
    /// Defines a basic avatar animation controller from presets.
    /// </summary>
    public class AvatarAnimationController : Animation, IBoneAnimationController, ITimelineAnimation
    {
        public bool Loop { get; set; }
        public float Speed { get { return 1; } set { } }
        public AvatarRenderer Renderer { get; private set; }

        public TimeSpan Duration
        {
            get { return avatarAnimation != null ? avatarAnimation.Length : boneAnimationController.Duration; }
        }

        public TimeSpan Position
        {
            get { return avatarAnimation != null ? avatarAnimation.CurrentPosition : boneAnimationController.Position; } 
        }
        
        AvatarAnimation avatarAnimation;
        BoneAnimationController boneAnimationController;

        public AvatarAnimationController(AvatarRenderer renderer, AvatarAnimationPreset preset)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            Renderer = renderer;
            Loop = true;
            avatarAnimation = new AvatarAnimation(preset);
        }

        public AvatarAnimationController(AvatarRenderer renderer, BoneAnimationClip clip)
        {
            if (clip.Transforms.Length != AvatarRenderer.BoneCount)
                throw new ArgumentException(Strings.InvalidAvatarAnimationClip);

            Renderer = renderer;
            Loop = true;
            boneAnimationController = new BoneAnimationController(clip);
            boneAnimationController.Completed += (sender, e) => { OnCompleted(); Stop(); };
            boneAnimationController.Play();
        }

        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            blendWeight = 1;
            if (bone >= 0 && bone < AvatarRenderer.BoneCount)
            {
                if (avatarAnimation != null)
                    transform = avatarAnimation.BoneTransforms[bone];
                else if (!boneAnimationController.TryGetBoneTransform(bone, out transform, out blendWeight))
                    return false;
                transform *= Renderer.BindPose[bone];
                return true;
            }
            transform = Matrix.Identity;
            return false;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (avatarAnimation != null)
            {
                avatarAnimation.Update(elapsedTime, Loop);
                if (!Loop && avatarAnimation.CurrentPosition >= avatarAnimation.Length)
                {
                    Stop();
                    OnCompleted();
                }
            }
            else
            {
                boneAnimationController.Update(elapsedTime);
            }
        }
    }
    #endregion

    #region AvatarBoneAnimation
    /// <summary>
    /// Provides methods and properties for animating an avatar using custom animations.

    /// </summary>
    public class AvatarBoneAnimation : BoneAnimation, IAvatarAnimation
    {
        /// <summary>
        /// Gets the skeleton of the avatar.
        /// </summary>
        public new AvatarSkeleton Skeleton { get; private set; }

        /// <summary>
        /// Gets the current position of the bones at the time specified by CurrentPosition.
        /// </summary>
        ReadOnlyCollection<Matrix> IAvatarAnimation.BoneTransforms
        {
            get { return readOnlyAnimationBoneTransforms; }
        }

        /// <summary>
        /// The current temporal position in the animation.
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set
            {
                currentPosition = value;
                Update(TimeSpan.Zero);
            }
        }

        /// <summary>
        /// The current temporal position in the animation.
        /// </summary>
        private TimeSpan currentPosition = TimeSpan.Zero;

        public AvatarExpression Expression { get; protected set; }

        private AnimationState cachedAnimationState = AnimationState.Stopped;
        private ReadOnlyCollection<Matrix> readOnlyAnimationBoneTransforms;
        private Matrix[] animationBoneTransforms;

        public TimeSpan Length
        {
            get { return TimeSpan.MaxValue; }
        }

        public AvatarBoneAnimation(AvatarSkeleton skeleton) : base(skeleton)
        {
            Skeleton = skeleton;
            animationBoneTransforms = new Matrix[AvatarRenderer.BoneCount];
            readOnlyAnimationBoneTransforms = new ReadOnlyCollection<Matrix>(animationBoneTransforms);
        }

        public AvatarBoneAnimation(AvatarSkeleton skeleton, AvatarAnimationPreset preset)
            : base(skeleton, new AvatarAnimationController(skeleton.Renderer, preset))
        {
            Skeleton = skeleton;
            animationBoneTransforms = new Matrix[AvatarRenderer.BoneCount];
            readOnlyAnimationBoneTransforms = new ReadOnlyCollection<Matrix>(animationBoneTransforms);
        }

        public AvatarBoneAnimation(AvatarSkeleton skeleton, BoneAnimationClip animationClip)
            : base(skeleton, new AvatarAnimationController(skeleton.Renderer, animationClip))
        {
            Skeleton = skeleton;
            animationBoneTransforms = new Matrix[AvatarRenderer.BoneCount];
            readOnlyAnimationBoneTransforms = new ReadOnlyCollection<Matrix>(animationBoneTransforms);
        }

        protected override void OnStarted()
        {
            if (Skeleton.State != AvatarRendererState.Ready)
            {
                cachedAnimationState = AnimationState.Playing;
                return;
            }
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            if (Skeleton.State != AvatarRendererState.Ready)
            {
                cachedAnimationState = AnimationState.Stopped;
                return;
            }
            base.OnStopped();
        }

        protected override void OnPaused()
        {
            if (Skeleton.State != AvatarRendererState.Ready)
            {
                cachedAnimationState = AnimationState.Paused;
                return;
            }
            base.OnPaused();
        }

        protected override void OnResumed()
        {
            if (Skeleton.State != AvatarRendererState.Ready)
            {
                cachedAnimationState = AnimationState.Playing;
                return;
            }
            base.OnResumed();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Skeleton.State == AvatarRendererState.Ready)
            {
                if (cachedAnimationState == AnimationState.Playing)
                {
                    cachedAnimationState = AnimationState.Stopped;
                    Play();
                }
                else if (cachedAnimationState == AnimationState.Paused)
                {
                    cachedAnimationState = AnimationState.Stopped;
                    Play();
                    Pause();
                }

                base.Update(elapsedTime);

                for (int i = 0; i < AvatarRenderer.BoneCount; i++)
                {
                    animationBoneTransforms[i] = Skeleton.BoneTransforms[i] * 
                                   Matrix.Invert(Skeleton.Renderer.BindPose[i]);
                }
            }
        }

        void IAvatarAnimation.Update(TimeSpan elapsedAnimationTime, bool loop)
        {

        }
    }
    #endregion
}