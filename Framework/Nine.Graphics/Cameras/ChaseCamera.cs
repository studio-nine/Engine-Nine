#region File Description
//-----------------------------------------------------------------------------
// ChaseCamera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Represents a simple chase camera with spring physics.
    /// </summary>
    public class ChaseCamera : Camera, IUpdateable
    {
        #region Properties
        /// <summary>
        /// Position of object being chased.
        /// </summary>
        public Transformable Target
        {
            get { return target; }
            set
            {
                if (target != value)
                {
                    target = value;
                    Reset();
                }
            }
        }
        private Transformable target;

        /// <summary>
        /// Desired camera position in the chased object's coordinate system.
        /// </summary>
        public Vector3 PositionOffset
        {
            get { return positionOffset; }
            set { positionOffset = value; }
        }
        private Vector3 positionOffset = new Vector3(0, 20, 100);
        private Vector3 desiredPosition;

        /// <summary>
        /// Look at point in the chased object's coordinate system.
        /// </summary>
        public Vector3 LookAtOffset
        {
            get { return lookAtOffset; }
            set { lookAtOffset = value; }
        }
        private Vector3 lookAtOffset;
        private Vector3 lookAt;

        /// <summary>
        /// Physics coefficient which controls the influence of the camera's position
        /// over the spring force. The stiffer the spring, the closer it will stay to
        /// the chased object.
        /// </summary>
        public float Stiffness
        {
            get { return stiffness; }
            set { stiffness = value; }
        }
        private float stiffness = 1800.0f;

        /// <summary>
        /// Physics coefficient which approximates internal friction of the spring.
        /// Sufficient damping will prevent the spring from oscillating infinitely.
        /// </summary>
        public float Damping
        {
            get { return damping; }
            set { damping = value; }
        }
        private float damping = 600.0f;

        /// <summary>
        /// Mass of the camera body. Heaver objects require stiffer springs with less
        /// damping to move at the same rate as lighter objects.
        /// </summary>
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }
        private float mass = 50.0f;

        /// <summary>
        /// Position of camera in world space.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
        }
        private Vector3 position;

        /// <summary>
        /// Velocity of camera.
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
        }
        private Vector3 velocity;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="ChaseCamera"/> class.
        /// </summary>
        public ChaseCamera(GraphicsDevice graphics)
            : base(graphics)
        {
            Reset();
        }

        /// <summary>
        /// Rebuilds object space values in world space. Invoke before publicly
        /// returning or privately accessing world space values.
        /// </summary>
        private void UpdateWorldPositions()
        {
            if (target == null)
                return;

            // Construct a matrix to transform from object space to world space
            var targetTransform = target.AbsoluteTransform;
            var chaseDirection = targetTransform.Forward;
            var chasePosition = targetTransform.Translation;

            var transform = Matrix.Identity;
            transform.Forward = chaseDirection;
            transform.Up = Vector3.Up;
            transform.Right = Vector3.Cross(Vector3.Up, chaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = chasePosition + Vector3.TransformNormal(PositionOffset, transform);
            lookAt = chasePosition + Vector3.TransformNormal(LookAtOffset, transform);
        }

        /// <summary>
        /// Rebuilds camera's view and projection matricies.
        /// </summary>
        private void UpdateMatrices()
        {
            Transform = Matrix.CreateWorld(position, lookAt - position, Vector3.Up);
        }

        /// <summary>
        /// Forces camera to be at desired position and to stop moving. The is useful
        /// when the chased object is first created or after it has been teleported.
        /// Failing to call this after a large change to the chased object's position
        /// will result in the camera quickly flying across the world.
        /// </summary>
        public void Reset()
        {
            UpdateWorldPositions();

            // Stop motion
            velocity = Vector3.Zero;

            // Force desired position
            position = desiredPosition;

            UpdateMatrices();
        }

        /// <summary>
        /// Animates the camera from its current position towards the desired offset
        /// behind the chased object. The camera's animation is controlled by a simple
        /// physical spring attached to the camera and anchored to the desired position.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            UpdateWorldPositions();

            float elapsed = (float)elapsedTime.TotalSeconds;

            // Calculate spring force
            Vector3 stretch = position - desiredPosition;
            Vector3 force = -stiffness * stretch - damping * velocity;

            // Apply acceleration
            Vector3 acceleration = force / mass;
            velocity += acceleration * elapsed;

            // Apply velocity
            position += velocity * elapsed;

            UpdateMatrices();
        }

        #endregion
    }
}
