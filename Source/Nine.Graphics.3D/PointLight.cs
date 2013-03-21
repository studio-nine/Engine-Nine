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

    [Nine.Serialization.BinarySerializable]
    public partial class PointLight : Light, ISpatialQueryable
    {
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
            if (boundingBoxChanged != null)
                boundingBoxChanged(this, EventArgs.Empty);
        }

        public BoundingSphere BoundingSphere
        {
            get { return new BoundingSphere(Position, Range); }
        }

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        event EventHandler<EventArgs> ISpatialQueryable.BoundingBoxChanged
        {
            add { boundingBoxChanged += value; }
            remove { boundingBoxChanged -= value; }
        }
        private EventHandler<EventArgs> boundingBoxChanged;


        protected override void OnTransformChanged()
        {
            OnBoundingBoxChanged();
        }
        #endregion
        
        public PointLight(GraphicsDevice graphics) : base(graphics)
        {
            range = 10;
            Attenuation = MathHelper.E;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
        }

        internal override void Draw(DrawingContext context, DynamicPrimitive primitive)
        {
            primitive.AddSphere(BoundingSphere, 8, null, Constants.LightFrustumColor, Constants.MiddleLineWidth);
            base.Draw(context, primitive);
        }
    }
    
#if !WINDOWS_PHONE
    partial class PointLight : IDeferredLight
    {
        /// <summary>
        /// Gets the light geometry for deferred lighting.
        /// </summary>
        IDrawableObject IDeferredLight.PrepareLightGeometry(DrawingContext context)
        {
            if (deferredGeometry == null)
            {
                deferredGeometry = new SphereInvert(context.graphics)
                {
                    Material = deferredMaterial = new DeferredPointLightMaterial(context.graphics)
                };
            }
            deferredMaterial.effect.CurrentTechnique = (specularColor != Vector3.Zero) ? deferredMaterial.effect.Techniques[0] : deferredMaterial.effect.Techniques[1];
            deferredMaterial.effect.HalfPixel.SetValue(context.HalfPixel);
            deferredMaterial.effect.ViewProjection.SetValue(context.matrices.ViewProjection);
            deferredMaterial.effect.ViewProjectionInverse.SetValue(context.matrices.ViewProjectionInverse);
            deferredMaterial.effect.EyePosition.SetValue(context.CameraPosition);
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
    }
#endif
}