#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Object that react to game updates.
    /// </summary>
    public interface IUpdateObject
    {
        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        void Update(GameTime gameTime);
    }


    /// <summary>
    /// Object that react to game draws.
    /// </summary>
    public interface IDisplayObject
    {
        /// <summary>
        /// Draws the internal state of the object.
        /// </summary>
        void Draw(GameTime gameTime);
    }


    /// <summary>
    /// Interface for a 3D geometry made up of triangles
    /// </summary>
    public interface IGeometry
    {
        /// <summary>
        /// Gets a readonly list of vertex positions.
        /// </summary>
        IList<Vector3> Positions { get; }

        /// <summary>
        /// Gets a readonly list of geometry indices.
        /// </summary>
        IList<ushort> Indices { get; }
    }


    /// <summary>
    /// Defines Spatial relations between objects.
    /// </summary>
    public interface ISpatialQuery
    {
        /// <summary>
        /// Finds the first object intersects with the specified point.
        /// </summary>
        T FindFirst<T>(Vector3 position);

        /// <summary>
        /// Finds the nearest object intersects with the specified ray.
        /// </summary>
        T FindFirst<T>(Ray ray);

        /// <summary>
        /// Finds the nearest object resides within the specified bounding sphere.
        /// </summary>
        T FindFirst<T>(Vector3 position, float radius);

        /// <summary>
        /// Finds all the objects resides within the specified bounding sphere.
        /// </summary>
        IEnumerable<T> Find<T>(Vector3 position, float radius);

        /// <summary>
        /// Finds all the objects that intersects with the specified ray.
        /// </summary>
        IEnumerable<T> Find<T>(Ray ray);

        /// <summary>
        /// Finds all the objects resides within the specified bounding fustum.
        /// </summary>
        IEnumerable<T> Find<T>(BoundingFrustum frustum);
    }
}