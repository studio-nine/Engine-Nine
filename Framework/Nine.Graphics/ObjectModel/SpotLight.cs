namespace Nine.Graphics.ObjectModel
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public partial class SpotLight : Light<ISpotLight>, ISpatialQueryable
    {
        const float NearPlane = 0.01f;

        public GraphicsDevice GraphicsDevice { get; private set; }

        #region BoundingBox
        public BoundingBox BoundingBox
        {
            get
            {
                if (isBoundingBoxDirty)
                {
                    BoundingSphere sphere = new BoundingSphere(Position, Range);
                    BoundingBox.CreateFromSphere(ref sphere, out boundingBox);
                    isBoundingBoxDirty = false;
                }
                return boundingBox;
            }
        }
        private bool isBoundingBoxDirty;
        private BoundingBox boundingBox;

        private void OnBoundingBoxChanged()
        {
            isBoundingFrustumDirty = true;
            isBoundingBoxDirty = true;
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;

        protected override void OnTransformChanged()
        {
            OnBoundingBoxChanged();
        }
        #endregion

        #region BoundingFrustum
        public BoundingFrustum BoundingFrustum
        {
            get
            {
                if (isBoundingFrustumDirty || boundingFrustum == null)
                {
                    Matrix projection = new Matrix();
                    Matrix view = Matrix.CreateLookAt(Position, Position + Direction, Vector3.Up);
                    if (float.IsNaN(view.M11))
                        view = Matrix.CreateLookAt(Position, Position + Direction, Vector3.UnitX);
                    Matrix.CreatePerspectiveFieldOfView(outerAngle, 1, NearPlane, Math.Max(NearPlane * 2, Range), out projection);
                    Matrix.Multiply(ref view, ref projection, out projection);
                    if (boundingFrustum == null)
                        boundingFrustum = new BoundingFrustum(projection);
                    else boundingFrustum.Matrix = projection;

                    isBoundingFrustumDirty = false;
                }
                return boundingFrustum;
            }
        }
        private bool isBoundingFrustumDirty = true;
        private BoundingFrustum boundingFrustum;
        #endregion

        public SpotLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
            range = 10;
            Attenuation = MathHelper.E;
            InnerAngle = MathHelper.PiOver4;
            OuterAngle = MathHelper.PiOver2;
            Falloff = 1;
        }

        protected internal override void FindAll(Scene scene, IList<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            var boundingFrustum = BoundingFrustum;
            //scene.FindAll(ref boundingFrustum, result);
        }

        static Vector3[] Corners = new Vector3[BoundingBox.CornerCount];

        protected override bool GetShadowFrustum(DrawingContext context,
                                                HashSet<ISpatialQueryable> shadowCastersInLightFrustum,
                                                HashSet<ISpatialQueryable> shadowCastersInViewFrustum,
                                                out Matrix frustumMatrix)
        {
            if (shadowCastersInLightFrustum.Count <= 0)
            {
                frustumMatrix = new Matrix();
                return false;
            }

            Matrix view = Matrix.CreateLookAt(Position, Position + Direction, Vector3.Up);
            if (float.IsNaN(view.M11))
                view = Matrix.CreateLookAt(Position, Position + Direction, Vector3.UnitX);

            Vector3 point;
            float nearZ = float.MaxValue;
            float farZ = float.MinValue;

            double left = double.MaxValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;
            double top = double.MinValue;

            foreach (var shadowCaster in shadowCastersInLightFrustum)
            {
                shadowCaster.BoundingBox.GetCorners(Corners);
                for (int i = 0; i < BoundingBox.CornerCount; i++)
                {
                    Vector3.Transform(ref Corners[i], ref view, out point);

                    float z = -point.Z;
                    if (z < nearZ)
                        nearZ = z;
                    if (z > farZ)
                        farZ = z;
                    
                    left = Math.Min(left, Math.Atan2(point.X, z));
                    right = Math.Max(right, Math.Atan2(point.X, z));
                    bottom = Math.Min(bottom, Math.Atan2(point.Y, z));
                    top = Math.Max(top, Math.Atan2(point.Y, z));

                    Corners[i] = point;
                }
            }

            double max = outerAngle * 0.5;
            if (left < -max)
                left = -max;
            if (right > max)
                right = max;
            if (bottom < -max)
                bottom = -max;
            if (top > max)
                top = max;

            Matrix projection;
            Matrix.CreatePerspectiveOffCenter((float)Math.Tan(left) * nearZ,
                                              (float)Math.Tan(right) * nearZ,
                                              (float)Math.Tan(bottom) * nearZ,
                                              (float)Math.Tan(top) * nearZ,
                                                     Math.Max(NearPlane, nearZ),
                                                     Math.Max(NearPlane * 2, farZ), out projection);
            Matrix.Multiply(ref view, ref projection, out frustumMatrix);
            return true;
        }

        public override void DrawFrustum(DrawingContext context)
        {
            //context.PrimitiveBatch.DrawFrustum(BoundingFrustum, null, context.Settings.Debug.LightFrustumColor);
        }

        protected override void Enable(ISpotLight light)
        {
            light.Direction = AbsoluteTransform.Forward;
            light.Position = AbsoluteTransform.Translation;
            light.DiffuseColor = DiffuseColor;
            light.SpecularColor = SpecularColor;
            light.Attenuation = Attenuation;
            light.Range = Range;
            light.InnerAngle = InnerAngle;
            light.OuterAngle = OuterAngle;     
            light.Falloff = Falloff;       
        }

        protected override void Disable(ISpotLight light)
        {
            light.DiffuseColor = Vector3.Zero;
            light.SpecularColor = Vector3.Zero;
        }

        public Vector3 Position { get { return AbsoluteTransform.Translation; } }
        public Vector3 Direction { get { return AbsoluteTransform.Forward; } }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }
        private Vector3 diffuseColor;


        [ContentSerializer(Optional = true)]
        public float Range
        {
            get { return range; }
            set
            {
                range = value;
                OnBoundingBoxChanged();
            }
        }
        private float range;


        [ContentSerializer(Optional = true)]
        public float Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; }
        }
        private float attenuation;


        [ContentSerializer(Optional = true)]
        public float InnerAngle
        {
            get { return innerAngle; }
            set { innerAngle = value; }
        }
        private float innerAngle;


        [ContentSerializer(Optional = true)]
        public float OuterAngle
        {
            get { return outerAngle; }
            set
            {
                outerAngle = value;
                isBoundingFrustumDirty = true;
            }
        }
        private float outerAngle;


        [ContentSerializer(Optional = true)]
        public float Falloff
        {
            get { return falloff; }
            set { falloff = value; }
        }
        private float falloff;
    }
}