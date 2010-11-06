#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines a octree base model collision detection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ModelCollision : IPickable
    {
        internal ModelCollision() { }

        /// <summary>
        /// Gets the collision tree.
        /// </summary>
        public Octree<object> CollisionTree { get; internal set; }

        /// <summary>
        /// Gets the bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; internal set; }

        public object Pick(Vector3 point)
        {
            throw new NotImplementedException();
        }

        public object Pick(Ray ray, out float? distance)
        {
            throw new NotImplementedException();
        }
    }
}
