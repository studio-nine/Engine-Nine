namespace Nine.Animations
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;


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
        public override void Update(TimeSpan elapsedTime)
        {
            if (State == Animations.AnimationState.Playing)
            {
                shouldLerp = (elapsedTime.TotalSeconds * Speed) < (1.0 / FramesPerSecond);
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
    #endregion

    #region BoneSnapshotController
    /// <summary>
    /// Provides a snapshot of the original bone transforms of a model.
    /// </summary>
    public class BoneSnapshotController : IBoneAnimationController
    {
        Matrix[] transforms;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoneSnapshotController"/> class.
        /// </summary>
        public BoneSnapshotController(Model model)
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
    #endregion

    #region LookAtController
    /// <summary>
    /// Controls the target bone to make it always look at the specified target.
    /// </summary>
    public class LookAtController : Nine.IUpdateable, IBoneAnimationController
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
        public Vector3 Forward 
        {
            get { return forward; }
            set { forward = currentLookAt = value; }
        }

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
        public Skeleton Skeleton { get; private set; }

        public event EventHandler<EventArgs> TargetSpotted;
        public event EventHandler<EventArgs> TargetLost;

        private Vector3 forward;
        private Vector3 currentLookAt;
        private Matrix desiredTransform;
        private float desiredBlendWeight;
        private bool hasTargetLastFrame;

        /// <summary>
        /// Creates a new LookAtController.
        /// </summary>
        public LookAtController(Skeleton skeleton, Matrix transform, int bone)
        {
            if (skeleton == null)
                throw new ArgumentNullException("skeleton");

            Transform = transform;
            Skeleton = skeleton;
            RotationSpeed = MathHelper.Pi;
            HorizontalRotation = VerticalRotation = new Range<float>(-MathHelper.Pi, MathHelper.Pi);
            Up = Vector3.Up;
            Forward = Vector3.Forward;
            Bone = bone;

            currentLookAt = Forward;
            desiredTransform = Matrix.Identity;
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            bool hasTarget = Target.HasValue;

            if (hasTarget)
            {
                // Transform target to the local space of parent bone
                Matrix parentWorldTransform = Transform;
                
                int parentBone = Skeleton.GetParentBone(Bone);
                if (parentBone >= 0)
                    parentWorldTransform = Skeleton.GetAbsoluteBoneTransform(parentBone) * Transform;

                Vector3 parentLocal = Vector3.Transform(Target.Value, Matrix.Invert(parentWorldTransform));

                // Compute the rotation transform to the target position
                Matrix boneTransform = Skeleton.GetBoneTransform(Bone);
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

                float maxRotation = (float)(RotationSpeed * elapsedTime.TotalSeconds);
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
                desiredBlendWeight += (float)elapsedTime.TotalSeconds;
            else
                desiredBlendWeight -= (float)elapsedTime.TotalSeconds;

            if (desiredBlendWeight < 0)
                desiredBlendWeight = 0;
            else if (desiredBlendWeight > 1)
                desiredBlendWeight = 1;

            if (hasTarget && !hasTargetLastFrame && TargetSpotted != null)
            {
                TargetSpotted(this, EventArgs.Empty);
            }
            else if (!hasTarget && hasTargetLastFrame && TargetLost != null)
            {
                TargetLost(this, EventArgs.Empty);
            }
            hasTargetLastFrame = hasTarget;
        }

        /// <summary>
        /// Tries to get the local transform and blend weight of the specified bone.
        /// </summary>
        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            transform = desiredTransform;
            blendWeight = desiredBlendWeight;

            return bone == Bone;
        }
    }
    #endregion
}