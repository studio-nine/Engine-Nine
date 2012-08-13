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

        private FastList<IDrawableObject> shadowCasters;
        private HashSet<ISpatialQueryable> shadowCasterBounds;
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
        public override void UpdateShadowFrustum(DrawingContext context, ISpatialQuery<IDrawableObject> shadowCasterQuery)
        {
            // 1. Compute the intersection points of the view frustum and the bounding box of the scene
            int length;
            Vector3[] intersections;
            ContainmentType containment;
            BoundingBox sceneBounds = context.BoundingBox;
            sceneBounds.Intersects(context.matrices.ViewFrustum, out containment, out intersections, out length);
            
            if (containment == ContainmentType.Disjoint)
            {
                shadowFrustum.Matrix = Matrix.Identity;
                return;
            }

            if (containment == ContainmentType.Contains)
            {
                length = BoundingFrustum.CornerCount;
                context.matrices.ViewFrustum.GetCorners(intersections);
            }

            // 2. Compute bounds in light space
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Direction, Vector3.Up);
            if (float.IsNaN(view.M11))
                view = Matrix.CreateLookAt(Vector3.Zero, Direction, Vector3.UnitX);

            var point = new Vector3();
            var far = float.MinValue;
            var left = float.MaxValue;
            var right = float.MinValue;
            var bottom = float.MaxValue;
            var top = float.MinValue;

            for (int i = 0; i < length; ++i)
            {
                // Transform the intersection point to light space.
                Vector3.Transform(ref intersections[i], ref view, out point); 
                
                if (point.Z > far)
                    far = point.Z;
                if (point.X < left)
                    left = point.X;
                if (point.X > right)
                    right = point.X;
                if (point.Y < bottom)
                    bottom = point.Y;
                if (point.Y > top)
                    top = point.Y;
            }

            // 3. Extend the near plane to include objects that are not inside the view frustum
            //    but will still have shadows
            Vector3 cross;
            Vector3.Subtract(ref sceneBounds.Max, ref sceneBounds.Min, out cross);
            var near = far - cross.Length();
            
            // 4. Creates the shadow frustum from light space bounds
            Matrix shadowFrustumMatrix;
            Matrix.CreateOrthographicOffCenter(left, right, bottom, top, near, far, out shadowFrustumMatrix);
            Matrix.Multiply(ref view, ref shadowFrustumMatrix, out shadowFrustumMatrix);
            shadowFrustum.Matrix = shadowFrustumMatrix;

            // 5. Query all the shadow caster bounds based on the above rough frustum and continue
            //    trim the above frustum to fit shadow casters as tight as possible.
            if (shadowCasterBounds == null)
            {
                shadowCasters = new FastList<IDrawableObject>();
                shadowCasterBounds = new HashSet<ISpatialQueryable>();
            }
            else
            {
                shadowCasters.Clear();
                shadowCasterBounds.Clear();
            }
            
            shadowCasterQuery.FindAll(shadowFrustum, shadowCasters);
            
            for (int i = 0; i < shadowCasters.Count; ++i)
            {
                // Multiple shadow casters may map to the same bound object, so store them in a hash set.
                var casterBound = ContainerTraverser.FindParentContainer<ISpatialQueryable>(shadowCasters[i]);
                if (casterBound != null)
                    shadowCasterBounds.Add(casterBound);
            }

            foreach (var casterBound in shadowCasterBounds)
            {
                casterBound.BoundingBox.GetCorners(Corners);
                for (int i = 0; i < BoundingBox.CornerCount; ++i)
                {
                    Vector3.Transform(ref Corners[i], ref view, out point);

                    if (point.Z < near)
                        near = point.Z;
                    if (point.Z > far)
                        far = point.Z;

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

            // 6. Recreate the final shadow frustum
            Matrix.CreateOrthographicOffCenter(left, right, bottom, top, near, far, out shadowFrustumMatrix);
            Matrix.Multiply(ref view, ref shadowFrustumMatrix, out shadowFrustumMatrix);
            shadowFrustum.Matrix = shadowFrustumMatrix;
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