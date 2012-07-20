namespace Nine.Graphics.ObjectModel
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
    public interface IContainedObject
    {
        /// <summary>
        /// Gets the parent container.
        /// </summary>
        object Parent { get; }
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
            var containedObject = target as IContainedObject;
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
                for (int i = 0; i < count; i++)
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

            var targetContainer = targetObject as IContainer;
            if (targetContainer == null)
                return true;
            
            var children = targetContainer.Children;
            if (children != null)
            {
                var count = children.Count;
                for (int i = 0; i < count; i++)
                    if (!Traverse(children[i], result))
                        return false;
            }
            return true;
        }
    }
}