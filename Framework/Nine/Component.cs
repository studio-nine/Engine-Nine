namespace Nine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Defines an object that can be contained by a parent container.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// Gets or sets the parent container.
        /// </summary>
        IContainer Parent { get; set; }
    }

    /// <summary>
    /// Defines a basic component that can be added to a parent game object.
    /// </summary>
    [ContentSerializable]
    public abstract class Component : Nine.Object, IUpdateable, IComponent
    {
        #region Properties
        /// <summary>
        /// Gets the parent of this object.
        /// </summary>
        public Group Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets the root scene that contains this object.
        /// </summary>
        public Scene Scene
        {
            get { return scene; }
        }

        IContainer IComponent.Parent
        {
            get { return Parent; }
            set { SetParent(value); }
        }

        private void SetParent(object value)
        {
            if (parent != value)
            {
                if (value != null)
                {
                    if (parent != null)
                        throw new InvalidOperationException("This object already belongs to a container");
                    var group = value as Group;
                    if (group == null)
                        throw new InvalidOperationException("This object can only be attached to a Group");
                    parent = group;
                    scene = parent.FindRoot<Scene>();
                    wasAdded = true;
                }
                else
                {
                    if (parent == null)
                        throw new InvalidOperationException("This object does not belongs to the specified container");
                    if (wasAdded)
                        OnAdded(parent);
                    OnRemoved(parent);
                    parent = null;
                    scene = null;
                }
            }
        }
        private Group parent;
        private Scene scene;
        private bool wasAdded;

        void IUpdateable.Update(float elapsedTime)
        {
            if (parent != null)
            {
                if (wasAdded)
                {
                    OnAdded(parent);
                    wasAdded = false;
                }
                Update(elapsedTime);
            }
        }
        #endregion

        /// <summary>
        /// Called when this component is added to a parent group.
        /// </summary>
        protected virtual void OnAdded(Group parent) { }

        /// <summary>
        /// Called when this component is removed from a parent group.
        /// </summary>
        protected virtual void OnRemoved(Group parent) { }

        /// <summary>
        /// Updates the internal state of this component.
        /// </summary>
        protected virtual void Update(float elapsedTime) { }
    }
}