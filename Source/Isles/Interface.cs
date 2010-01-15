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
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles
{
    /// <summary>
    /// Represents a peace of surface with Z axis facing up.
    /// </summary>
    public interface ISurface
    {
        /// <summary>
        /// Returns true if the point is on the surface.
        /// </summary>
        bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal);
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


    /// <summary>
    /// Interface for playable animation
    /// </summary>
    public interface IAnimation
    {
        /// <summary>
        /// Gets or sets the playing speed of the animation. 1 is the normal speed.
        /// </summary>
        float Speed { get; set; }

        /// <summary>
        /// Gets the total length of the animation.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Play the animation from start.
        /// </summary>
        void Play();

        /// <summary>
        /// Stop playing the animation.
        /// </summary>
        void Stop();

        /// <summary>
        /// Resume the animation from last stopped point.
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Fired when the animation has completed.
        /// </summary>
        event EventHandler Complete;
    }


    /// <summary>
    /// Interface for game camera
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Gets the camera view matrix
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// Gets the camera projection matrix
        /// </summary>
        Matrix Projection { get; }
    }


    /// <summary>
    /// Interface for transitions
    /// </summary>
    public interface ITransition<T>
    {
        T Update(GameTime time);
    }


    /// <summary>
    /// Interface for object movement
    /// </summary>
    public interface IMovement
    {
        Vector3 Position { get; set; }
        Vector3 Target { get; set; }
        Matrix Transform { get; }

        /// <summary>
        /// Gets the normalized heading of the moving entity.
        /// </summary>
        Vector3 Heading { get; set; }


        void Update(GameTime time);


        event EventHandler TargetReached;
        event EventHandler TargetFailed;
    }
}