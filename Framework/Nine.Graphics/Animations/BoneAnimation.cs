namespace Nine.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Nine.Graphics;


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
        /// The transform is ordered by bone index then ordered by frame number.
        /// </summary>
        [ContentSerializer]
        public Matrix[][] Transforms { get; internal set; }
    }
    #endregion

    #region BoneAnimation
    /// <summary>
    /// Represents the animation of a skeleton that can be controlled by
    /// either the predefined animation clip or by custom controllers.
    /// </summary>
    public class BoneAnimation : Animation, IBoneAnimationController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoneAnimation"/> class.
        /// </summary>
        public BoneAnimation(Skeleton skeleton)
        {
            if (skeleton == null)
                throw new ArgumentNullException("skeleton");

            BlendEnabled = true;
            BlendDuration = TimeSpan.FromSeconds(0.5f);
            Skeleton = skeleton;
            Controllers = new BoneAnimationControllerCollection(skeleton);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoneAnimation"/> class.
        /// </summary>
        public BoneAnimation(Skeleton skeleton, BoneAnimationClip animation)
            : this(skeleton, new BoneAnimationController(animation))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoneAnimation"/> class.
        /// </summary>
        public BoneAnimation(Skeleton skeleton, IBoneAnimationController animationController)
            : this(skeleton)
        {
            if (animationController == null)
                throw new ArgumentNullException("animationController");

            this.Controllers.Add(animationController);
        }
        
        /// <summary>
        /// Gets the skeleton currently animated by this bone animation.
        /// </summary>
        public Skeleton Skeleton { get; private set; }

        /// <summary>
        /// Gets all the controllers affecting this BoneAnimation.
        /// </summary>
        public BoneAnimationControllerCollection Controllers { get; private set; }

        /// <summary>
        /// Gets or sets whether this BoneAnimation should blend with the
        /// previous bone poses (specified by BlendTarget) when started 
        /// playing the animation.
        /// </summary>
        public bool BlendEnabled { get; set; }

        /// <summary>
        /// Gets or sets the duration of blend between this BoneAnimation
        /// and the previous animation specified by BlendTarget.
        /// </summary>
        public TimeSpan BlendDuration { get; set; }

        /// <summary>
        /// Gets or sets the key animation of this LayeredAnimation.
        /// A LayeredAnimation ends either when the last contained 
        /// animation stops or when the specifed KeyAnimation ends.
        /// </summary>
        public ITimelineAnimation KeyController
        {
            get { return keyController; }
            set
            {
                if (State != AnimationState.Stopped)
                    throw new InvalidOperationException(
                        "Cannot modify the collection when the animation is been played.");

                if (value != null && !Controllers.Any(c => c == value))
                    throw new ArgumentException(
                        "The specified controller must be added to this animation.");

                keyController = value;
            }
        }

        private ITimelineAnimation keyController;

        /// <summary>
        /// Gets or sets a value indicating whether all other animations
        /// should adjust the playing speed to sychronize the pace with 
        /// the KeyController if a valid one is specified.
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

        private double blendTimer = 0;
        private Matrix[] blendTarget;

        private static int numControllers = 0;
        private static int numBones = 0;
        private static BoneAnimationItem[] weightedBones = null;

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            blendTimer = 0;

            if (BlendEnabled && Skeleton.HasAnimated)
            {
                ValidateBlendTarget();
                if (blendTarget == null || blendTarget.Length < Skeleton.BoneTransforms.Length)
                    blendTarget = new Matrix[Skeleton.BoneTransforms.Length];
                Skeleton.CopyBoneTransformsTo(blendTarget);
            }
            Skeleton.HasAnimated = true;

            if (Controllers.Count > 0 && weightedBones == null || numControllers < Controllers.Count || 
                                                                  numBones < Skeleton.BoneTransforms.Length)
            {
                numControllers = Math.Max(numControllers, Controllers.Count);
                numBones = Math.Max(numBones, Skeleton.BoneTransforms.Length);

                weightedBones = new BoneAnimationItem[numControllers * numBones];
            }

            SychronizeSpeed();

            foreach (IBoneAnimationController controller in Controllers)
            {
                IAnimation animation = controller as IAnimation;
                if (animation != null)
                    animation.Play();
            }

            // Refresh the pose immediately so the skin transform is correct even if update is never called.
            UpdateBoneTransforms(TimeSpan.Zero);

            base.OnStarted();
        }

        [Conditional("DEBUG")]
        private void ValidateBlendTarget()
        {
            if (!(Skeleton.BoneTransforms != null && Skeleton.BoneTransforms.Length > 0 &&
                  Skeleton.BoneTransforms[Skeleton.SkeletonRoot].M44 != 0))
            {
                throw new InvalidOperationException(Strings.InvalidateSkeleton);
            }
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected override void OnStopped()
        {
            foreach (IBoneAnimationController controller in Controllers)
            {
                IAnimation animation = controller as IAnimation;
                if (animation != null)
                    animation.Stop();
            }

            base.OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected override void OnPaused()
        {
            foreach (IBoneAnimationController controller in Controllers)
            {
                IAnimation animation = controller as IAnimation;
                if (animation != null)
                    animation.Pause();
            }

            base.OnPaused();
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected override void OnResumed()
        {
            foreach (IBoneAnimationController controller in Controllers)
            {
                IAnimation animation = controller as IAnimation;
                if (animation != null)
                    animation.Resume();
            }

            base.OnResumed();
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public override void Update(TimeSpan elapsedTime)
        {
            if (State == AnimationState.Playing && Skeleton != null)
            {
                UpdateControllers(elapsedTime);
                UpdateBoneTransforms(elapsedTime);
            }
        }

        private void UpdateControllers(TimeSpan elapsedTime)
        {
            SychronizeSpeed();

            var currentController = 0;
            for (currentController = 0; currentController < Controllers.Count; currentController++)
            {
                var update = Controllers[currentController].Controller as Nine.IUpdateable;
                if (update != null)
                    update.Update(elapsedTime);
            }

            bool allStopped = true;

            if (KeyController != null)
            {
                allStopped = (KeyController.State == AnimationState.Stopped);
            }
            else
            {
                for (currentController = 0; currentController < Controllers.Count; currentController++)
                {
                    IAnimation animation = Controllers[currentController].Controller as IAnimation;
                    if (animation != null)
                    {
                        if (animation.State != AnimationState.Stopped)
                            allStopped = false;
                    }
                    else
                    {
                        allStopped = false;
                    }
                }
            }

            if (allStopped)
            {
                Stop();
                OnCompleted();
            }
        }

        private void SychronizeSpeed()
        {
            if (isSychronized && keyController != null)
            {
                TimeSpan duration = keyController.Duration;
                for (var currentController = 0; currentController < Controllers.Count; currentController++)
                {
                    var controller = Controllers[currentController].Controller;
                    if (controller == keyController)
                        continue;

                    ITimelineAnimation animation = controller as ITimelineAnimation;
                    if (animation == null)
                        continue;

                    animation.Speed = (float)(
                        (double)animation.Duration.Ticks * keyController.Speed / duration.Ticks);
                }
            }
        }

        /// <summary>
        /// Updates the bone transforms.
        /// </summary>
        private void UpdateBoneTransforms(TimeSpan elapsedTime)
        {
            // Update default blend
            float blendLerp = 0;

            if (BlendEnabled)
            {
                blendTimer += elapsedTime.TotalSeconds;
                blendLerp = (float)(blendTimer / BlendDuration.TotalSeconds);
            }

            // Update controller transforms
            Array.Clear(weightedBones, 0, weightedBones.Length);

            for (int controllerIndex = 0; controllerIndex < Controllers.Count; controllerIndex++)
            {
                for (int bone = 0; bone < Skeleton.BoneTransforms.Length; bone++)
                {
                    int index = controllerIndex * numBones + bone;
                    if (Controllers[controllerIndex].Controller.TryGetBoneTransform(
                                            bone, out weightedBones[index].Transform,
                                                  out weightedBones[index].BlendWeight))
                    {
                        weightedBones[index].BlendWeight *= Controllers[controllerIndex].BlendWeight * (
                            Controllers[controllerIndex].BoneWeights[bone].Enabled ?
                            Controllers[controllerIndex].BoneWeights[bone].BlendWeight : 0);
                    }
                    else
                    {
                        weightedBones[index].BlendWeight = 0;
                    }
                }
            }
            
            // Normalize weights
            for (int bone = 0; bone < Skeleton.BoneTransforms.Length; bone++)
            {
                int firstNonZeroChannel = -1;
                float totalWeight = 0;
                
                for (int controllerIndex = 0; controllerIndex < Controllers.Count; controllerIndex++)
                {
                    int index = controllerIndex * numBones + bone;
                    if (firstNonZeroChannel < 0 && weightedBones[index].BlendWeight > 0)
                        firstNonZeroChannel = controllerIndex;

                    totalWeight += weightedBones[index].BlendWeight;
                }

                if (totalWeight <= 0)
                    continue;

                Matrix transform = weightedBones[firstNonZeroChannel * numBones + bone].Transform;

                for (int controllerIndex = firstNonZeroChannel + 1; controllerIndex < Controllers.Count; controllerIndex++)
                {
                    int index = controllerIndex * numBones + bone;
                    if (weightedBones[index].BlendWeight <= float.Epsilon)
                        continue;

                    // This is not mathmatically correct, but produces an acceptable result.
                    transform = LerpHelper.Slerp(transform,
                                                 weightedBones[index].Transform,
                                                 weightedBones[index].BlendWeight / totalWeight);
                }

                // Perform default blend
                if (BlendEnabled && blendTarget != null && blendLerp <= 1)
                {
                    transform = LerpHelper.Slerp(blendTarget[bone], transform, MathHelper.SmoothStep(0, 1, blendLerp));
                }

                Skeleton.BoneTransforms[bone] = transform;
            }
        }

        /// <summary>
        /// Tries to get the local transform and blend weight of the specified bone.
        /// </summary>
        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            blendWeight = 1;

            if (bone >= 0 && bone < Skeleton.BoneTransforms.Length)
            {
                transform = Skeleton.BoneTransforms[bone];
                return true;
            }
            transform = Matrix.Identity;
            return false;
        }
    }

    struct BoneAnimationItem
    {
        public float BlendWeight;
        public Matrix Transform;
    }

    #region WeightedBoneAnimationController
    /// <summary>
    /// Represents a BoneAnimation that has a weight associated with each bone.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WeightedBoneAnimationController
    {
        internal Skeleton Skeleton;

        /// <summary>
        /// Gets the inner controller used by this <c>WeightedBoneAnimationController</c>.
        /// </summary>
        public IBoneAnimationController Controller { get; private set; }

        /// <summary>
        /// Gets or sets the weight applied to the final bone transform.
        /// This parameter has nothing to do with BlendEnabled and BlendDuration.
        /// </summary>
        public float BlendWeight { get; set; }

        /// <summary>
        /// Gets the collection to manipulated the blend weights of each bone.
        /// </summary>
        public WeightedBoneAnimationControllerBoneCollection BoneWeights { get; private set; }

        /// <summary>
        /// Creates a new instance of WeightedBoneAnimationController.
        /// </summary>
        internal WeightedBoneAnimationController(Skeleton skeleton, IBoneAnimationController controller)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            List<WeightedBoneAnimationControllerBone> bones = new List<WeightedBoneAnimationControllerBone>(skeleton.BoneTransforms.Length);
            for (int i = 0; i < skeleton.BoneTransforms.Length; i++)
                bones.Add(new WeightedBoneAnimationControllerBone());

            this.Skeleton = skeleton;
            this.Controller = controller;
            this.BlendWeight = 1;
            this.BoneWeights = new WeightedBoneAnimationControllerBoneCollection(this, bones);
        }

        /// <summary>
        /// Enables the animation on all bones.
        /// </summary>
        public void EnableAll()
        {
            for (int bone = 0; bone < BoneWeights.Count; bone++)
                BoneWeights[bone].Enabled = true;
        }

        /// <summary>
        /// Disables the animation on all bones.
        /// </summary>
        public void DisableAll()
        {
            for (int bone = 0; bone < BoneWeights.Count; bone++)
                BoneWeights[bone].Enabled = false;
        }

        /// <summary>
        /// Enables the animation on the target and its child bones.
        /// </summary>
        public void Enable(int bone, bool enableChildBones)
        {
            SetEnabled(bone, true, enableChildBones);
        }

        /// <summary>
        /// Enables the animation on the target and its child bones.
        /// </summary>
        public void Enable(string bone, bool enableChildBones)
        {
            SetEnabled(Skeleton.GetBone(bone), true, enableChildBones);
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
            SetEnabled(Skeleton.GetBone(bone), false, disableChildBones);
        }

        private void SetEnabled(int bone, bool enabled, bool recursive)
        {
            BoneWeights[bone].Enabled = enabled;

            if (recursive)
            {
                foreach (int child in Skeleton.GetChildBones(bone))
                {
                    SetEnabled(child, enabled, true);
                }
            }
        }
    }

    /// <summary>
    /// Represents a bone that has a blend weight.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WeightedBoneAnimationControllerBone
    {
        internal WeightedBoneAnimationControllerBone()
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

    /// <summary>
    /// Represents a collection of weighted bone for BoneAnimation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WeightedBoneAnimationControllerBoneCollection : ReadOnlyCollection<WeightedBoneAnimationControllerBone>
    {
        WeightedBoneAnimationController animation;

        internal WeightedBoneAnimationControllerBoneCollection(WeightedBoneAnimationController animation, IList<WeightedBoneAnimationControllerBone> bones)
            : base(bones)
        {
            this.animation = animation;
        }

        public WeightedBoneAnimationControllerBone this[string name]
        {
            get { return this[animation.Skeleton.GetBone(name)]; }
        }
    }
    
    /// <summary>
    /// Represents a collection of controllers for BoneAnimation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BoneAnimationControllerCollection : ICollection<IBoneAnimationController>
    {
        private Skeleton Skeleton;
        private List<WeightedBoneAnimationController> controllers = new List<WeightedBoneAnimationController>();

        internal BoneAnimationControllerCollection(Skeleton skeleton)
        {
            this.Skeleton = skeleton;
        }

        public WeightedBoneAnimationController this[int index]
        {
            get { return controllers[index]; }
        }

        public WeightedBoneAnimationController this[IBoneAnimationController item]
        {
            get
            {
                foreach (WeightedBoneAnimationController controller in controllers)
                    if (controller.Controller == item)
                        return controller;
                return null;
            }
        }

        public void Add(IBoneAnimationController item)
        {
            Add(item, 1.0f);
        }

        public void Add(IBoneAnimationController item, float blendWeight)
        {
            controllers.Add(new WeightedBoneAnimationController(Skeleton, item) { BlendWeight = blendWeight });
        }

        public void Clear()
        {
            controllers.Clear();
        }

        public bool Contains(IBoneAnimationController item)
        {
            foreach (WeightedBoneAnimationController controller in controllers)
                if (controller.Controller == item)
                    return true;
            return false;
        }

        public void CopyTo(IBoneAnimationController[] array, int arrayIndex)
        {
            foreach (WeightedBoneAnimationController controller in controllers)
                array[arrayIndex++] = controller.Controller;
        }

        public int Count
        {
            get { return controllers.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IBoneAnimationController item)
        {
            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].Controller == item)
                {
                    controllers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<IBoneAnimationController> GetEnumerator()
        {
            return controllers.Select(controller => controller.Controller).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    #endregion
    #endregion
}
