#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a generic object container.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Gets the number of child objects
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Copies all the child objects to the target array.
        /// </summary>
        void CopyTo(object[] array, int startIndex);
    }

    /// <summary>
    /// Defines an object that can be contained by a parent container.
    /// </summary>
    public interface IContainedObject
    {
        /// <summary>
        /// Gets the parent container.
        /// </summary>
        IContainer Parent { get; }
    }

    static class ContainerTraverser
    {
        // TODO: This array holds a strong reference to the traversed object,
        //       they may never be disposed.
        static object[] containedObjects;
        static int startIndex;

        public static object FindRootContainer(object target)
        {
            object parent = null;
            while ((parent = GetParent(target)) != null)
            {
                target = parent;
            }
            return target;
        }

        private static object GetParent(object target)
        {
            var containedObject = target as IContainedObject;
            return containedObject != null ? containedObject.Parent : target;
        }

        public static void Traverse<T>(object target, ICollection<T> result) where T : class
        {
            if (target == null)
                return;

            var t = target as T;
            if (t != null)
                result.Add(t);

            var targetObject = target as IContainer;
            if (targetObject == null)
                return;

            var childCount = targetObject.Count;
            if (childCount <= 0)
                return;

            if (containedObjects == null)
                containedObjects = new object[childCount];
            else if (containedObjects.Length < childCount + startIndex)
                Array.Resize(ref containedObjects, childCount + startIndex);

            targetObject.CopyTo(containedObjects, startIndex);

            startIndex += childCount;

            for (int i = 0; i < childCount; i++)
            {
                Traverse(containedObjects[i + startIndex - childCount] as IContainer, result);
            }

            startIndex -= childCount;
        }

        public static bool Traverse<T>(object target, Func<T, TraverseOptions> result) where T : class
        {
            if (target == null)
                return true;

            var traverseOptions = TraverseOptions.Continue;
            var t = target as T;
            if (t != null)
                traverseOptions = result(t);

            if (traverseOptions == TraverseOptions.Stop)
                return false;
            if (traverseOptions == TraverseOptions.Skip)
                return true;

            var targetObject = target as IContainer;
            if (targetObject == null)
                return true;

            var childCount = targetObject.Count;
            if (childCount <= 0)
                return true;
            
            if (containedObjects == null)
                containedObjects = new object[childCount];
            else if (containedObjects.Length < childCount + startIndex)
                Array.Resize(ref containedObjects, childCount + startIndex);

            targetObject.CopyTo(containedObjects, startIndex);

            bool continueTraverse = true;
            startIndex += childCount;
            
            for (int i = 0; i < childCount; i++)
            {
                if (!Traverse(containedObjects[i + startIndex - childCount], result))
                {
                    continueTraverse = false;
                    break;
                }
            }

            startIndex -= childCount;
            return continueTraverse;
        }
    }
}