#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using Nine.Graphics;
using Nine.Animations;

namespace Nine
{
    /// <summary>
    /// Defines an animation controller that controls the target skeleton using the source skeleton.
    /// </summary>
    /// <remarks>
    /// !!! This controller is currently not working !!!
    /// </remarks>
    public class SkeletonController : IBoneAnimationController
    {
        /// <summary>
        /// Gets or sets whether this controller is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the target skeleton to be manipulated.
        /// </summary>
        public Skeleton TargetSkeleton { get; set; }

        /// <summary>
        /// Gets or sets the source skeleton that provides the transform info.
        /// </summary>
        public Skeleton SourceSkeleton { get; set; }

        private Dictionary<int, SkeletonControllerBoneBinding> bindings;

        public SkeletonController(Skeleton targetSkeleton, Skeleton sourceSkeleton, SkeletonControllerBoneBinding[] boneBindings)
        {
            if (boneBindings == null)
                throw new ArgumentNullException("boneBindings");

            this.Enabled = true;
            this.TargetSkeleton = targetSkeleton;
            this.SourceSkeleton = sourceSkeleton;
            this.bindings = boneBindings.ToDictionary(b => b.Bone);
        }

        public bool TryGetBoneTransform(int bone, out Matrix transform, out float blendWeight)
        {
            if (!Enabled || SourceSkeleton == null || TargetSkeleton == null)
            {
                blendWeight = 0;
                transform = Matrix.Identity;
                return false;
            }

            if (TargetSkeleton == SourceSkeleton)
            {
                throw new ArgumentException("Target skeleton and source skeleton must not be equal");
            }

            if (bone >= TargetSkeleton.BoneTransforms.Length)
            {
                throw new InvalidOperationException(
                    string.Format("Bone {0} in the bone binding does not exist on the target skeleton", bone));
            }

            SkeletonControllerBoneBinding binding;
            if (bindings.TryGetValue(bone, out binding))
            {
                transform = new Matrix();

                int parentBone = TargetSkeleton.GetParentBone(bone);

                Matrix target = TargetSkeleton.GetBoneTransform(bone);
                Matrix invertParentTargetRotation = parentBone >= 0 ? Matrix.Invert(TargetSkeleton.GetAbsoluteBoneTransform(parentBone)) : Matrix.Identity;
                invertParentTargetRotation.Translation = Vector3.Zero;

                for (int i = 0; i < binding.BlendSource.Length; i++)
                {
                    Matrix source = SourceSkeleton.GetBoneTransform(binding.BlendSource[i]) * invertParentTargetRotation;
                    transform += GetNomalizedBoneTransform(source, target) * binding.BlendWeights[i];
                }
                blendWeight = 1;
                return true;
            }

            blendWeight = 0;
            transform = Matrix.Identity;
            return false;
        }

        private Matrix GetNomalizedBoneTransform(Matrix source, Matrix target)
        {
            // Apply source rotation to target transform
            Vector3 to = Vector3.Normalize(source.Translation);
            Vector3 from = Vector3.Normalize(target.Translation);
            Vector3 axis = Vector3.Cross(from, to);

            axis.Normalize();

            if (float.IsNaN(axis.X))
            {
                return target;
            }

            Matrix result = target * Matrix.CreateFromAxisAngle(axis, (float)Math.Acos(MathHelper.Clamp(Vector3.Dot(from, to), -1, 1)));
            return result;
        }
    }

    public class SkeletonControllerBoneBinding
    {
        public int Bone { get; private set; }
        public int[] BlendSource { get; private set; }
        public float[] BlendWeights { get; private set; }

        public SkeletonControllerBoneBinding(int bone, int blendSource)
            : this(bone, new int[] { blendSource }, new float[] { 1 })
        {

        }

        public SkeletonControllerBoneBinding(int bone, int[] blendSource, float[] blendWeights)
        {
            if (bone < 0)
                throw new ArgumentOutOfRangeException("bone");
            if (blendSource == null)
                throw new ArgumentNullException("blendSource");
            if (blendWeights == null)
                throw new ArgumentNullException("blendWeights");
            if (blendSource.Length != blendWeights.Length)
                throw new ArgumentException("The length of blend source and blend weights must be equal.");

            this.Bone = bone;
            this.BlendSource = blendSource;
            this.BlendWeights = blendWeights;
        }
    }
}
