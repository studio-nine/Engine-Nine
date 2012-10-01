namespace Nine.Graphics.ParticleEffects
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;
    
    /// <summary>
    /// Defines a point emitter that emit particles from a point in 3D space.
    /// </summary>
    public class PointEmitter : ParticleEmitter
    {
        public float Spread
        {
            get { return spread; }
            set { spread = value; } 
        }
        private float spread = MathHelper.Pi;

        protected override void GetBoundingBox(out BoundingBox box)
        {
            box.Min = box.Max = Vector3.Zero;
        }

        public override bool Emit(float lerpAmount, ref Vector3 position, ref Vector3 direction)
        {
            position = Vector3.Zero;
            Randomize(spread, out direction);
            return true;
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a box in 3D space.
    /// </summary>
    public class BoxEmitter : ParticleEmitter
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        public BoxEmitter()
        {
            Min = Vector3.One * -1.0f;
            Max = Vector3.One * 1.0f;
        }

        protected override void GetBoundingBox(out BoundingBox boundingBox)
        {
            boundingBox.Min = Min;
            boundingBox.Max = Max;
        }

        public override bool Emit(float lerpAmount, ref Vector3 position, ref Vector3 direction)
        {
            position.X = Min.X + (float)(Random.NextDouble() * (Max.X - Min.X));
            position.Y = Min.Y + (float)(Random.NextDouble() * (Max.Y - Min.Y));
            position.Z = Min.Z + (float)(Random.NextDouble() * (Max.Z - Min.Z));

            Randomize(spread, out direction);
            return true;
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a sphere in 3D space.
    /// </summary>
    public class SphereEmitter : ParticleEmitter
    {
        public bool Shell { get; set; }
        public bool Radiate { get; set; }
        public float Radius { get; set; }

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

        protected override void GetBoundingBox(out BoundingBox box)
        {
            box.Min = Vector3.One * -Radius;
            box.Max = Vector3.One * Radius;
        }

        public override bool Emit(float lerpAmount, ref Vector3 position, ref Vector3 direction)
        {
            Randomize(out position);

            if (Radiate)
                direction = position;
            else
                Randomize(spread, out direction);

            float radius = Radius;
            if (!Shell)
                radius *= (float)Random.NextDouble();

            position *= radius;
            return true;
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

        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }
        private float spread = MathHelper.Pi;

        public float Radius { get; set; }

        protected override void GetBoundingBox(out BoundingBox boundingBox)
        {
            boundingBox.Min = Vector3.One * -Math.Max(Radius, Height * 0.5f);
            boundingBox.Max = Vector3.One * Math.Max(Radius, Height * 0.5f);
        }

        public CylinderEmitter()
        {
            Radius = 100.0f;
            Height = 100.0f;
        }

        public override bool Emit(float lerpAmount, ref Vector3 position, ref Vector3 direction)
        {
            double angle = Random.NextDouble() * Math.PI * 2;

            float radius = Radius;
            if (!Shell)
                radius *= (float)Random.NextDouble();

            position.Z = (float)Math.Sin(angle);
            position.X = (float)Math.Cos(angle);
            position.Y = 0;

            if (Radiate)
                direction = position;
            else
                Randomize(spread, out direction);

            position.Y = -Height * 0.5f + (float)Random.NextDouble() * Height;
            position.X *= radius;
            position.Z *= radius;
            return true;
        }
    }

    /// <summary>
    /// Defines a point emitter that emit particles from a list of lines in 3D space.
    /// </summary>
    public class LineEmitter : ParticleEmitter
    {
        private Vector3[] lineList;
        private float[] lineWeights;
        private BoundingBox bounds;

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
            var positions = new List<Vector3>();
            var weights = new List<float>();
            positions.AddRange(value);

            float totalLength = 0;
            for (int i = 0; i < positions.Count; i += 2)
            {
                totalLength += Vector3.Subtract(lineList[i], lineList[i + 1]).Length();
                weights.Add(totalLength);
            }

            if (totalLength > 0)
            {
                float invLength = 1 / totalLength;
                for (int i = 0; i < positions.Count; ++i)
                    weights[i] *= invLength;
            }
            
            bounds = BoundingBox.CreateFromPoints(value);

            lineList = positions.ToArray();
            lineWeights = weights.ToArray();
        }

        protected override void GetBoundingBox(out BoundingBox box)
        {
            box = bounds;
        }

        public override bool Emit(float lerpAmount, ref Vector3 position, ref Vector3 direction)
        {
            if (lineWeights == null)
                return false;

            float percentage = (float)Random.NextDouble();
            for (int i = 0; i < lineWeights.Length; ++i)
            {
                if (lineWeights[i] >= percentage)
                {
                    Vector3.Lerp(ref lineList[i * 2], ref lineList[i * 2 + 1], percentage / lineWeights[i], out position);
                    Randomize(spread, out direction);
                    return true;
                }
            }
            return false;
        }
    }
}
