namespace Nine.Graphics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// Defines an object that can be bound to a specific bone of a model.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    [ContentProperty("Transformable")]
    public class ModelAttachment
    {
        /// <summary>
        /// Gets or sets the transformable object that is bound to the model.
        /// </summary>
        public Transformable Transformable
        {
            get { return transformable; }
            set
            {
                if (transformable != value)
                {
                    Validate(model, value);

                    if (model != null)
                    {
                        if (transformable != null)
                        {
                            model.NotifyRemoved(transformable);
                            model.children.Remove(transformable);
                        }
                        if (value != null)
                        {
                            model.NotifyAdded(value);
                            model.children.Add(value);
                        }
                    }
                    transformable = value;
                }
            }
        }
        private Transformable transformable;

        /// <summary>
        /// Gets or sets the bone name of the target model.
        /// </summary>
        public string Bone
        {
            get { return bone; }
            set { bone = value; boneIndexNeedsUpdate = true; }
        }
        private string bone;
        private bool boneIndexNeedsUpdate = true;
                
        /// <summary>
        /// Gets or sets the bias transformation matrix for the binding.
        /// </summary>
        public Matrix? Transform { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether source object is scaled 
        /// according to the target bone.
        /// </summary>
        public bool UseBoneScale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source model will 
        /// use the target skeleton.
        /// </summary>
        public bool ShareSkeleton { get; set; }

        private int boneIndex = -1;
        private Model model;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelAttachment"/> class.
        /// </summary>
        public ModelAttachment()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelAttachment"/> class.
        /// </summary>
        public ModelAttachment(Transformable source)
        {
            this.Transformable = source;
        }

        internal int GetBoneIndex()
        {
            if (model == null)
                return -1;
            if (boneIndexNeedsUpdate)
            {
                if (string.IsNullOrEmpty(bone))
                {
                    if (transformable != null)
                    {
                        if (ShareSkeleton)
                        {
                            var sourceModel = transformable as Model;
                            if (sourceModel == null)
                                throw new InvalidOperationException("The target object must be a Model when skeleton sharing is turned on.");
                            sourceModel.SharedSkeleton = model.Skeleton;
                        }
                        transformable.Transform = Transform.HasValue ? Transform.Value : Matrix.Identity;
                    }
                    return -1;
                }
                boneIndex = model.Skeleton.GetBone(bone);
                if (boneIndex < 0)
                    throw new InvalidOperationException(string.Format("Target bone {0} not found", bone));
                boneIndexNeedsUpdate = false;
            }
            return boneIndex;
        }

        internal void SetParent(Model model)
        {
            if (this.model != model)
            {
                if (this.model != null && model != null)
                    throw new InvalidOperationException("Cannot attach to multiple models");

                Validate(model, transformable);

                if (transformable != null)
                {
                    if (model != null)
                    {
                        model.NotifyAdded(transformable);                        
                        model.children.Add(transformable);
                    }
                    else
                    {
                        this.model.NotifyRemoved(transformable);
                        this.model.children.Remove(transformable);
                    }
                }

                this.model = model;
            }
        }

        private void Validate(Model model, Transformable transformable)
        {
            if (model != null)
            {
                if (model == transformable)
                    throw new InvalidOperationException("Cannot attach a model to itself");

                if (transformable != null && transformable.Parent != null)
                    throw new InvalidOperationException("Cannot attached an object that is already added to the scene.");
            }

            if (transformable != null)
                ((Nine.IComponent)transformable).Parent = model;
        }
    }

    /// <summary>
    /// Defines an object that is attached to the specific bone of a model.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ModelAttachmentCollection : Collection<ModelAttachment>
    {
        private Model model;        

        internal ModelAttachmentCollection(Model parent)
        {
            model = parent;
        }

        public ModelAttachment Add(Transformable transformable)
        {
            var result = new ModelAttachment(transformable);
            Add(result);
            return result;
        }

        public bool Remove(Transformable transformable)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Transformable == transformable)
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        internal void UpdateTransforms()
        {
            var count = Count;
            for (int i = 0; i < count; ++i)
            {
                var attachment = this[i];
                var transformable = attachment.Transformable;
                if (transformable == null)
                    continue;

                var boneIndex = attachment.GetBoneIndex();
                if (boneIndex >= 0)
                {
                    if (attachment.ShareSkeleton)
                        throw new InvalidOperationException("Cannot specify bone name when skeleton is shared");

                    var boneTransform = model.Skeleton.GetAbsoluteBoneTransform(boneIndex);
                    if (!attachment.UseBoneScale)
                    {
                        Vector3 translation, scale;
                        Quaternion rotation;
                        if (!boneTransform.Decompose(out scale, out rotation, out translation))
                            throw new InvalidOperationException();
                        Matrix.CreateFromQuaternion(ref rotation, out boneTransform);
                        boneTransform.M41 = translation.X;
                        boneTransform.M42 = translation.Y;
                        boneTransform.M43 = translation.Z;
                    }
                    if (attachment.Transform != null)
                        transformable.Transform = attachment.Transform.Value * boneTransform;
                    else
                        transformable.Transform = boneTransform;
                }
            }
        }

        protected override void ClearItems()
        {
            for (int i = 0; i < Count; ++i)
                this[i].SetParent(null);
            base.ClearItems();
        }

        protected override void InsertItem(int index, ModelAttachment item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            item.SetParent(model);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ModelAttachment item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            var old = this[index];
            if (old != item)
            {
                old.SetParent(null);
                item.SetParent(model);
            }
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].SetParent(null);
            base.RemoveItem(index);
        }
    }
}