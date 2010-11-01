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
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    public interface ISpatialEmitter
    {
        BoundingBox BoundingBox { get; }

        Vector3 Emit(GameTime time, float lerpAmount);
    }


    public sealed class PointEmitter : ISpatialEmitter
    {
        private Vector3 previousPosition;

        public Vector3 Position { get; set; }

        /// <summary>
        /// When position change has exceeded this value, PointEmitter will
        /// not interpolate between the old and new positions.
        /// </summary>
        public float MaxLerpDistance { get; set; }

        public BoundingBox BoundingBox 
        {
            get { return new BoundingBox { Min = Position, Max = Position }; }
        }

        public PointEmitter()
        {
            MaxLerpDistance = 5;
            previousPosition = Vector3.UnitZ * float.MaxValue;
        }

        public Vector3 Emit(GameTime time, float lerpAmount)
        {
            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;

            Vector3 result;
            Vector3 increment = Position - previousPosition;

            if (increment.LengthSquared() < MaxLerpDistance * MaxLerpDistance)
            {
                result = Vector3.Lerp(previousPosition, Position, lerpAmount);
            }
            else
            {
                result = Position;
            }

            previousPosition = Position;

            return result;
        }
    }


    public sealed class BoxEmitter : ISpatialEmitter
    {
        static Random random = new Random();

        public BoundingBox Box { get; set; }

        public BoundingBox BoundingBox
        {
            get { return Box; }
        }

        public BoxEmitter()
        {
            Box = new BoundingBox(Vector3.One * -1.0f, Vector3.One * 1.0f);
        }

        public Vector3 Emit(GameTime time, float lerpAmount)
        {
            Vector3 result = new Vector3();

            result.X = Box.Min.X + (float)(random.NextDouble() * (Box.Max.X - Box.Min.X));
            result.Y = Box.Min.Y + (float)(random.NextDouble() * (Box.Max.Y - Box.Min.Y));
            result.Z = Box.Min.Z + (float)(random.NextDouble() * (Box.Max.Z - Box.Min.Z));

            return result;
        }
    }


    public sealed class SphereEmitter : ISpatialEmitter
    {
        static Random random = new Random();

        public BoundingSphere Sphere { get; set; }

        public BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(Sphere); }
        }

        public SphereEmitter()
        {
            Sphere = new BoundingSphere(Vector3.Zero, 1.0f);
        }

        public Vector3 Emit(GameTime time, float lerpAmount)
        {
            Vector3 result = new Vector3();
            
            double r = random.NextDouble() * Sphere.Radius;
            double a = random.NextDouble() * Math.PI * 2;
            double b = random.NextDouble() * Math.PI - MathHelper.PiOver2;
            double rr = Math.Cos(b) * r;

            result.X = (float)(rr * Math.Cos(a));
            result.Y = (float)(rr * Math.Sin(a));
            result.Z = (float)(r * Math.Sin(b));

            return result;
        }
    }


    public sealed class CylinderEmitter : ISpatialEmitter
    {
        static Random random = new Random();

        public Vector3 Center { get; set; }
        public Vector3 Up { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }

        public BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(new BoundingSphere(Center, Radius)); }
        }

        public CylinderEmitter()
        {
            Up = Vector3.UnitZ;
            Radius = 100.0f;
            Height = 100.0f;
        }

        public Vector3 Emit(GameTime time, float lerpAmount)
        {
            throw new NotImplementedException();
        }
    }


    public sealed class RingEmitter : ISpatialEmitter
    {
        static Random random = new Random();

        public Vector3 Center { get; set; }
        public Vector3 Up { get; set; }
        public float Radius { get; set; }

        public BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(new BoundingSphere(Center, Radius)); }
        }

        public RingEmitter()
        {
            Up = Vector3.UnitZ;
            Radius = 100.0f;
        }

        public Vector3 Emit(GameTime time, float lerpAmount)
        {
            Vector3 result = new Vector3();

            double angle = random.NextDouble() * Math.PI * 2;

            result.Z = 0;
            result.X = Radius * (float)Math.Cos(angle);
            result.Y = Radius * (float)Math.Sin(angle);

            if (Up != Vector3.UnitZ)
            {
                float dot = Vector3.Dot(Vector3.UnitZ, Up);

                Matrix transform = Matrix.CreateFromAxisAngle(
                    Vector3.Cross(Vector3.UnitZ, Up), (float)Math.Cos(dot));

                result = Vector3.Transform(result, transform);
            }

            result += Center;

            return result;
        }
    }


    public sealed class BoneEmitter : ISpatialEmitter
    {
        static Random random = new Random();

        public Matrix[] Bones { get; set; }

        public BoundingBox BoundingBox
        {
            get { throw new NotImplementedException(); }
        }

        public Vector3 Emit(GameTime time, float lerpAmount)
        {
            throw new NotImplementedException();
        }
    }
}
