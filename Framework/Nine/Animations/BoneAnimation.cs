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

    #region BoneAnimation
    public class BoneAnimation : Animation, IBoneAnimationController
    {
        public BoneAnimation(Model model, object modelInstance)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Model = model;
            this.BlendEnabled = modelInstance != null;
            this.ModelInstance = modelInstance;
            this.BlendDuration = TimeSpan.FromSeconds(0.5);
            this.Controllers = new BoneAnimationControllerCollection(model);
        }
        
        public BoneAnimation(Model model, object modelInstance, BoneAnimationClip animation)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Model = model;
            this.BlendEnabled = modelInstance != null;
            this.ModelInstance = modelInstance;
            this.BlendDuration = TimeSpan.FromSeconds(0.5);
            this.Controllers = new BoneAnimationControllerCollection(model);
            this.Controllers.Add(new BoneAnimationController(animation));
        }
        
        /// <summary>
        /// Gets the model affected by this bone animation.
        /// </summary>
        public Model Model { get; private set; }

        /// <summary>
        /// Gets the object identifying the model instance affected by this
        /// animation from other model instances.
        /// This value is required to enabled default blending.
        /// </summary>
        public object ModelInstance { get; private set; }
        
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

        private Matrix[] blendTarget;
        private Matrix[] boneTransformPerModelInstance;

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
                        "The specified controller must be added to this animation.");

                keyController = value;
            }
        }

        private BoneAnimationController keyController;

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

        /// <summary>
        /// Occurs when this animation has completely finished playing.
        /// </summary>
        public override event EventHandler Completed;

        private Matrix[] skinTransforms = null;
        private Matrix[] boneTransforms = null;
        private ModelSkinning skinning = null;
        private double blendTimer = 0;

        private static Matrix[] skinIntermediateTransforms = null;
        private static BoneAnimationItem[,] weightedBones = null;
        private static Dictionary<object, Matrix[]> boneTransformSnapshots = null;
        
        protected override void OnStarted()
        {
            blendTimer = 0;

            if (BlendEnabled && ModelInstance == null)
            {
                throw new InvalidOperationException(
                    "A valid ModelInstance needs to be set when default blend is enabled.");
            }

            if (Controllers.Count > 0 && (weightedBones == null || weightedBones.GetUpperBound(0) < Controllers.Count))
            {
                weightedBones = new BoneAnimationItem[Controllers.Count, Model.Bones.Count];
            }

            if (skinTransforms == null && (skinning = Model.GetSkinning()) != null)
            {
                skinTransforms = new Matrix[skinning.InverseBindPose.Count];
            }

            if (boneTransforms == null)
            {
                boneTransforms = new Matrix[Model.Bones.Count];
                Model.CopyBoneTransformsTo(boneTransforms);
            }

            if (BlendEnabled)
            {
                if (boneTransformSnapshots == null)
                    boneTransformSnapshots = new Dictionary<object, Matrix[]>();

                if (!boneTransformSnapshots.TryGetValue(ModelInstance, out boneTransformPerModelInstance))
                {
                    boneTransformPerModelInstance = new Matrix[Model.Bones.Count];
                    Model.CopyBoneTransformsTo(boneTransformPerModelInstance);
                    boneTransformSnapshots.Add(ModelInstance, boneTransformPerModelInstance);
                }

                if (blendTarget == null)
                    blendTarget = new Matrix[Model.Bones.Count];

                boneTransformPerModelInstance.CopyTo(blendTarget, 0);
            }

            SychronizeSpeed();

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
                UpdateControllers(gameTime);
                UpdateBoneTransforms(gameTime);
                UpdateSkinTransform();
            }
        }

        private void UpdateControllers(GameTime gameTime)
        {
            SychronizeSpeed();

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
                    if (controller is IAnimation)
                    {
                        if (((IAnimation)controller).State != AnimationState.Stopped)
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
                foreach (IBoneAnimationController controller in Controllers)
                {
                    if (controller == keyController || !(controller is TimelineAnimation))
                        continue;

                    TimelineAnimation animation = (TimelineAnimation)controller;
                    animation.Speed = (float)(
                        (double)animation.Duration.Ticks * keyController.Speed / duration.Ticks);
                }
            }
        }

        private void UpdateBoneTransforms(GameTime gameTime)
        {
            // Update default blend
            float blendLerp = 0;

            if (BlendEnabled && blendTarget != null)
            {
                blendTimer += gameTime.ElapsedGameTime.TotalSeconds;
                blendLerp = (float)(blendTimer / BlendDuration.TotalSeconds);
            }

            // Update controller transforms
            Array.Clear(weightedBones, 0, weightedBones.Length);

            for (int controllerIndex = 0; controllerIndex < Controllers.Count; controllerIndex++)
            {
                for (int bone = 0; bone < Model.Bones.Count; bone++)
                {
                    if (Controllers[controllerIndex].Controller.TryGetBoneTransform(
                                            bone, out weightedBones[controllerIndex, bone].Transform,
                                                  out weightedBones[controllerIndex, bone].BlendWeight))
                    {
                        weightedBones[controllerIndex, bone].BlendWeight *= Controllers[controllerIndex].BlendWeight * (
                            Controllers[controllerIndex].BoneWeights[bone].Enabled ?
                            Controllers[controllerIndex].BoneWeights[bone].BlendWeight : 0);
                    }
                    else
                    {
                        weightedBones[controllerIndex, bone].BlendWeight = 0;
                    }
                }
            }
            
            // Normalize weights
            for (int bone = 0; bone < Model.Bones.Count; bone++)
            {
                int firstNonZeroChannel = -1;
                float totalWeight = 0;

                for (int controllerIndex = 0; controllerIndex < Controllers.Count; controllerIndex++)
                {
                    if (firstNonZeroChannel < 0 && weightedBones[controllerIndex, bone].BlendWeight > 0)
                        firstNonZeroChannel = controllerIndex;

                    totalWeight += weightedBones[controllerIndex, bone].BlendWeight;
                }

                if (totalWeight <= 0)
                    continue;

                Matrix transform = weightedBones[firstNonZeroChannel, bone].Transform;

                for (int controllerIndex = firstNonZeroChannel + 1; controllerIndex < Controllers.Count; controllerIndex++)
                {
                    if (weightedBones[controllerIndex, bone].BlendWeight <= float.Epsilon)
                        continue;

                    // This is not mathmatically correct, but provides an acceptable result.
                    transform = LerpHelper.Slerp(transform,
                                                 weightedBones[controllerIndex, bone].Transform,
                                                 weightedBones[controllerIndex, bone].BlendWeight / totalWeight);
                }

                // Perform default blend
                if (BlendEnabled && blendTarget != null && blendLerp <= 1)
                {
                    transform = LerpHelper.Slerp(blendTarget[bone], transform, MathHelper.SmoothStep(0, 1, blendLerp));
                }

                if (boneTransformPerModelInstance != null)
                    boneTransformPerModelInstance[bone] = transform;
                boneTransforms[bone] = transform;
            }
        }

        private void UpdateSkinTransform()
        {
            if (skinning != null && skinTransforms != null)
            {
                if (skinIntermediateTransforms == null || skinIntermediateTransforms.Length < Model.Bones.Count)
                    skinIntermediateTransforms = new Matrix[Model.Bones.Count];

                CopyAbsoluteBoneTransformsTo(skinIntermediateTransforms);

                for (int i = 0; i < skinning.InverseBindPose.Count; i++)
                {
                    // Apply inverse bind pose
                    skinTransforms[i] = skinning.InverseBindPose[i] * skinIntermediateTransforms[skinning.SkeletonIndex + i];
                }
            }
        }

        public int GetParentBone(int bone)
        {
            ModelBone parent = Model.Bones[bone].Parent;
            return parent != null ? parent.Index : -1;
        }

        public Matrix[] GetBoneTransforms()
        {
            return skinTransforms;
        }

        /// <summary>
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public Matrix GetAbsoluteBoneTransform(int boneIndex)
        {
            if (boneTransforms == null)
                return Model.GetAbsoluteBoneTransform(boneIndex);

            ModelBone bone = Model.Bones[boneIndex];
            Matrix absoluteTransform = boneTransforms[bone.Index];

            while (bone.Parent != null)
            {
                bone = bone.Parent;
                absoluteTransform = absoluteTransform * boneTransforms[bone.Index];
            }

            return absoluteTransform;
        }

        /// <summary>
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public Matrix GetAbsoluteBoneTransform(string boneName)
        {
            return GetAbsoluteBoneTransform(Model.Bones[boneName].Index);
        }

        public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
        {
            if (boneTransforms == null)
            {
                Model.CopyAbsoluteBoneTransformsTo(destinationBoneTransforms);
                return;
            }

            if (destinationBoneTransforms == null)
                throw new ArgumentNullException("destinationBoneTransforms");

            if (destinationBoneTransforms.Length < boneTransforms.Length)
                throw new ArgumentOutOfRangeException("destinationBoneTransforms");

            for (int i = 0; i < boneTransforms.Length; i++)
            {
                ModelBone bone = Model.Bones[i];
                if (bone.Parent == null)
                {
                    destinationBoneTransforms[i] = boneTransforms[i];
                }
                else
                {
                    destinationBoneTransforms[i] = boneTransforms[i] * destinationBoneTransforms[bone.Parent.Index];
                }
            }
        }

        public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms)
        {
            if (boneTransforms == null)
            {
                Model.CopyBoneTransformsTo(destinationBoneTransforms);
                return;
            }

            if (destinationBoneTransforms == null)
                throw new ArgumentNullException("destinationBoneTransforms");

            if (destinationBoneTransforms.Length < boneTransforms.Length)
                throw new ArgumentOutOfRangeException("destinationBoneTransforms");

            for (int i = 0; i < boneTransforms.Length; i++)
            {
                destinationBoneTransforms[i] = boneTransforms[i];
            }
        }

        /// <summary>
        /// Gets the local transform of the specified bone.
        /// </summary>
        public Matrix GetBoneTransform(int bone)
        {
            if (bone >= 0 && bone < boneTransforms.Length)
            {
                return boneTransforms[bone];
            }

            throw new ArgumentOutOfRangeException("bone");
        }
        
        /// <summary>
        /// Gets the local transform of the specified bone.
        /// </summary>
        public Matrix GetBoneTransform(string boneName)
        {
            return GetBoneTransform(Model.Bones[boneName].Index);
        }

        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            blendWeight = 1;

            if (boneTransforms != null && bone >= 0 && bone < boneTransforms.Length)
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
            if (controller == null)
                throw new ArgumentNullException("controller");

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
