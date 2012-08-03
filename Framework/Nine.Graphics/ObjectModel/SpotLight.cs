namespace Nine.Graphics.ObjectModel
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

        public GraphicsDevice GraphicsDevice { get; private set; }
        
        public Vector3 Position
        {
            get { return AbsoluteTransform.Translation; }
        }

        public Vector3 Direction
        {
            get { return AbsoluteTransform.Forward; }
        }

        public Vector3 SpecularColor { get; set; }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }
        private Vector3 diffuseColor;

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
            set
            {
                outerAngle = value;
                isBoundingFrustumDirty = true;
            }
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

        public override void FindAll(Scene scene, IList<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            var boundingFrustum = BoundingFrustum;
            //scene.FindAll(ref boundingFrustum, result);
        }

        static Vector3[] Corners = new Vector3[BoundingBox.CornerCount];

        public override bool GetShadowFrustum(BoundingFrustum viewFrustum, IList<IDrawableObject> drawablesInViewFrustum, out Matrix shadowFrustum)
        {
            throw new NotImplementedException();
            /*
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
             */
        }

        public override void DrawFrustum(DrawingContext context)
        {
            //context.PrimitiveBatch.DrawFrustum(BoundingFrustum, null, context.Settings.Debug.LightFrustumColor);
        }

        #region IDeferredLight
        /// <summary>
        /// Gets the light geometry for deferred lighting.
        /// </summary>
        IDrawableObject IDeferredLight.GetLightGeometry(DrawingContext context)
        {
            if (deferredGeometry == null)
            {
                deferredGeometry = new CentrumInvert(Context.GraphicsDevice)
                {
                    Material = deferredMaterial = new DeferredSpotLightMaterial(Context.GraphicsDevice)
                };
            }
            deferredMaterial.effect.HalfPixel.SetValue(context.HalfPixel);
            deferredMaterial.effect.World.SetValue(AbsoluteTransform);
            deferredMaterial.effect.ViewProjection.SetValue(context.matrices.ViewProjection);
            deferredMaterial.effect.ViewProjectionInverse.SetValue(context.matrices.ViewProjectionInverse);
            deferredMaterial.effect.EyePosition.SetValue(context.EyePosition);
            deferredMaterial.effect.Position.SetValue(Position);
            deferredMaterial.effect.Direction.SetValue(Direction);
            deferredMaterial.effect.DiffuseColor.SetValue(diffuseColor);
            deferredMaterial.effect.SpecularColor.SetValue(SpecularColor);
            deferredMaterial.effect.Range.SetValue(range);
            deferredMaterial.effect.Attenuation.SetValue(attenuation);
            deferredMaterial.effect.Falloff.SetValue(falloff);
            deferredMaterial.effect.innerAngle.SetValue(innerAngle);
            deferredMaterial.effect.outerAngle.SetValue(outerAngle);
            deferredGeometry.Visible = Enabled;
            return deferredGeometry;
        }
        private DeferredSpotLightMaterial deferredMaterial;
        private CentrumInvert deferredGeometry;
        #endregion
    }
}