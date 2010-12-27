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
    using Nine.Navigation.Steering;

    /// <summary>
    /// Represents a basic navigator to simulate game object movement.
    /// </summary>
    public class Navigator : IUpdateObject
    {
        #region Fields
        private NavigatorState state = NavigatorState.Stopped;
        private Steerer steerer;
        private ArriveBehavior arrive;
        private ISpatialQuery<Navigator> myFriends;
        private ISpatialQuery<Navigator> myOpponents;
        private ISpatialQuery<ISteerable> friends;
        private ISpatialQuery<ISteerable> friendsAndOpponents;

        private float realHeight;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the world transformation matrix this navigator.
        /// </summary>
        public Matrix Transform 
        {
            get 
            {
                return Matrix.CreateFromAxisAngle(Vector3.UnitZ, Rotation) *
                       Matrix.CreateTranslation(Position);
            }
        }

        /// <summary>
        /// Gets or sets the current position of this navigator.
        /// </summary>
        public Vector3 Position 
        {
            get { return new Vector3(steerer.Position, realHeight); }
            set { steerer.Position = new Vector2(value.X, value.Y); realHeight = value.Z; }
        }

        /// <summary>
        /// Gets the forward direction (facing) of this navigator.
        /// </summary>
        public Vector2 Forward
        {
            get { return steerer.Forward; }
        }

        /// <summary>
        /// Gets or sets the rotation of this navigator.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the max acceleration of this navigator.
        /// </summary>
        public float Acceleration
        {
            get { return steerer.Acceleration; }
            set { steerer.Acceleration = value; }
        }

        /// <summary>
        /// Gets or sets the current speed of this navigator.
        /// </summary>
        public float Speed
        {
            get { return steerer.Speed; }
        }

        /// <summary>
        /// Gets or sets the max speed of this navigator.
        /// </summary>
        public float MaxSpeed
        {
            get { return steerer.MaxSpeed; }
            set { steerer.MaxSpeed = value; }
        }

        /// <summary>
        /// Gets or sets the visual bounding radius of this navigator.
        /// </summary>
        public float BoundingRadius
        {
            get { return steerer.BoundingRadius; }
            set { steerer.BoundingRadius = value; }
        }

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
        /// Gets or sets whether this navigator is used as a machinery.
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
        public ISpatialQuery<Navigator> Obstacles { get; set; }

        /// <summary>
        /// Gets or sets the interface to query for nearby friends.
        /// Friends will try to make way for each other when possible.
        /// </summary>
        public ISpatialQuery<Navigator> Friends
        {
            get { return myFriends; }
            set { myFriends = value; UpdateFriendsAndOpponents(); }
        }

        /// <summary>
        /// Gets or sets the interface to query for nearby opponents.
        /// </summary>
        public ISpatialQuery<Navigator> Opponents
        {
            get { return myOpponents; }
            set { myOpponents = value; UpdateFriendsAndOpponents(); }
        }

        private void UpdateFriendsAndOpponents()
        {
            if (myFriends != null)
            {
                friends = new SpatialQuery<Navigator, ISteerable>(myFriends) { Converter = SpatialQueryConverter };

                if (myOpponents != null)
                    friendsAndOpponents = new SpatialQuery<Navigator, ISteerable>(myFriends, myOpponents) { Converter = SpatialQueryConverter };
                else
                    friendsAndOpponents = friends;
            }
            else
            {
                friends = null;

                if (myOpponents != null)
                    friendsAndOpponents = new SpatialQuery<Navigator, ISteerable>(myOpponents) { Converter = SpatialQueryConverter };
                else
                    friendsAndOpponents = null;
            }
        }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Occures when this navigator has started to move.
        /// </summary>
        public event EventHandler<EventArgs> Started;

        /// <summary>
        /// Occures when this navigator has stopped moving when calling <c>Stop</c> 
        /// or when the target is reached or when failed to reach the target.
        /// </summary>
        public event EventHandler<EventArgs> Stopped;
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new instance of Navigator.
        /// </summary>
        public Navigator()
        {
            steerer = new Steerer();
            steerer.Behaviors.Add(arrive = new ArriveBehavior() { Enabled = false });
        }

        /// <summary>
        /// Moves this navigator to the specified location.
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            arrive.Enabled = true;
            arrive.Target = new Vector2(position.X, position.Y);

            if (!IsMachinery)
            {
                steerer.Forward = Vector2.Normalize(arrive.Target - steerer.Position);
            }

            state = NavigatorState.Moving;
            OnStarted();
        }

        /// <summary>
        /// Moves this navigator along the specified waypoints.
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

        public void Update(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 currentPosition = steerer.Position;
            steerer.Update(gameTime);
            Vector2 nextPosition = steerer.Position;

            // We don't want to make the entity moving too physically
            // Moves the agent towards the target.
            Vector3 facing = new Vector3(steerer.Forward, 0);

            // Compute the normal of the terrain. We don't want to our entity
            // to be moving too fast when climbing hills :)
            float height = 0;
            Vector3 normal = Vector3.UnitZ;
            
            // Test to see if we reached the border of the ground.
            if (Ground != null && !Ground.TryGetHeightAndNormal(Position, out height, out normal))
            {
                steerer.Position = currentPosition;
                return;
            }

            // Imagine we are climbing a hill
            Vector3 right = Vector3.Cross(facing, normal);
            Vector3 direction = Vector3.Cross(normal, right);

            // Adjust player animation speed to avoid sliding artifact
            float increment = Vector3.Dot(direction, facing);
            steerer.Position = Vector2.Lerp(steerer.Position, nextPosition, increment);

            realHeight = height;

            UpdateRotation(elapsedSeconds, facing);

            if (steerer.Speed <= 0 && state != NavigatorState.Stopped)
            {
                state = NavigatorState.Stopped;
                OnStopped();
            }
        }

        void UpdateRotation(float elapsedSeconds, Vector3 facing)
        {
            // Adjust the facing of the entity.
            // Smooth entity rotation exponentially
            const float PiPi = 2 * MathHelper.Pi;
            float rotationOffset = (float)Math.Atan2(facing.Y, facing.X) - Rotation;

            while (rotationOffset > MathHelper.Pi)
                rotationOffset -= PiPi;
            while (rotationOffset < -MathHelper.Pi)
                rotationOffset += PiPi;

            if (Math.Abs(rotationOffset) > float.Epsilon)
            {
                float smoother = elapsedSeconds * 5;
                if (smoother > 1) smoother = 1;
                Rotation += rotationOffset * smoother;
            }
        }

        protected virtual void OnStarted()
        {
            if (Started != null)
                Started(this, EventArgs.Empty);
        }

        protected virtual void OnStopped()
        {
            if (Stopped != null)
                Stopped(this, EventArgs.Empty);
        }

        private static ISteerable SpatialQueryConverter(Navigator navigator)
        {
            return navigator.steerer;
        }
        #endregion
    }

    internal enum NavigatorState
    {
        Stopped,
        WaitForPath,
        Moving,
    }
}