#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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
    /// Interface for an object that can be picked by a given ray.
    /// </summary>
    public interface IPickable
    {
        /// <summary>
        /// Gets the object that contains the given point.
        /// </summary>
        /// <param name="distance">Distance to the start of the ray.</param>
        /// <returns>Picked object</returns>
        object Pick(Vector3 point);


        /// <summary>
        /// Gets the nearest intersection point from the specifed picking ray.
        /// </summary>
        /// <param name="distance">Distance to the start of the ray.</param>
        /// <returns>Picked object</returns>
        object Pick(Ray ray, out float? distance);
    }
}