#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
    using Nine.Animations;

    /// <summary>
    /// Defines a point emitter that emit particles from a point in 3D space.
    /// </summary>
    public class PointEmitter : ParticleEmitter
    {
        public Vector3 Position { get; set; }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        private Vector3 direction = Vector3.UnitZ;
        
        public float Spread
        {
            get { return spread; }
            set { spread = value; } 
        }
        private float spread = MathHelper.Pi;

        public override BoundingBox BoundingBox
        {
            get { return new BoundingBox { Min = Position, Max = Position }; }
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            position = Position;
            CreateRandomVelocity(ref velocity, ref direction, spread);
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a box in 3D space.
    /// </summary>
    public class BoxEmitter : ParticleEmitter
    {
        public BoundingBox Box { get; set; }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        private Vector3 direction = Vector3.UnitZ;

        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        public override BoundingBox BoundingBox
        {
            get { return Box; }
        }

        public BoxEmitter()
        {
            Box = new BoundingBox(Vector3.One * -1.0f, Vector3.One * 1.0f);
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            position.X = Box.Min.X + (float)(Random.NextDouble() * (Box.Max.X - Box.Min.X));
            position.Y = Box.Min.Y + (float)(Random.NextDouble() * (Box.Max.Y - Box.Min.Y));
            position.Z = Box.Min.Z + (float)(Random.NextDouble() * (Box.Max.Z - Box.Min.Z));

            CreateRandomVelocity(ref velocity, ref direction, spread);
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a sphere in 3D space.
    /// </summary>
    public class SphereEmitter : ParticleEmitter
    {
        public bool Shell { get; set; }
        public bool Radiate { get; set; }
        public Vector3 Center { get; set; }
        public float Radius { get; set; }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        private Vector3 direction = Vector3.UnitZ;

        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        public SphereEmitter()
        {
            Radius = 100;
        }

        public override BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(new BoundingSphere(Center, Radius)); }
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            CreateRandomVelocity(ref position);

            if (Radiate)
                velocity = position;
            else
                CreateRandomVelocity(ref velocity, ref direction, spread);

            float radius = Radius;
            if (!Shell)
                radius *= (float)Random.NextDouble();

            position *= radius;
            position += Center;
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a cylinder in 3D space.
    /// </summary>
    public class CylinderEmitter : ParticleEmitter
    {
        public bool Shell { get; set; }
        public bool Radiate { get; set; }
        public float Height { get; set; }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        private Vector3 direction = Vector3.UnitZ;

        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        public Vector3 Center { get; set; }
        public Vector3 Up { get; set; }
        public float Radius { get; set; }

        public override BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(new BoundingSphere(Center, Radius)); }
        }

        public CylinderEmitter()
        {
            Up = Vector3.UnitZ;
            Radius = 100.0f;
            Height = 100.0f;
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            double angle = Random.NextDouble() * Math.PI * 2;

            float radius = Radius;
            if (!Shell)
                radius *= (float)Random.NextDouble();

            position.Z = 0;
            position.X = (float)Math.Cos(angle);
            position.Y = (float)Math.Sin(angle);

            bool needTransform = false;
            Matrix transform = new Matrix();
            if (Up != Vector3.UnitZ)
            {
                needTransform = true;
                transform = MatrixHelper.CreateRotation(Vector3.UnitZ, Up);
            }

            if (Radiate)
            {
                if (needTransform)
                    Vector3.TransformNormal(ref position, ref transform, out velocity);
                else
                    velocity = position;
            }

            position.Z = -Height * 0.5f + (float)Random.NextDouble() * Height;
            position.X *= radius;
            position.Y *= radius;

            if (needTransform)
            {
                Vector3.TransformNormal(ref position, ref transform, out position);
            }

            if (!Radiate)
                CreateRandomVelocity(ref velocity, ref direction, spread);
            
            position += Center;
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a list of lines in 3D space.
    /// </summary>
    public class LineEmitter : ParticleEmitter
    {
        private List<Vector3> lineList = new List<Vector3>();
        private List<float> lineWeights = new List<float>();
        private BoundingBox bounds;

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        private Vector3 direction = Vector3.UnitZ;

        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        public IEnumerable<Vector3> LineList
        {
            get { return lineList; }
            set { UpdateLineList(value); }
        }

        private void UpdateLineList(IEnumerable<Vector3> value)
        {
            lineList.Clear();
            lineWeights.Clear();

            lineList.AddRange(value);

            float totalLength = 0;
            for (int i = 0; i < lineList.Count; i += 2)
            {
                totalLength += Vector3.Subtract(lineList[i], lineList[i + 1]).Length();
                lineWeights.Add(totalLength);
            }

            if (totalLength > 0)
            {
                float invLength = 1 / totalLength;
                for (int i = 0; i < lineWeights.Count; i++)
                    lineWeights[i] *= invLength;
            }
            
            bounds = BoundingBox.CreateFromPoints(value);
        }

        public override BoundingBox BoundingBox { get { return bounds; } }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            float percentage = (float)Random.NextDouble();
            for (int i = 0; i < lineWeights.Count; i++)
            {
                if (lineWeights[i] >= percentage)
                {
                    Emit(ref position, ref velocity,
                         lineList[i * 2], lineList[i * 2 + 1], percentage / lineWeights[i]);
                    break;
                }
            }
        }

        private void Emit(ref Vector3 position, ref Vector3 velocity,Vector3 v1, Vector3 v2, float percentage)
        {
            position = Vector3.Lerp(v1, v2, percentage);
            CreateRandomVelocity(ref velocity, ref direction, spread);
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a model bone collection in 3D space.
    /// </summary>
    public class BoneEmitter : ParticleEmitter
    {
        LineEmitter lineEmitter = new LineEmitter();

        public void SetBoneTransforms(Model model, Matrix transform)
        {

        }

        public void SetBoneTransforms(BoneAnimation animation, Matrix transform)
        {

        }

        public override BoundingBox BoundingBox
        {
            get { return lineEmitter.BoundingBox; }
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {

        }
    }
}
