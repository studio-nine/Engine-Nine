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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Navigation
{
    /// <summary>
    /// Represents a basic navigator to simulate game object movement.
    /// </summary>
    public class Navigator
    {
        /// <summary>
        /// Gets or sets the current position of this navigator.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets the forward direction (facing) of this navigator.
        /// </summary>
        public Vector3 Forward { get; set; }

        /// <summary>
        /// Gets the current velocity of this navigator.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// Gets or sets the max acceleration of this navigator.
        /// </summary>
        public float Acceleration { get; set; }

        /// <summary>
        /// Gets or sets the current speed of this navigator.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Gets or sets the max speed of this navigator.
        /// </summary>
        public float MaxSpeed { get; set; }

        /// <summary>
        /// Gets the current angular speed of this navigator.
        /// </summary>
        public float AngularSpeed { get; private set; }
        
        /// <summary>
        /// Gets or sets the max angular speed of this navigator.
        /// </summary>
        public float MaxAngularSpeed { get; set; }

        /// <summary>
        /// Gets or sets the height of this navigator above the ground surface.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Gets or sets the visual bounding radius of this navigator.
        /// </summary>
        public float BoundingRadius { get; set; }

        /// <summary>
        /// Gets or sets wether this navigator is used as a machinery.
        /// A machinery always moves towards the <c>Forward</c> direction.
        /// </summary>
        public bool IsMachinery { get; set; }

        /// <summary>
        /// Gets or sets the ground surface this navigator is moving on.
        /// </summary>
        public ISurface Ground { get; set; }

        /// <summary>
        /// Gets or sets the navigation graph used for path finding.
        /// </summary>
        public IGraph<IGraphNode> NavigationGraph { get; set; }

        /// <summary>
        /// Gets or sets the interface to query for nearby obstacles.
        /// </summary>
        public ISpatialQuery Obstacles { get; set; }

        /// <summary>
        /// Gets or sets the interface to query for nearby friends.
        /// </summary>
        public ISpatialQuery Friends { get; set; }

        /// <summary>
        /// Gets or sets the interface to query for nearby neutrals.
        /// </summary>
        public ISpatialQuery Neutrals { get; set; }

        /// <summary>
        /// Gets or sets the interface to query for nearby opponents.
        /// </summary>
        public ISpatialQuery Opponents { get; set; }

        /// <summary>
        /// Moves this navigator to the specified location.
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves this navigator along the specfied waypoints.
        /// </summary>
        public void MoveAlong(IEnumerable<Vector3> wayPoints)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves this navigator towards the specified direction for one frame.
        /// </summary>
        public void Move(Vector3 direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops this navigator from moving.
        /// </summary>
        public void Stop()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Occures when this navigator has started to move.
        /// </summary>
        public event EventHandler<EventArgs> Started;

        /// <summary>
        /// Occures when this navigator has stopped moving when calling <c>Stop</c> 
        /// or when the target is reached or when failed to reach the target.
        /// </summary>
        public event EventHandler<EventArgs> Stopped;
    }
}