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
    public class CharactorMovement : IMovable
    {
        private Vector3 force;
        private Vector3 position;
        private Matrix transform;

        public Vector3 Forward { get; set; }

        public float MaxSpeed { get; set; }
        public float MaxForce { get; set; }
        public float Facing { get; set; }
        public float AngularSpeed { get; set; }

        public ISurface Surface { get; set; }
        public float SurfaceSensitivity { get; set; }


        public float Speed
        {
            get { return MaxSpeed; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Matrix Transform
        {
            get { return transform; }
        }


        public CharactorMovement()
        {
            MaxSpeed = 100.0f;
            MaxForce = 100.0f;
            AngularSpeed = MathHelper.Pi;
            Forward = Vector3.UnitX;            
            SurfaceSensitivity = 1.0f;
            force = Vector3.Zero;
            Facing = 0;

            transform = Matrix.Identity;
            transform.Up = Vector3.UnitZ;
            transform.Forward = Vector3.UnitX;
            transform.Right = -Vector3.UnitY;            
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

            force.Normalize();
            
            // Update height from new surface location
            float height = position.Z;
            Vector3 normal = Vector3.UnitZ;

            Position += force * MaxSpeed * elapsedSeconds;

            if (Surface != null)
                Surface.TryGetHeightAndNormal(Position, out height, out normal);

            position.Z = height;

            transform.Up = Vector3.Normalize(
                    Vector3.Lerp(Vector3.UnitZ, normal, SurfaceSensitivity));

            // Update facing
            if (force.X != 0 || force.Y != 0)
            {
                float targetFacing = (float)Math.Atan2(force.Y, force.X);

                // Adjust the facing of the charactor.
                // Smooth entity rotation exponentially
                float rotationOffset = targetFacing - Facing;

                while (rotationOffset > MathHelper.Pi)
                    rotationOffset -= 2 * MathHelper.Pi;
                while (rotationOffset < -MathHelper.Pi)
                    rotationOffset += 2 * MathHelper.Pi;

                if (Math.Abs(rotationOffset) > float.Epsilon)
                {
                    float smoother = (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
                    if (smoother > 1) 
                        smoother = 1;
                    Facing += rotationOffset * smoother;
                }
            }

            Vector3 facing;

            facing.X = (float)Math.Cos(Facing);
            facing.Y = (float)Math.Sin(Facing);


            force = Vector3.Zero;
        }
    }
}