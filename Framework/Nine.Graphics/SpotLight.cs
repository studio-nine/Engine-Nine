namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.Materials;

    [ContentSerializable]
    public partial class SpotLight : Light, ISpatialQueryable, IDeferredLight
    {
        const float NearPlane = 0.01f;
        
        public Vector3 Position
        {
            get { return AbsoluteTransform.Translation; }
        }

        public Vector3 Direction
        {
            get { return AbsoluteTransform.Forward; }
        }
        
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }
        private Vector3 specularColor;

        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }
        private Vector3 diffuseColor;

        public float Range
        {
            get { return range; }
            set { range = value; frustumTransformNeedsUpdate = true; }
        }
        private float range;

        public float Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; }
        }
        private float attenuation;

        public float InnerAngle
        {
            get { return innerAngle; }
            set { innerAngle = value; }
        }
        private float innerAngle;

        public float OuterAngle
        {
            get { return outerAngle; }
            set { outerAngle = value; frustumTransformNeedsUpdate = true; }
        }
        private float outerAngle;

        public float Falloff
        {
            get { return falloff; }
            set { falloff = value; }
        }
        private float falloff;
        
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
            frustumTransformNeedsUpdate = true;
            OnBoundingBoxChanged();
        }

        private void UpdateFrustumTransform()
        {
            if (frustumTransformNeedsUpdate)
            {
                float radius = (float)Math.Tan(outerAngle * 0.5) * range;
                frustumTransform.M11 = radius; frustumTransform.M12 = 0; frustumTransform.M13 = 0; frustumTransform.M14 = 0;
                frustumTransform.M21 = 0; frustumTransform.M22 = range; frustumTransform.M23 = 0; frustumTransform.M24 = 0;
                frustumTransform.M31 = 0; frustumTransform.M32 = 0; frustumTransform.M33 = radius; frustumTransform.M34 = 0;
                frustumTransform.M41 = 0; frustumTransform.M42 = -range; frustumTransform.M43 = 0; frustumTransform.M44 = 1;

                Matrix temp = MatrixHelper.CreateRotation(Vector3.Down, Direction);
                Matrix.Multiply(ref frustumTransform, ref temp, out frustumTransform);

                var position = AbsoluteTransform.Translation;
                frustumTransform.M41 += position.X;
                frustumTransform.M42 += position.Y;
                frustumTransform.M43 += position.Z;
                frustumTransformNeedsUpdate = false;
            }
        }
        private bool frustumTransformNeedsUpdate = true;
        private Matrix frustumTransform;
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

        public SpotLight(GraphicsDevice graphics) : base(graphics)
        {
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
            range = 10;
            Attenuation = MathHelper.E;
            InnerAngle = MathHelper.PiOver4;
            OuterAngle = MathHelper.PiOver2;
            Falloff = 1;
        }

        public override void FindAll(Scene scene, IList<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            var boundingFrustum = BoundingFrustum;
            //scene.FindAll(ref boundingFrustum, result);
        }

        static Vector3[] Corners = new Vector3[BoundingBox.CornerCount];
        
            /*
        protected override void UpdateShadowFrustum(DrawingContext context, out Matrix shadowFrustum)
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
                for (int i = 0; i < BoundingBox.CornerCount; ++i)
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
           
        }  */

        public override void Draw(DrawingContext context, DynamicPrimitive primitive)
        {
            primitive.AddFrustum(BoundingFrustum, null, Constants.LightFrustumColor);
            base.Draw(context, primitive);
        }

        #region IDeferredLight
        /// <summary>
        /// Gets the light geometry for deferred lighting.
        /// </summary>
        IDrawableObject IDeferredLight.PrepareLightGeometry(DrawingContext context)
        {
            if (deferredGeometry == null)
            {
                deferredGeometry = new CentrumInvert(context.graphics)
                {
                    Material = deferredMaterial = new DeferredSpotLightMaterial(context.graphics)
                };
            }
            UpdateFrustumTransform();
            deferredMaterial.effect.CurrentTechnique = (specularColor != Vector3.Zero) ? deferredMaterial.effect.Techniques[0] : deferredMaterial.effect.Techniques[1];
            deferredMaterial.effect.HalfPixel.SetValue(context.HalfPixel);
            deferredMaterial.effect.World.SetValue(frustumTransform);
            deferredMaterial.effect.ViewProjection.SetValue(context.matrices.ViewProjection);
            deferredMaterial.effect.ViewProjectionInverse.SetValue(context.matrices.ViewProjectionInverse);
            deferredMaterial.effect.EyePosition.SetValue(context.EyePosition);
            deferredMaterial.effect.Position.SetValue(Position);
            deferredMaterial.effect.Direction.SetValue(Direction);
            deferredMaterial.effect.DiffuseColor.SetValue(diffuseColor);
            deferredMaterial.effect.SpecularColor.SetValue(specularColor);
            deferredMaterial.effect.Range.SetValue(range);
            deferredMaterial.effect.Attenuation.SetValue(attenuation);
            deferredMaterial.effect.Falloff.SetValue(falloff);
            deferredMaterial.effect.innerAngle.SetValue((float)Math.Cos(innerAngle * 0.5));
            deferredMaterial.effect.outerAngle.SetValue((float)Math.Cos(outerAngle * 0.5));
            deferredGeometry.Visible = Enabled;
            return deferredGeometry;
        }
        private DeferredSpotLightMaterial deferredMaterial;
        private CentrumInvert deferredGeometry;
        #endregion
    }
}