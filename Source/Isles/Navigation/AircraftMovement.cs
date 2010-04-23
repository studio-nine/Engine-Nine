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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Navigation
{
    public class AircraftMovement
    {
        private Vector3 force;
        private Vector3 velocity;

        public Vector3 Position { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Acceleration { get; private set; }

        public float MaxForce { get; set; }
        public float MaxSpeed { get; set; }
        public float Mass { get; set; }

        public float Speed
        {
            get { return velocity.Length(); }
        }

        public Matrix Transform
        {
            get
            {
                Matrix transform = Matrix.Identity;

                transform.Translation = Position;
                transform.Forward = Vector3.Normalize(Forward);
                transform.Right = Vector3.Cross(transform.Forward, Up);
                transform.Up = Vector3.Cross(transform.Right, transform.Forward);

                return transform;
            }
        }


        public AircraftMovement()
        {
            Mass = 1.0f;
            MaxForce = 100.0f;
            MaxSpeed = 100.0f;
            Forward = Vector3.UnitX;
            Up = Vector3.UnitZ;
            velocity = Vector3.Zero;
        }


        public void ApplyForce(Vector3 steeringForce)
        {
            force += steeringForce;

            float lengthSq = force.LengthSquared();

            if (lengthSq > MaxForce * MaxForce)
            {
                float inv = (float)(MaxForce / Math.Sqrt(lengthSq));

                force.X *= inv;
                force.Y *= inv;
                force.Z *= inv;
            }
        }

        public void Update(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Acceleration = force / Mass;

            velocity += Acceleration * elapsedSeconds;

            float lengthSq = velocity.LengthSquared();

            if (lengthSq > MaxSpeed * MaxSpeed)
            {
                float inv = (float)(MaxSpeed / Math.Sqrt(lengthSq));

                velocity.X *= inv;
                velocity.Y *= inv;
                velocity.Z *= inv;
            }

            if (lengthSq > 0)
            {
                Forward = Vector3.Normalize(velocity);
            }

            Position += velocity * elapsedSeconds;
                        
            force = Vector3.Zero;
        }
    }
}