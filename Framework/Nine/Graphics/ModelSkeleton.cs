#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.ObjectModel;
#endregion

namespace Nine.Graphics
{
    #region IBoneHierarchy
    /// <summary>
    /// Represents a bone hierarchy that can be animated by <c>BoneAnimation</c>.
    /// </summary>
    public interface IBoneHierarchy
    {
        /// <summary>
        /// Gets a fixed sized array of transformation matrices for each bone
        /// according to its parent bone.
        /// </summary>
        Matrix[] BoneTransforms { get; }

        /// <summary>
        /// Gets a collection of names for each bone.
        /// Return null if the skeleton does not have a name for each bone.
        /// </summary>
        ReadOnlyCollection<string> BoneNames { get; }

        /// <summary>
        /// Gets the hierarchical relationship between bones.
        /// </summary>
        ReadOnlyCollection<int> ParentBones { get; }

        /// <summary>
        /// Gets the index of the root bone of the skeleton.
        /// Return null if the skeleton is not intended for skinned models.
        /// </summary>
        int SkeletonRoot { get; }

        /// <summary>
        /// Gets a collection of inverse transformation matrices for each bone
        /// according to the skeleton root bone.
        /// Return null if the skeleton is not intended for skinned models.
        /// </summary>
        ReadOnlyCollection<Matrix> InverseAbsoluteBindPose { get; }
    }
    #endregion

    #region ModelSkeleton
    /// <summary>
    /// Defines a bone hierarchy used by models.
    /// </summary>
    public class ModelSkeleton : IBoneHierarchy
    {
        public int SkeletonRoot { get; private set; }
        public Matrix[] BoneTransforms { get; private set; }
        public ReadOnlyCollection<string> BoneNames { get; private set; }
        public ReadOnlyCollection<int> ParentBones { get; private set; }
        public ReadOnlyCollection<Matrix> InverseAbsoluteBindPose { get; private set; }

        /// <summary>
        /// Initializes a new instance of <c>ModelSkeleton</c>.
        /// </summary>
        public ModelSkeleton(Model model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var skeleton = model.GetSkeletonData();
            if (skeleton != null)
            {
                SkeletonRoot = skeleton.SkeletonRoot;
                InverseAbsoluteBindPose = new ReadOnlyCollection<Matrix>(skeleton.InverseAbsoluteBindPose);
            }

            var boneTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(boneTransforms);
            BoneTransforms = boneTransforms;
            BoneNames = new ReadOnlyCollection<string>(new BoneNameCollection() { Model = model });
            ParentBones = new ReadOnlyCollection<int>(new ParentBoneCollection() { Model = model });
        }

        #region Collections
        class BoneNameCollection : IList<string>
        {
            internal Model Model;

            public int IndexOf(string item)
            {
                var bone = Model.Bones.FirstOrDefault(b => b.Name == item);
                return bone != null ? bone.Index : -1;
            }

            public void Insert(int index, string item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public string this[int index]
            {
                get { return Model.Bones[index].Name; }
                set { throw new NotSupportedException(); }
            }

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                return Model.Bones.Any(b => b.Name == item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                for (int i = 0; i < Model.Bones.Count; i++)
                    array[i + arrayIndex] = Model.Bones[i].Name;
            }

            public int Count
            {
                get { return Model.Bones.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<string> GetEnumerator()
            {
                return Model.Bones.Select(b => b.Name).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class ParentBoneCollection : IList<int>
        {
            internal Model Model;

            public int IndexOf(int item)
            {
                return this.First(i => i == item);
            }

            public void Insert(int index, int item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public int this[int index]
            {
                get { var parent = Model.Bones[index].Parent; return parent != null ? parent.Index : -1; }
                set { throw new NotSupportedException(); }
            }

            public void Add(int item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(int item)
            {
                return this.Any(i => i == item);
            }

            public void CopyTo(int[] array, int arrayIndex)
            {
                for (int i = 0; i < Model.Bones.Count; i++)
                    array[i + arrayIndex] = this[i];
            }

            public int Count
            {
                get { return Model.Bones.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(int item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<int> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion
    }
    #endregion

    #region ModelSkeletonData
    /// <summary>
    /// Combines all the data needed to render and animate a skinned object.
    /// This is typically stored in the Tag property of the Model being animated.
    /// </summary>
    class ModelSkeletonData
    {
        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> InverseAbsoluteBindPose { get; internal set; }
        
        
        /// <summary>
        /// Index of the skeleton root on the parent mesh bone collection.
        /// </summary>
        [ContentSerializer]
        public int SkeletonRoot { get; internal set; }
        
        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        internal ModelSkeletonData(List<Matrix> inverseBindPose, int skeleton)
        {
            if (skeleton < 0 || inverseBindPose == null || inverseBindPose.Count <= 0)
                throw new ArgumentException("Error creating skinner.");

            InverseAbsoluteBindPose = inverseBindPose;
            SkeletonRoot = skeleton;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        internal ModelSkeletonData() { }
    }
    #endregion

    #region BoneHierarchyExtensions
    /// <summary>
    /// Contains extension methods to bone hierarchy.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BoneHierarchyExtensions
    {
        /// <summary>
        /// Gets the index of the parent bone.
        /// </summary>
        public static int GetParentBone(this IBoneHierarchy skeleton, int bone)
        {
            return skeleton.ParentBones[bone];
        }

        /// <summary>
        /// Gets all the child bones of the input bone.
        /// </summary>
        public static IEnumerable<int> GetChildBones(this IBoneHierarchy skeleton, int bone)
        {
            for (int i = bone + 1; i < skeleton.ParentBones.Count; i++)
                if (skeleton.ParentBones[i] == bone)
                    yield return i;
        }

        /// <summary>
        /// Gets the name of the bone.
        /// </summary>
        public static string GetBoneName(this IBoneHierarchy skeleton, int bone)
        {
            return skeleton.BoneNames[bone];
        }

        /// <summary>
        /// Gets the index of the bone.
        /// </summary>
        public static int GetBone(this IBoneHierarchy skeleton, string boneName)
        {
            return skeleton.BoneNames.IndexOf(boneName);
        }

        /// <summary>
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public static Matrix GetAbsoluteBoneTransform(this IBoneHierarchy skeleton, int bone)
        {
            Matrix absoluteTransform = skeleton.BoneTransforms[bone];

            while ((bone = GetParentBone(skeleton, bone)) >= 0)
            {
                absoluteTransform = absoluteTransform * skeleton.BoneTransforms[bone];
            }

            return absoluteTransform;
        }

        /// <summary>
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public static Matrix GetAbsoluteBoneTransform(this IBoneHierarchy skeleton, string boneName)
        {
            return GetAbsoluteBoneTransform(skeleton, GetBone(skeleton, boneName));
        }

        /// <summary>
        /// Copies the aboslute transforms of all the bones.
        /// </summary>
        public static void CopyAbsoluteBoneTransformsTo(this IBoneHierarchy skeleton, Matrix[] destinationBoneTransforms)
        {
            if (destinationBoneTransforms == null)
                throw new ArgumentNullException("destinationBoneTransforms");

            if (destinationBoneTransforms.Length < skeleton.BoneTransforms.Length)
                throw new ArgumentOutOfRangeException("destinationBoneTransforms");

            int parent = 0;
            for (int i = 0; i < skeleton.BoneTransforms.Length; i++)
            {
                if ((parent = GetParentBone(skeleton, i)) < 0)
                {
                    destinationBoneTransforms[i] = skeleton.BoneTransforms[i];
                }
                else
                {
                    destinationBoneTransforms[i] = skeleton.BoneTransforms[i] * destinationBoneTransforms[parent];
                }
            }
        }

        /// <summary>
        /// Copies the local transforms of all the bones.
        /// </summary>
        public static void CopyBoneTransformsTo(this IBoneHierarchy skeleton, Matrix[] destinationBoneTransforms)
        {
            skeleton.BoneTransforms.CopyTo(destinationBoneTransforms, 0);
        }

        /// <summary>
        /// Gets the local transform of the specified bone.
        /// </summary>
        public static Matrix GetBoneTransform(this IBoneHierarchy skeleton, int bone)
        {
            return skeleton.BoneTransforms[bone];
        }

        /// <summary>
        /// Gets the local transform of the specified bone.
        /// </summary>
        public static Matrix GetBoneTransform(this IBoneHierarchy skeleton, string boneName)
        {
            return GetBoneTransform(skeleton, GetBone(skeleton, boneName));
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        /// <returns>
        /// A matrix array used to draw skinned meshes.
        /// </returns>
        /// <remarks>
        /// Whenever the bone or skeleton changes, you should re-skin the model.
        /// </remarks>
        public static Matrix[] GetSkinTransforms(this IBoneHierarchy skeleton)
        {
            if (skeleton.InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            Matrix[] skin = new Matrix[skeleton.InverseAbsoluteBindPose.Count];

            if (bones == null || bones.Length < skeleton.BoneTransforms.Length)
                bones = new Matrix[skeleton.BoneTransforms.Length];

            skeleton.CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < skeleton.InverseAbsoluteBindPose.Count; i++)
            {
                // Apply inverse bind pose
                skin[i] = skeleton.InverseAbsoluteBindPose[i] * bones[skeleton.SkeletonRoot + i];
            }

            return skin;
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        /// <param name="world">
        /// A world matrix that will be applied to the result bone transforms.
        /// </param>
        /// <param name="model"></param>
        /// <returns>
        /// A matrix array used to draw skinned meshes.
        /// </returns>
        /// <remarks>
        /// Whenever the bone or skeleton changes, you should re-skin the model.
        /// </remarks>
        public static Matrix[] GetSkinTransforms(this IBoneHierarchy skeleton, Matrix world)
        {
            if (skeleton.InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            Matrix[] skin = new Matrix[skeleton.InverseAbsoluteBindPose.Count];

            if (bones == null || bones.Length < skeleton.BoneTransforms.Length)
                bones = new Matrix[skeleton.BoneTransforms.Length];

            skeleton.CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < skeleton.InverseAbsoluteBindPose.Count; i++)
            {
                // Apply inverse bind pose
                skin[i] = skeleton.InverseAbsoluteBindPose[i] * bones[skeleton.SkeletonRoot + i] * world;
            }

            return skin;
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        /// <param name="skin">
        /// A matrix array to hold the result transformations.
        /// The length must be at least InverseBindPose.Count.
        /// </param>
        public static void GetSkinTransforms(this IBoneHierarchy skeleton, Matrix world, Matrix[] skinTransforms)
        {
            if (skeleton.InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            if (bones == null || bones.Length < skeleton.BoneTransforms.Length)
                bones = new Matrix[skeleton.BoneTransforms.Length];

            skeleton.CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < skeleton.InverseAbsoluteBindPose.Count; i++)
            {
                // Apply inverse bind pose
                skinTransforms[i] = skeleton.InverseAbsoluteBindPose[i] * bones[skeleton.SkeletonRoot + i] * world;
            }
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        public static void GetSkinTransforms(this IBoneHierarchy skeleton, Matrix[] skinTransforms)
        {
            if (skeleton.InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            if (bones == null || bones.Length < skeleton.BoneTransforms.Length)
                bones = new Matrix[skeleton.BoneTransforms.Length];

            skeleton.CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < skeleton.InverseAbsoluteBindPose.Count; i++)
            {
                // Apply inverse bind pose
                skinTransforms[i] = skeleton.InverseAbsoluteBindPose[i] * bones[skeleton.SkeletonRoot + i];
            }
        }

        static Matrix[] bones = null;
    }
    #endregion
}
