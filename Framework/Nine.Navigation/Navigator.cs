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
    /// Defines the state of a <see cref="Navigator"/>
    /// </summary>
    public enum NavigatorState
    {
        /// <summary>
        /// The navigator has stopped moving.
        /// </summary>
        Stopped,

        /// <summary>
        /// The navigator is moving.
        /// </summary>
        Moving,
    }

    /// <summary>
    /// Represents a basic navigator to simulate game object movement.
    /// </summary>
    public class Navigator : IUpdateable, ISpatialQueryable
    {
        #region Fields
        private Steerable steerable;
        private ArriveBehavior arrive;
        private SeparationBehavior separation;
        private SteerableAvoidanceBehavior steerableAvoidance;
        private WallAvoidanceBehavior wallAvoidance;
        private Queue<Vector3> waypoints;

        private ISpatialQuery<Navigator> myFriends;
        private ISpatialQuery<Navigator> myOpponents;
        private ISpatialQuery<Steerable> friends;
        private ISpatialQuery<Steerable> friendsAndOpponents;
        private ISpatialQuery<LineSegment> walls;
        private ISpatialQuery<BoundingCircle> obstacles;

        private float realHeight;
        private bool holdPosition;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current state of this <see cref="Navigator"/>.
        /// </summary>
        public NavigatorState State { get; private set; }

        /// <summary>
        /// Gets the world transformation matrix this navigator.
        /// </summary>
        public Matrix Transform 
        {
            get 
            {
                return Matrix.CreateFromAxisAngle(Vector3.UnitZ, Rotation - MathHelper.PiOver2) *
                       Matrix.CreateTranslation(Position);
                //return Matrix.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.Atan2(steerable.Forward.Y, steerable.Forward.X)) - MathHelper.PiOver2) *
                //       Matrix.CreateTranslation(Position);
            }
        }

        /// <summary>
        /// Gets or sets the current position of this navigator.
        /// </summary>
        public Vector3 Position 
        {
            get { return new Vector3(steerable.Position, realHeight); }
            set { steerable.Position = new Vector2(value.X, value.Y); realHeight = value.Z; }
        }

        /// <summary>
        /// Gets the forward direction (facing) of this navigator.
        /// </summary>
        public Vector2 Forward
        {
            get { return steerable.Forward; }
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
            get { return steerable.Acceleration; }
            set { steerable.Acceleration = value; }
        }

        /// <summary>
        /// Gets or sets the current speed of this navigator.
        /// </summary>
        public float Speed
        {
            get { return steerable.Speed; }
        }

        /// <summary>
        /// Gets or sets the max speed of this navigator.
        /// </summary>
        public float MaxSpeed
        {
            get { return steerable.MaxSpeed; }
            set { steerable.MaxSpeed = value; }
        }

        /// <summary>
        /// Gets or sets the visual bounding radius of this navigator.
        /// </summary>
        public float SoftBoundingRadius
        {
            get { return steerable.BoundingRadius; }
            set
            {
                steerable.BoundingRadius = value;
                separation.Range = 0;
            }
        }

        /// <summary>
        /// Gets or sets the visual bounding radius of this navigator.
        /// </summary>
        public float HardBoundingRadius { get; set; }

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
        /// Gets or sets whether this navigator should hold its position
        /// when stopped.
        /// </summary>
        public bool HoldPosition
        {
            get { return holdPosition; }
            set { holdPosition = value; separation.Enabled = !HoldPosition; }
        }

        /// <summary>
        /// Gets or sets the ground surface this navigator is moving on.
        /// </summary>
        public ISurface Ground { get; set; }

        /// <summary>
        /// Gets or sets the navigation graph used for path finding.
        /// </summary>
        public IPathGraph PathGraph { get; set; }

        /// <summary>
        /// Gets or sets the interface to query for nearby obstacles.
        /// </summary>
        public ISpatialQuery<BoundingCircle> Obstacles
        {
            get { return obstacles; }
            set { obstacles = value; UpdateFriendsAndOpponents(); }
        }

        /// <summary>
        /// Gets or sets the interface to query for nearby collision walls.
        /// </summary>
        public ISpatialQuery<LineSegment> Walls
        {
            get { return walls; }
            set 
            {
                walls = value;
                wallAvoidance.Walls = value;
                wallAvoidance.Enabled = value != null;
            }
        }

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

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region ISpatialQueryable
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;

        object ISpatialQueryable.SpatialData { get; set; }
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
            steerable = new Steerable();
            steerable.BlendMode = SteeringBehaviorBlendMode.Solo;
            steerable.Behaviors.Add(new StuckAvoidanceBehavior());
            steerable.Behaviors.Add(wallAvoidance = new WallAvoidanceBehavior() { Enabled = false });
            steerable.Behaviors.Add(steerableAvoidance = new SteerableAvoidanceBehavior() { Enabled = false });
            steerable.Behaviors.Add(separation = new SeparationBehavior() { Enabled = false });
            steerable.Behaviors.Add(arrive = new ArriveBehavior() { Enabled = false });
            waypoints = new Queue<Vector3>();
        }

        /// <summary>
        /// Moves this navigator to the specified location.
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            Vector2 target = new Vector2(position.X, position.Y);

            steerable.Target = target;
            arrive.Enabled = true;
            steerableAvoidance.Enabled = true;

            if (!IsMachinery)
            {
                steerable.Forward = Vector2.Normalize(target - steerable.Position);
            }

            State = NavigatorState.Moving;
            OnStarted();
        }

        /// <summary>
        /// Moves this navigator along the specified waypoints.
        /// </summary>
        public void MoveAlong(IEnumerable<Vector3> wayPoints)
        {
            this.waypoints.Clear();

            foreach(Vector3 waypoint in wayPoints)
                waypoints.Enqueue(waypoint);

            if (this.waypoints.Count > 0)
                MoveTo(waypoints.Dequeue());
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
            if (State == NavigatorState.Moving)
            {
                steerableAvoidance.Enabled = false;
                arrive.Enabled = false;
                State = NavigatorState.Stopped;
                OnStopped();
            }
        }

        /// <summary>
        /// Updates the specified game time.
        /// </summary>
        public void Update(TimeSpan gameTime)
        {
            float elapsedTime = (float)gameTime.TotalSeconds;
            if (elapsedTime <= 0)
                return;

            Vector2 currentPosition = steerable.Position;
            steerable.Update(gameTime);

            if (steerable.Speed > 0 && steerable.Force != Vector2.Zero && State == NavigatorState.Stopped)
            {
                State = NavigatorState.Moving;
                OnStarted();
                return;
            }

            if (steerable.Speed <= 0 && steerable.Force == Vector2.Zero)
            {
                if (State == NavigatorState.Moving)
                {
                    if (waypoints.Count > 0)
                        MoveTo(waypoints.Dequeue());
                    else
                        Stop();
                }
                    
                return;
            }

            Vector2 nextPosition = steerable.Position;
            
            // Compute the normal of the terrain. We don't want to our entity
            // to be moving too fast when climbing hills :)
            float height = 0;
            Vector3 normal = Vector3.UnitZ;
            
            // Test to see if we reached the border of the ground.
            if (Ground != null && !Ground.TryGetHeightAndNormal(Position, out height, out normal))
            {
                steerable.Position = currentPosition;
                return;
            }

            // We don't want to make the entity moving too physically
            // Moves the agent towards the target.
            Vector3 facing = new Vector3(steerable.Forward, 0);

            // Imagine we are climbing a hill
            Vector3 right = Vector3.Cross(facing, normal);
            Vector3 direction = Vector3.Cross(normal, right);

            // Adjust player animation speed to avoid sliding artifact
            float increment = Vector3.Dot(direction, facing);
            steerable.Position = Vector2.Lerp(currentPosition, nextPosition, increment);

            realHeight = height;

            UpdateRotation(elapsedTime, facing);
        }

        private void UpdateRotation(float elapsedSeconds, Vector3 facing)
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

        private void UpdateFriendsAndOpponents()
        {
            if (myFriends != null)
            {
                friends = new SpatialQuery<Navigator, Steerable>(myFriends) { Filter = null, Converter = NavigatorToSteerer };

                if (myOpponents != null)
                    friendsAndOpponents = new SpatialQuery<Navigator, Steerable>(myFriends, myOpponents) { Filter = null, Converter = NavigatorToSteerer };
                else
                    friendsAndOpponents = friends;
            }
            else
            {
                friends = null;

                if (myOpponents != null)
                    friendsAndOpponents = new SpatialQuery<Navigator, Steerable>(myOpponents) { Filter = null, Converter = NavigatorToSteerer };
                else
                    friendsAndOpponents = null;
            }

            separation.Neighbors = friends;
            separation.Enabled = friends != null;

            steerableAvoidance.Neighbors = friendsAndOpponents;
            steerableAvoidance.Enabled = friendsAndOpponents != null;
        }

        /// <summary>
        /// Called when this <see cref="Navigator"/> has started moving.
        /// </summary>
        protected virtual void OnStarted()
        {
            separation.Enabled = friends != null;

            if (Started != null)
                Started(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when this <see cref="Navigator"/> has stopped moving.
        /// </summary>
        protected virtual void OnStopped()
        {
            if (Stopped != null)
                Stopped(this, EventArgs.Empty);
        }

        private static Steerable NavigatorToSteerer(Navigator navigator)
        {
            return navigator.steerable;
        }

        private static BoundingCircle NavigatorToBoundingCircle(Navigator navigator)
        {
            return new BoundingCircle(navigator.steerable.Position, navigator.HardBoundingRadius);
        }

        private bool SelfFilter(Navigator navigator)
        {
            return navigator == this;
        }
        #endregion
    }
}