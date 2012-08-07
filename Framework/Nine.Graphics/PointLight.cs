namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;

    [ContentSerializable]
    public partial class PointLight : Light, ISpatialQueryable, IDeferredLight
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 Position
        {
            get { return AbsoluteTransform.Translation; }
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
            isBoundingBoxDirty = true;
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        public BoundingSphere BoundingSphere
        {
            get { return new BoundingSphere(Position, Range); }
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
        
        public PointLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;

            range = 10;
            Attenuation = MathHelper.E;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
        }

        public override void FindAll(Scene scene, IList<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            var boundingSphere = BoundingSphere;
            //scene.FindAll(ref boundingSphere, result);
        }

        public override void DrawFrustum(DrawingContext context)
        {
            //context.PrimitiveBatch.DrawSphere(BoundingSphere, 8, null, context.Settings.Debug.LightFrustumColor);
        }

        public override bool GetShadowFrustum(BoundingFrustum viewFrustum, IList<IDrawableObject> drawablesInViewFrustum, out Matrix shadowFrustum)
        {
            throw new NotImplementedException();
        }

        #region IDeferredLight
        /// <summary>
        /// Gets the light geometry for deferred lighting.
        /// </summary>
        IDrawableObject IDeferredLight.GetLightGeometry(DrawingContext context)
        {
            if (deferredGeometry == null)
            {
                deferredGeometry = new SphereInvert(context.GraphicsDevice)
                {
                    Material = deferredMaterial = new DeferredPointLightMaterial(context.GraphicsDevice)
                };
            }
            deferredMaterial.effect.CurrentTechnique = (specularColor != Vector3.Zero) ? deferredMaterial.effect.Techniques[0] : deferredMaterial.effect.Techniques[1];
            deferredMaterial.effect.HalfPixel.SetValue(context.HalfPixel);
            deferredMaterial.effect.ViewProjection.SetValue(context.matrices.ViewProjection);
            deferredMaterial.effect.ViewProjectionInverse.SetValue(context.matrices.ViewProjectionInverse);
            deferredMaterial.effect.EyePosition.SetValue(context.EyePosition);
            deferredMaterial.effect.Position.SetValue(Position);
            deferredMaterial.effect.DiffuseColor.SetValue(diffuseColor);
            deferredMaterial.effect.SpecularColor.SetValue(specularColor);
            deferredMaterial.effect.Range.SetValue(range);
            deferredMaterial.effect.Attenuation.SetValue(attenuation);
            deferredGeometry.Visible = Enabled;
            return deferredGeometry;
        }
        private DeferredPointLightMaterial deferredMaterial;
        private SphereInvert deferredGeometry;
        #endregion
    }
}