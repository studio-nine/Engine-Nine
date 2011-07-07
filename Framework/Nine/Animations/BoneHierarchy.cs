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
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Animations
{
    /// <summary>
    /// Represents a bone hierarchy that can be animated by <c>BoneAnimation</c>.
    /// </summary>
    public interface IBoneHierarchy
    {
        /// <summary>
        /// Gets a collection of local bone transformation matrices.
        /// </summary>
        ReadOnlyCollection<Matrix> BoneTransforms { get; }

        /// <summary>
        /// Gets the hierarchical relationship between bones.
        /// </summary>
        ReadOnlyCollection<int> ParentBones { get; }

        /// <summary>
        /// Gets the index of the root skeleton in this bone hierarchy.
        /// </summary>
        int SkeletonRoot { get; }

        /// <summary>
        /// Gets a collection of local bone transformation matrices.
        /// </summary>
        ReadOnlyCollection<Matrix> BindPose { get; }
    }
}