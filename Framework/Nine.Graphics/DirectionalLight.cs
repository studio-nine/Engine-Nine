namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    [ContentSerializable]
    public partial class DirectionalLight : Light, IDeferredLight
    {
        public Vector3 Direction
        {
            get { return AbsoluteTransform.Forward; }
            set { Transform = MatrixHelper.CreateWorld(Vector3.Zero, value); version++; }
        }

        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; version++; }
        }
        private Vector3 specularColor;

        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; version++; }
        }
        private Vector3 diffuseColor;

        /// <summary>
        /// Gets or sets the version of this light. This value increases each
        /// time the color or direction property changed. It can be used to
        /// detect whether this light has changed to a new state very quickly.
        /// </summary>
        public int Version
        {
            get { return version; }
        }
        internal int version;

        private FastList<ISpatialQueryable> shadowCasters;
        private static Vector3[] Corners = new Vector3[BoundingBox.CornerCount];

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionalLight"/> class.
        /// </summary>
        public DirectionalLight(GraphicsDevice graphics) : base(graphics)
        {
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
        }

        /// <summary>
        /// Computes the shadow frustum of this light based on the current
        /// view frustum and objects in the current scene;
        /// </summary>
        protected override void UpdateShadowFrustum(BoundingFrustum viewFrustum, HashSet<ISpatialQueryable> shadowCasters, out Matrix shadowFrustum)
        {
            // 1. Find the bounds of all the shadow casters in the current view frustum
            //    from the respect to the light source.
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Direction, Vector3.Up);
            if (float.IsNaN(view.M11))
                view = Matrix.CreateLookAt(Vector3.Zero, Direction, Vector3.UnitX);

            Vector3 point;
            float nearZ = float.MaxValue;
            float farZ = float.MinValue;

            float left = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MaxValue;
            float top = float.MinValue;

            foreach (var shadowCaster in shadowCasters)
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
                    if (point.X < left)
                        left = point.X;
                    if (point.X > right)
                        right = point.X;
                    if (point.Y < bottom)
                        bottom = point.Y;
                    if (point.Y > top)
                        top = point.Y;
                }
            }

            Matrix projection;
            Matrix.CreateOrthographicOffCenter(left, right, bottom, top, nearZ, farZ, out projection);
            Matrix.Multiply(ref view, ref projection, out shadowFrustum);

            // 2. Now extend the above frustum to include objects that are outside the
            //    view frustum. E.g. a bird might cast a shadow even if it is not visible.
        }

        protected override void OnAdded(DrawingContext context)
        {
            context.directionalLights.Add(this);
        }

        protected override void OnRemoved(DrawingContext context)
        {
            context.directionalLights.Remove(this);
        }

        #region IDeferredLight
        /// <summary>
        /// Gets the light geometry for deferred lighting.
        /// </summary>
        IDrawableObject IDeferredLight.PrepareLightGeometry(DrawingContext context)
        {
            if (deferredGeometry == null)
            {
                deferredGeometry = new FullScreenQuad(context.graphics)
                {
                    Material = deferredMaterial = new DeferredDirectionalLightMaterial(context.graphics)
                };
            }
            deferredMaterial.effect.CurrentTechnique = (specularColor != Vector3.Zero) ? deferredMaterial.effect.Techniques[0] : deferredMaterial.effect.Techniques[1];
            deferredMaterial.effect.ViewProjectionInverse.SetValue(context.matrices.ViewProjectionInverse);
            deferredMaterial.effect.EyePosition.SetValue(context.EyePosition);
            deferredMaterial.effect.Direction.SetValue(Direction);
            deferredMaterial.effect.DiffuseColor.SetValue(diffuseColor);
            deferredMaterial.effect.SpecularColor.SetValue(specularColor);
            deferredGeometry.Visible = Enabled;
            return deferredGeometry;
        }
        private DeferredDirectionalLightMaterial deferredMaterial;
        private FullScreenQuad deferredGeometry;
        #endregion
    }
}