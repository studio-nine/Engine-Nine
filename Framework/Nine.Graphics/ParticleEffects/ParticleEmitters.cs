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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        [ContentSerializer(Optional = true)]
        public float Spread
        {
            get { return spread; }
            set { spread = value; } 
        }
        private float spread = MathHelper.Pi;        

        protected override BoundingBox BoundingBoxValue
        {
            get { return new BoundingBox(); }
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            position = Vector3.Zero;
            CreateRandomVector(ref velocity, spread);
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a box in 3D space.
    /// </summary>
    public class BoxEmitter : ParticleEmitter
    {
        [ContentSerializer(Optional = true)]
        public Matrix? Transform 
        {
            get
            {
                if (hasTransform)
                    return transform;
                return null;
            }
            set
            {
                if ((hasTransform = (value != null)))
                {
                    transform = value.Value;
                }
            }
        }
        private bool hasTransform;
        private Matrix transform;

        [ContentSerializer(Optional = true)]
        public BoundingBox Box { get; set; }

        [ContentSerializer(Optional = true)]
        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        protected override BoundingBox BoundingBoxValue
        {
            get 
            {
                if (!hasTransform)
                    return Box;
                
                Box.GetCorners(corners);
                Vector3.Transform(corners, ref transform, corners);
                return BoundingBox.CreateFromPoints(corners);
            }
        }
        static Vector3[] corners = new Vector3[BoundingBox.CornerCount];

        public BoxEmitter()
        {
            Box = new BoundingBox(Vector3.One * -1.0f, Vector3.One * 1.0f);
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            position.X = Box.Min.X + (float)(Random.NextDouble() * (Box.Max.X - Box.Min.X));
            position.Y = Box.Min.Y + (float)(Random.NextDouble() * (Box.Max.Y - Box.Min.Y));
            position.Z = Box.Min.Z + (float)(Random.NextDouble() * (Box.Max.Z - Box.Min.Z));

            if (Transform != null)
            {
                Vector3.Transform(ref position, ref transform, out position);
            }

            CreateRandomVector(ref velocity, spread);
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a sphere in 3D space.
    /// </summary>
    public class SphereEmitter : ParticleEmitter
    {
        [ContentSerializer(Optional = true)]
        public bool Shell { get; set; }
        [ContentSerializer(Optional = true)]
        public bool Radiate { get; set; }
        [ContentSerializer(Optional = true)]
        public float Radius { get; set; }

        [ContentSerializer(Optional = true)]
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

        protected override BoundingBox BoundingBoxValue
        {
            get { return new BoundingBox(Vector3.One * -Radius, Vector3.One * Radius); }
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {
            CreateRandomVector(ref position);

            if (Radiate)
                velocity = position;
            else
                CreateRandomVector(ref velocity, spread);

            float radius = Radius;
            if (!Shell)
                radius *= (float)Random.NextDouble();

            position *= radius;
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a cylinder in 3D space.
    /// </summary>
    public class CylinderEmitter : ParticleEmitter
    {
        [ContentSerializer(Optional = true)]
        public bool Shell { get; set; }
        [ContentSerializer(Optional = true)]
        public bool Radiate { get; set; }
        [ContentSerializer(Optional = true)]
        public float Height { get; set; }

        [ContentSerializer(Optional = true)]
        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        [ContentSerializer(Optional = true)]
        public Vector3 Up { get; set; }
        [ContentSerializer(Optional = true)]
        public float Radius { get; set; }

        protected override BoundingBox BoundingBoxValue
        {
            get { return new BoundingBox(Vector3.One * -Math.Max(Radius, Height * 0.5f), 
                                         Vector3.One * Math.Max(Radius, Height * 0.5f)); }
        }

        public CylinderEmitter()
        {
            Up = Vector3.Up;
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
            if (Up != Vector3.Up)
            {
                needTransform = true;
                transform = MatrixHelper.CreateRotation(Vector3.Up, Up);
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
                CreateRandomVector(ref velocity, spread);
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

        [ContentSerializer(Optional = true)]
        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        [ContentSerializer(Optional = true)]
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

        protected override BoundingBox BoundingBoxValue { get { return bounds; } }

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
            CreateRandomVector(ref velocity, spread);
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
            throw new NotImplementedException();
        }

        public void SetBoneTransforms(BoneAnimation animation, Matrix transform)
        {
            throw new NotImplementedException();
        }

        protected override BoundingBox BoundingBoxValue
        {
            get { return lineEmitter.BoundingBox; }
        }

        public override void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity)
        {

        }
    }
}
