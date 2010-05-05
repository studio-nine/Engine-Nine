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
    /// Object that react to game updates.
    /// </summary>
    public interface IUpdateObject
    {
        void Update(GameTime gameTime);
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
    /// Interface for playable animation
    /// </summary>
    public interface IAnimation
    {
        /// <summary>
        /// Gets whether the animation is being played.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        void Play();

        /// <summary>
        /// Stops the animation.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Fired when the animation has completed.
        /// </summary>
        event EventHandler Complete;
    }



    /// <summary>
    /// A container that contains one or more components.
    /// </summary>
    public interface IComponentContainer
    {
        ICollection<object> Components { get; }

        T GetComponent<T>();
        IEnumerable<T> GetComponents<T>();
    }


    /// <summary>
    /// A component that can be added to a container.
    /// </summary>
    public interface IComponent
    {
        IComponentContainer Parent { get; set; }
    }
}