namespace Nine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Defines a generic object container.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IContainer
    {
        /// <summary>
        /// Gets a readonly list of children of this container.
        /// </summary>
        IList Children { get; }
    }

    /// <summary>
    /// Defines an object that can be contained by a parent container.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IComponent
    {
        /// <summary>
        /// Gets or sets the parent container.
        /// </summary>
        object Parent { get; set; }
    }

    /// <summary>
    /// Defines a basic component that can be added to a parent game object.
    /// </summary>
    [ContentSerializable]
    public abstract class Component : Nine.Object, IComponent
    {
        #region Properties
        /// <summary>
        /// Gets the parent of this object.
        /// </summary>
        public Group Parent
        {
            get { return parent; }
        }

        object IComponent.Parent
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
                    OnAdded(parent);
                }
                else
                {
                    if (parent == null)
                        throw new InvalidOperationException("This object does not belongs to the specified container");
                    OnRemoved(parent);
                    parent = null;
                }
            }
        }
        private Group parent;

        /// <summary>
        /// Called when this component is added to a parent group.
        /// </summary>
        protected virtual void OnAdded(Group parent) { }

        /// <summary>
        /// Called when this component is removed from a parent group.
        /// </summary>
        protected virtual void OnRemoved(Group parent) { }
        #endregion
    }

    static class ContainerTraverser
    {
        /// <summary>
        /// Finds the root of the target object in the scene tree.
        /// </summary>
        public static object FindRootContainer(object target)
        {
            object parent = null;
            while ((parent = GetParent(target)) != null)
            {
                target = parent;
            }
            return target;
        }

        /// <summary>
        /// Traverse up the scene tree and returns the first parent of type T.
        /// Returns the target object if it is already an instance of T.
        /// </summary>
        public static T FindParentContainer<T>(object target) where T : class
        {
            if (!(target is T))
            {
                object parent = null;
                while ((parent = GetParent(target)) != null)
                {
                    target = parent;
                    if (target is T)
                        break;
                }
            }
            return target as T;
        }

        private static object GetParent(object target)
        {
            var containedObject = target as IComponent;
            return containedObject != null ? containedObject.Parent : null;
        }

        /// <summary>
        /// Finds all the child objects of the target including the input target.
        /// </summary>
        public static void Traverse<T>(object target, ICollection<T> result) where T : class
        {
            var targetObject = target as T;
            if (targetObject != null)
                result.Add(targetObject);

            var targetContainer = targetObject as IContainer;
            if (targetContainer == null)
                return;

            var children = targetContainer.Children;
            if (children != null)
            {
                var count = children.Count;
                for (int i = 0; i < count; ++i)
                    Traverse(children[i], result);
            }
        }
        
        /// <summary>
        /// Finds all the child objects of the target including the input target.
        /// </summary>
        public static bool Traverse<T>(object target, Func<T, TraverseOptions> result) where T : class
        {
            var traverseOptions = TraverseOptions.Continue;
            var targetObject = target as T;
            if (targetObject != null)
                traverseOptions = result(targetObject);

            if (traverseOptions == TraverseOptions.Stop)
                return false;
            if (traverseOptions == TraverseOptions.Skip)
                return true;

            var targetContainer = target as IContainer;
            if (targetContainer == null)
                return true;
            
            var children = targetContainer.Children;
            if (children != null)
            {
                var count = children.Count;
                for (int i = 0; i < count; ++i)
                    if (!Traverse(children[i], result))
                        return false;
            }
            return true;
        }
    }
}