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
        /// </summary>
        [ContentSerializer]
        public Matrix[][] Transforms { get; internal set; }
    }
    #endregion

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
    /// Represents skeleton animation for models.
    /// </summary>
    public class BoneAnimationController : KeyframeAnimation, IBoneAnimationController
    {
        /// <summary>
        /// Gets or sets whether this BoneAnimation should automatically
        /// perform matrix interpolation when the playing speed is less then 1.
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

    #region BoneAnimation
    public class BoneAnimation : Animation, IBoneAnimationController
    {
        public BoneAnimation(Model model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Model = model;
            this.DefaultBlendEnabled = true;
            this.DefaultBlendDuration = TimeSpan.FromSeconds(0.5);
            this.Controllers = new BoneAnimationControllerCollection(model);
        }

        public BoneAnimation(Model model, params IBoneAnimationController[] controllers)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Model = model;
            this.DefaultBlendEnabled = true;
            this.DefaultBlendDuration = TimeSpan.FromSeconds(0.5);
            this.Controllers = new BoneAnimationControllerCollection(model);

            foreach (IBoneAnimationController controller in controllers)
            {
                Controllers.Add(controller);
            }
        }

        public BoneAnimation(Model model, params BoneAnimationClip[] animations)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Model = model;
            this.DefaultBlendEnabled = true;
            this.DefaultBlendDuration = TimeSpan.FromSeconds(0.5);
            this.Controllers = new BoneAnimationControllerCollection(model);

            foreach (BoneAnimationClip controller in animations)
            {
                Controllers.Add(new BoneAnimationController(controller));
            }
        }
        
        /// <summary>
        /// Gets the model affected by this bone animation.
        /// </summary>
        public Model Model { get; private set; }

        /// <summary>
        /// Gets all the controllers affecting this BoneAnimation.
        /// </summary>
        public BoneAnimationControllerCollection Controllers { get; private set; }

        /// <summary>
        /// Gets or sets whether this BoneAnimation should blend with the
        /// previous bone poses when started playing the animation.
        /// </summary>
        public bool DefaultBlendEnabled { get; set; }

        /// <summary>
        /// Gets or sets blend time.
        /// </summary>
        public TimeSpan DefaultBlendDuration { get; set; }

        /// <summary>
        /// Gets or sets the blend target of default blend.
        /// </summary>
        public IBoneAnimationController DefaultBlendTarget { get; set; }

        /// <summary>
        /// Gets or sets the key animation of this LayeredAnimation.
        /// A LayeredAnimation ends either when the last contained 
        /// animation stops or when the specifed KeyAnimation ends.
        /// </summary>
        public BoneAnimationController KeyController
        {
            get { return keyController; }
            set
            {
                if (State != AnimationState.Stopped)
                    throw new InvalidOperationException(
                        "Cannot modify the collection when the animation is been played.");

                if (value != null && !Controllers.Contains(value))
                    throw new ArgumentException(
                        "The specified animation must be added to this animation.");

                keyController = value;
            }
        }

        private BoneAnimationController keyController;

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

        /// <summary>
        /// Occurs when this animation has completely finished playing.
        /// </summary>
        public override event EventHandler Completed;

        private Matrix[] skinTransforms = null;
        private Matrix[] boneTransforms = null;
        private ModelSkinning skinning = null;
        private double blendTimer = 0;

        private static BoneAnimationItem[,] weightedBones = null;

        /// <summary>
        /// Starts this BoneAnimation with default blending enabled.
        /// </summary>
        public void Play(IBoneAnimationController blendTarget)
        {
            if (blendTarget != null)
            {
                DefaultBlendEnabled = true;
                DefaultBlendTarget = blendTarget;
                blendTimer = 0;
            }
            Play();
        }
        
        protected override void OnStarted()
        {
            if (Controllers.Count > 2)
            {
                throw new NotImplementedException(
                    "BoneAnimation only supports at most 2 animations by now.");
            }

            if (Controllers.Count > 0 && (weightedBones == null || weightedBones.Length < Controllers.Count))
            {
                weightedBones = new BoneAnimationItem[Controllers.Count, Model.Bones.Count];
            }

            if (skinTransforms == null && (skinning = Model.GetSkinning()) != null)
                skinTransforms = new Matrix[skinning.InverseBindPose.Count];
            if (boneTransforms == null)
                boneTransforms = new Matrix[Model.Bones.Count];

            if (isSychronized && keyController != null)
            {
                TimeSpan duration = keyController.Duration;
                foreach (IBoneAnimationController controller in Controllers)
                {
                    if (controller == keyController || !(controller is TimelineAnimation))
                        continue;
                    
                    TimelineAnimation animation = (TimelineAnimation)controller;
                    animation.Speed = (float)(
                        (double)animation.Duration.Ticks * keyController.Speed / duration.Ticks);
                }
            }

            foreach (IBoneAnimationController controller in Controllers)
            {
                if (controller is IAnimation)
                    ((IAnimation)controller).Play();
            }

            base.OnStarted();
        }
        
        protected override void OnStopped()
        {
            foreach (IBoneAnimationController controller in Controllers)
            {
                if (controller is IAnimation)
                    ((IAnimation)controller).Stop();
            }

            base.OnStopped();
        }

        protected override void OnPaused()
        {
            foreach (IBoneAnimationController controller in Controllers)
            {
                if (controller is IAnimation)
                    ((IAnimation)controller).Pause();
            }

            base.OnPaused();
        }

        protected override void OnResumed()
        {
            foreach (IBoneAnimationController controller in Controllers)
            {
                if (controller is IAnimation)
                    ((IAnimation)controller).Resume();
            }

            base.OnResumed();
        }

        protected virtual void OnCompleted()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        public override void Update(GameTime gameTime)
        {
            if (State == AnimationState.Playing && Model != null)
            {
                UpdateLayeredAnimation(gameTime);
                UpdateBoneTransforms(gameTime);
            }
        }

        private void UpdateLayeredAnimation(GameTime gameTime)
        {
            foreach (IBoneAnimationController controller in Controllers)
            {
                if (controller is IUpdateObject)
                    ((IUpdateObject)controller).Update(gameTime);
            }

            bool allStopped = true;

            if (KeyController != null)
            {
                allStopped = (KeyController.State == AnimationState.Stopped);
            }
            else
            {
                foreach (IBoneAnimationController controller in Controllers)
                {
                    if (controller is IAnimation && ((IAnimation)controller).State != AnimationState.Stopped)
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

        private void UpdateBoneTransforms(GameTime gameTime)
        {
            // Update default blend
            float blendLerp = 1;
            if (DefaultBlendEnabled && DefaultBlendTarget != null)
            {
                blendTimer += gameTime.ElapsedGameTime.TotalSeconds;

                if (blendTimer < DefaultBlendDuration.TotalSeconds)
                {
                    blendLerp = (float)(blendTimer / DefaultBlendDuration.TotalSeconds);
                }
                else
                {
                    blendTimer = 0;
                    DefaultBlendTarget = null;
                }
            }

            // Update controller transforms
            Array.Clear(weightedBones, 0, weightedBones.Length);

            for (int animationIndex = 0; animationIndex < Controllers.Count; animationIndex++)
            {
                for (int bone = 0; bone < Model.Bones.Count; bone++)
                {
                    if (Controllers[animationIndex].Controller.TryGetBoneTransform(
                                            bone, out weightedBones[animationIndex, bone].Transform,
                                                  out weightedBones[animationIndex, bone].BlendWeight))
                    {
                        weightedBones[animationIndex, bone].BlendWeight *= Controllers[animationIndex].BlendWeight * (
                            Controllers[animationIndex].BoneWeights[bone].Enabled ?
                            Controllers[animationIndex].BoneWeights[bone].BlendWeight : 0);
                    }
                }
            }
            
            // Normalize weights
            for (int bone = 0; bone < Model.Bones.Count; bone++)
            {
                Matrix transform = new Matrix();
                float totalWeight = 0;

                for (int animationIndex = 0; animationIndex < Controllers.Count; animationIndex++)
                {
                    totalWeight += weightedBones[animationIndex, bone].BlendWeight;
                }

                if (totalWeight <= 0)
                    continue;

                for (int animationIndex = 0; animationIndex < Controllers.Count; animationIndex++)
                {
                    // TODO: Implement blending from multiple sources
                    //
                    //transform += bones[animationIndex, bone].Transform * (bones[animationIndex, bone].BlendWeight / totalWeight);
                }

                if (Controllers.Count == 1)
                    transform = weightedBones[0, bone].Transform;
                else if (Controllers.Count == 2)
                    transform = LerpHelper.Slerp(weightedBones[0, bone].Transform, weightedBones[1, bone].Transform, weightedBones[1, bone].BlendWeight / totalWeight);

                // Perform default blend
                Matrix defaultBlendTransform;
                float defaultBlendTargetBlendWeight;
                if (DefaultBlendEnabled && DefaultBlendTarget != null &&
                    DefaultBlendTarget.TryGetBoneTransform(bone, out defaultBlendTransform, out defaultBlendTargetBlendWeight))
                {
                    transform = LerpHelper.Slerp(defaultBlendTransform, transform, MathHelper.SmoothStep(0, 1, blendLerp));
                }

                boneTransforms[bone] = transform;
                Model.Bones[bone].Transform = transform;
            }

            if (skinning != null && skinTransforms != null)
                skinning.GetBoneTransforms(Model, skinTransforms);
        }

        public Matrix[] GetBoneTransforms()
        {
            return skinTransforms;
        }

        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            blendWeight = 1;

            if (bone >= 0 && bone < boneTransforms.Length)
            {
                transform = boneTransforms[bone];
                return true;
            }

            transform = Matrix.Identity;
            return false;
        }
    }

    internal struct BoneAnimationItem
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
        internal Model Model;

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
        internal WeightedBoneAnimationController(Model model, IBoneAnimationController controller)
        {
            List<WeightedBoneAnimationControllerBone> bones = new List<WeightedBoneAnimationControllerBone>(model.Bones.Count);
            for (int i = 0; i < model.Bones.Count; i++)
                bones.Add(new WeightedBoneAnimationControllerBone());

            this.Model = model;
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
            BoneWeights[bone].Enabled = enabled;

            if (recursive)
            {
                foreach (ModelBone child in Model.Bones[bone].Children)
                {
                    SetEnabled(child.Index, enabled, true);
                }
            }
        }
    }

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
            get { return this[animation.Model.Bones[name].Index]; }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BoneAnimationControllerCollection : ICollection<IBoneAnimationController>
    {
        private Model Model;
        private List<WeightedBoneAnimationController> controllers = new List<WeightedBoneAnimationController>();

        internal BoneAnimationControllerCollection(Model model)
        {
            this.Model = model;
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
            controllers.Add(new WeightedBoneAnimationController(Model, item) { BlendWeight = blendWeight });
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
            foreach (WeightedBoneAnimationController controller in controllers)
                yield return controller.Controller;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    #endregion
    #endregion
}
