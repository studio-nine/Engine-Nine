namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    #region Skeleton
    /// <summary>
    /// Represents a bone hierarchy that can be animated by <c>BoneAnimation</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class Skeleton
    {
        /// <summary>
        /// Gets a fixed sized array of transformation matrices for each bone
        /// according to its parent bone.
        /// </summary>
        public abstract Matrix[] BoneTransforms { get; }

        /// <summary>
        /// Gets the hierarchical relationship between bones.
        /// </summary>
        public abstract ReadOnlyCollection<int> ParentBones { get; }

        /// <summary>
        /// Gets a collection of names for each bone.
        /// Return null if the skeleton does not have a name for each bone.
        /// </summary>
        public ReadOnlyCollection<string> BoneNames { get; protected set; }

        /// <summary>
        /// Gets the index of the root bone of the skeleton.
        /// </summary>
        public int SkeletonRoot { get; protected set; }

        /// <summary>
        /// Gets a collection of inverse transformation matrices for each bone
        /// according to the skeleton root bone.
        /// Return null if the skeleton is not intended for skinned models.
        /// </summary>
        public ReadOnlyCollection<Matrix> InverseAbsoluteBindPose { get; protected set; }

        /// <summary>
        /// Keep track of whether this skeleton has been animated.
        /// We don't perform blending when a skeleton has just been created.
        /// </summary>
        internal bool HasAnimated;

        /// <summary>
        /// Gets the index of the parent bone.
        /// </summary>
        public int GetParentBone(int bone)
        {
            return ParentBones[bone];
        }

        /// <summary>
        /// Gets all the child bones of the input bone.
        /// </summary>
        public IEnumerable<int> GetChildBones(int bone)
        {
            var count = ParentBones.Count;
            for (int i = bone + 1; i < count; ++i)
                if (ParentBones[i] == bone)
                    yield return i;
        }

        /// <summary>
        /// Gets all the child bones of the input bone.
        /// </summary>
        public void GetChildBones(int bone, ICollection<int> childBones)
        {
            var count = ParentBones.Count;
            for (int i = bone + 1; i < count; ++i)
                if (ParentBones[i] == bone)
                    childBones.Add(i);
        }

        /// <summary>
        /// Gets the name of the bone.
        /// </summary>
        public string GetBoneName(int bone)
        {
            return BoneNames[bone];
        }

        /// <summary>
        /// Gets the index of the bone.
        /// </summary>
        public int GetBone(string boneName)
        {
            return BoneNames.IndexOf(boneName);
        }

        /// <summary>
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public Matrix GetAbsoluteBoneTransform(int bone)
        {
            Matrix absoluteTransform = BoneTransforms[bone];

            while ((bone = GetParentBone(bone)) >= 0)
            {
                absoluteTransform = absoluteTransform * BoneTransforms[bone];
            }

            return absoluteTransform;
        }

        /// <summary>
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public Matrix GetAbsoluteBoneTransform(string boneName)
        {
            return GetAbsoluteBoneTransform(GetBone(boneName));
        }

        /// <summary>
        /// Copies the aboslute transforms of all the bones.
        /// </summary>
        public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
        {
            if (destinationBoneTransforms == null)
                throw new ArgumentNullException("destinationBoneTransforms");

            if (destinationBoneTransforms.Length < BoneTransforms.Length)
                throw new ArgumentOutOfRangeException("destinationBoneTransforms");

            int parent = 0;
            for (int i = 0; i < BoneTransforms.Length; ++i)
            {
                if ((parent = GetParentBone(i)) < 0)
                {
                    destinationBoneTransforms[i] = BoneTransforms[i];
                }
                else
                {
                    Matrix.Multiply(ref BoneTransforms[i], ref destinationBoneTransforms[parent], out destinationBoneTransforms[i]);
                }
            }
        }

        /// <summary>
        /// Copies the local transforms of all the bones.
        /// </summary>
        public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms)
        {
            BoneTransforms.CopyTo(destinationBoneTransforms, 0);
        }

        /// <summary>
        /// Gets the local transform of the specified bone.
        /// </summary>
        public Matrix GetBoneTransform(int bone)
        {
            return BoneTransforms[bone];
        }

        /// <summary>
        /// Gets the local transform of the specified bone.
        /// </summary>
        public Matrix GetBoneTransform(string boneName)
        {
            return GetBoneTransform(GetBone(boneName));
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
        public Matrix[] GetSkinTransforms()
        {
            if (InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            Matrix[] skin = new Matrix[InverseAbsoluteBindPose.Count];

            if (bones == null || bones.Length < BoneTransforms.Length)
                bones = new Matrix[BoneTransforms.Length];

            CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < InverseAbsoluteBindPose.Count; ++i)
            {
                // Apply inverse bind pose
                skin[i] = InverseAbsoluteBindPose[i] * bones[SkeletonRoot + i];
            }

            return skin;
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        /// <param name="world">
        /// A world matrix that will be applied to the result bone transforms.
        /// </param>
        /// <returns>
        /// A matrix array used to draw skinned meshes.
        /// </returns>
        /// <remarks>
        /// Whenever the bone or skeleton changes, you should re-skin the model.
        /// </remarks>
        public Matrix[] GetSkinTransforms(Matrix world)
        {
            if (InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            Matrix[] skin = new Matrix[InverseAbsoluteBindPose.Count];

            if (bones == null || bones.Length < BoneTransforms.Length)
                bones = new Matrix[BoneTransforms.Length];

            CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < InverseAbsoluteBindPose.Count; ++i)
            {
                // Apply inverse bind pose
                skin[i] = InverseAbsoluteBindPose[i] * bones[SkeletonRoot + i] * world;
            }

            return skin;
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        /// <param name="skinTransforms">
        /// A matrix array to hold the result transformations.
        /// The length must be at least InverseBindPose.Count.
        /// </param>
        public void GetSkinTransforms(Matrix world, Matrix[] skinTransforms)
        {
            if (InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            if (bones == null || bones.Length < BoneTransforms.Length)
                bones = new Matrix[BoneTransforms.Length];

            CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < InverseAbsoluteBindPose.Count; ++i)
            {
                // Apply inverse bind pose
                Matrix inverseAbsoluteBindPose = InverseAbsoluteBindPose[i];
                Matrix.Multiply(ref inverseAbsoluteBindPose, ref bones[SkeletonRoot + i], out skinTransforms[i]);
                Matrix.Multiply(ref skinTransforms[i], ref world, out skinTransforms[i]);
            }
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        public void GetSkinTransforms(Matrix[] skinTransforms)
        {
            if (InverseAbsoluteBindPose == null)
                throw new NotSupportedException(Strings.SkeletonNotSupportSkin);

            if (bones == null || bones.Length < BoneTransforms.Length)
                bones = new Matrix[BoneTransforms.Length];

            CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < InverseAbsoluteBindPose.Count; ++i)
            {
                // Apply inverse bind pose
                Matrix inverseAbsoluteBindPose = InverseAbsoluteBindPose[i];
                Matrix.Multiply(ref inverseAbsoluteBindPose, ref bones[SkeletonRoot + i], out skinTransforms[i]);
            }
        }

        static Matrix[] bones = null;
    }
    #endregion

    #region ModelSkeleton
    /// <summary>
    /// Defines a bone hierarchy used by models.
    /// </summary>
    public class ModelSkeleton : Skeleton
    {
        Matrix[] boneTransforms;
        ReadOnlyCollection<int> parentBones;

        /// <summary>
        /// Gets a fixed sized array of transformation matrices for each bone
        /// according to its parent bone.
        /// </summary>
        public override Matrix[] BoneTransforms { get { return boneTransforms; } }

        /// <summary>
        /// Gets the hierarchical relationship between bones.
        /// </summary>
        public override ReadOnlyCollection<int> ParentBones { get { return parentBones; } }

        /// <summary>
        /// Initializes a new instance of <c>ModelSkeleton</c>.
        /// </summary>
        public ModelSkeleton(Microsoft.Xna.Framework.Graphics.Model model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var skeleton = model.GetSkeletonData();
            if (skeleton != null)
            {
                SkeletonRoot = skeleton.SkeletonRoot;
                InverseAbsoluteBindPose = new ReadOnlyCollection<Matrix>(skeleton.InverseAbsoluteBindPose);
            }

            boneTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(boneTransforms);
            BoneNames = new ReadOnlyCollection<string>(new BoneNameCollection() { Model = model });
            parentBones = new ReadOnlyCollection<int>(new ParentBoneCollection() { Model = model });
        }

        #region Collections
        class BoneNameCollection : IList<string>
        {
            internal Microsoft.Xna.Framework.Graphics.Model Model;

            public int IndexOf(string item)
            {
                for (int i = 0; i < Model.Bones.Count; ++i)
                    if (Model.Bones[i].Name == item)
                        return i;
                return -1;
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
                for (int i = 0; i < Model.Bones.Count; ++i)
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
            internal Microsoft.Xna.Framework.Graphics.Model Model;

            public int IndexOf(int item)
            {
                for (int i = 0; i < Count; ++i)
                    if (this[i] == item)
                        return i;
                return -1;
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
                for (int i = 0; i < Model.Bones.Count; ++i)
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
                for (int i = 0; i < Count; ++i)
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
    [ContentSerializable]
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
                throw new ArgumentException("Error creating skeleton data.");

            InverseAbsoluteBindPose = inverseBindPose;
            SkeletonRoot = skeleton;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        internal ModelSkeletonData() { }
    }
    #endregion
}
