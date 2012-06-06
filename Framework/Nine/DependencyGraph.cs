#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
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
    /// Defines a dependency graph provider that provides the topology data for a give array of items.
    /// </summary>
    interface IDependencyProvider<T>
    {
        int GetDependencies(IList<T> elements, int index, int[] dependencies);
    }

    /// <summary>
    /// Contains methods to sort an array of items by topology.
    /// </summary>
    static class DependencyGraph
    {
        static int[] dependencies;
        static bool[] dependencyMatrix;

        /// <summary>
        /// Sorts the specified array into the right topology in place.
        /// </summary>
        public static void Sort<T>(IList<T> elements, int[] reorder, IDependencyProvider<T> dependencyProvider)
        {
            if (dependencyProvider == null)
                throw new ArgumentNullException("dependencyProvider");
            if (reorder == null)
                throw new ArgumentNullException("reorder");
            if (reorder.Length < elements.Count)
                throw new ArgumentException();

            var length = elements.Count;
            if (dependencyMatrix == null || dependencyMatrix.Length < length * length)
                dependencyMatrix = new bool[length * length];
            if (dependencies == null || dependencies.Length < length)
                dependencies = new int[length];

            // Build dependency graph
            Array.Clear(dependencyMatrix, 0, dependencyMatrix.Length);
            Array.Clear(dependencies, 0, dependencies.Length);

            for (int i = 0; i < length; i++)
            {
                var dependencyCount = dependencyProvider.GetDependencies(elements, i, dependencies);
                for (int j = 0; j < dependencyCount; j++)
                {
                    var d = dependencies[j];
                    if (d < 0 || d >= length)
                        throw new InvalidOperationException();
                    dependencyMatrix[d + i * length] = true;
                }
            }

            // Start dependency sorting
            var head = 0;
            while (head < length)
            {
                bool isCyclic = true;

                for (int i = 0; i < length; i++)
                {
                    if (dependencies[i] < 0)
                        continue;

                    // Determine whether this element has any dependencies
                    bool hasDependencies = false;
                    for (int j = 0; j < length; j++)
                    {
                        if (dependencyMatrix[j + i * length])
                        {
                            hasDependencies = true;
                            break;
                        }
                    }

                    if (!hasDependencies)
                    {
                        // Output the element to the front if it does not have any dependencies
                        isCyclic = false;
                        reorder[head++] = i;
                        dependencies[i] = -1;

                        // Remove all the edges that depends on this element
                        for (int k = 0; k < length; k++)
                            dependencyMatrix[i + k * length] = false;
                        break;
                    }
                }

                if (isCyclic)
                    throw new InvalidOperationException("Cyclic dependencies found");
            }
        }
    }
}