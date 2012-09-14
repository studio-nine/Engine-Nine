namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Defines a directional light source.
    /// </summary>
    public partial class DirectionalLight : Light
    {
        /// <summary>
        /// Gets or sets a value indicating the start distance of the region that can be
        /// shadowed by this directional light from the camera's perspective.
        /// </summary>
        public float ShadowStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the end distance of the region that can be
        /// shadowed by this directional light from the camera's perspective.
        /// </summary>
        public float ShadowEnd { get; set; }

        /// <summary>
        /// Gets or sets the direction of this light.
        /// </summary>
        public Vector3 Direction
        {
            get { return AbsoluteTransform.Forward; }
            set { Transform = MatrixHelper.CreateWorld(Vector3.Zero, value); }
        }

        /// <summary>
        /// Gets or sets the specular color of this light.
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }
        private Vector3 specularColor;

        /// <summary>
        /// Gets or sets the diffuse color of this light.
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }
        private Vector3 diffuseColor;

        private FastList<IDrawableObject> shadowCasters;
        private HashSet<ISpatialQueryable> shadowCasterBounds;
        private static Vector3[] Corners = new Vector3[BoundingBox.CornerCount];

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionalLight"/> class.
        /// </summary>
        public DirectionalLight(GraphicsDevice graphics) : base(graphics)
        {
            ShadowStart = 0;
            ShadowEnd = float.MaxValue;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
        }

        protected override void OnAdded(DrawingContext context)
        {
            context.directionalLights.Add(this);
        }

        protected override void OnRemoved(DrawingContext context)
        {
            context.directionalLights.Remove(this);
        }
    }

#if !WINDOWS_PHONE
    partial class DirectionalLight : IDeferredLight
    {
        /// <summary>
        /// Computes the shadow frustum of this light based on the current
        /// view frustum and objects in the current scene;
        /// </summary>
        public override void UpdateShadowFrustum(DrawingContext context, ISpatialQuery<IDrawableObject> shadowCasterQuery)
        {
            // Compute the intersection points of the view frustum and the
            // bounding box of the scene
            int length;
            Vector3[] intersections;
            ContainmentType containmentType;

            var sceneBounds = context.BoundingBox;
            var viewFrustum = context.matrices.ViewFrustum;
            
            // Trim view frustum based on shadow start and shadow end.            

            sceneBounds.Intersects(viewFrustum, out intersections, out length);

            // Include the corners of the view frustum and scene bounds since
            // they are not included in the above intersections.
            viewFrustum.GetCorners(Corners);
            for (int i = 0; i < BoundingFrustum.CornerCount; i++)
            {
                sceneBounds.Contains(ref Corners[i], out containmentType);
                if (containmentType == ContainmentType.Contains)
                    intersections[length++] = Corners[i];
            }

            sceneBounds.GetCorners(Corners);
            for (int i = 0; i < BoundingFrustum.CornerCount; i++)
            {
                viewFrustum.Contains(ref Corners[i], out containmentType);
                if (containmentType == ContainmentType.Contains)
                    intersections[length++] = Corners[i];
            }

            // Transform the above intersection points to light space, than
            // find a bounding box in light space.
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

                point.Z = -point.Z;                
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

            // Extend the near plane to include objects that are not inside the
            // current view but will still cast shadows that appears in the view.
            Vector3 cross;
            Vector3.Subtract(ref sceneBounds.Max, ref sceneBounds.Min, out cross);
            var near = far - cross.Length();
            
            Matrix shadowFrustumMatrix;
            Matrix.CreateOrthographicOffCenter(left, right, bottom, top, near, far, out shadowFrustumMatrix);
            Matrix.Multiply(ref view, ref shadowFrustumMatrix, out shadowFrustumMatrix);
            shadowFrustum.Matrix = shadowFrustumMatrix;

            // Query all the shadow caster bounds based on the above rough 
            // frustum and continue trim the above frustum to fit shadow casters
            // as tight as possible.
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
                // Multiple shadow casters may map to the same bound object, 
                // so store them in a hash set.
                var casterBound = ContainerTraverser.FindParentContainer<ISpatialQueryable>(shadowCasters[i]);
                if (casterBound != null)
                    shadowCasterBounds.Add(casterBound);
            }

            near = float.MaxValue;
            far = float.MinValue;
            left = float.MaxValue;
            right = float.MinValue;
            bottom = float.MaxValue;
            top = float.MinValue;

            foreach (var casterBound in shadowCasterBounds)
            {
                casterBound.BoundingBox.GetCorners(Corners);
                for (int i = 0; i < BoundingBox.CornerCount; ++i)
                {
                    Vector3.Transform(ref Corners[i], ref view, out point);

                    point.Z = -point.Z;
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

            // Pad the frustum a little bit so when it remains stable during minor changes.
            var diagonal = new Vector2(right - left, top - bottom);
            var diagonalLength = diagonal.Length();
            var padding = new Vector2((diagonalLength - diagonal.X) * 0.5f, (diagonalLength - diagonal.Y) * 0.5f);
            left -= padding.X;
            right += padding.X;
            top += padding.Y;
            bottom -= padding.Y;
            
            // Move the light in texel-sized increments
            // http://msdn.microsoft.com/en-us/library/windows/desktop/ee416324(v=vs.85).aspx
            var step = diagonalLength / ShadowMap.Size;
            left = (float)Math.Floor(left / step) * step;
            right = (float)Math.Ceiling(right / step) * step;
            bottom = (float)Math.Floor(bottom / step) * step;
            top = (float)Math.Ceiling(top / step) * step;

            // Create the final shadow frustum
            Matrix.CreateOrthographicOffCenter(left, right, bottom, top, near, far, out shadowFrustumMatrix);
            Matrix.Multiply(ref view, ref shadowFrustumMatrix, out shadowFrustumMatrix);
            shadowFrustum.Matrix = shadowFrustumMatrix;
        }

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
            deferredMaterial.effect.EyePosition.SetValue(context.CameraPosition);
            deferredMaterial.effect.Direction.SetValue(Direction);
            deferredMaterial.effect.DiffuseColor.SetValue(diffuseColor);
            deferredMaterial.effect.SpecularColor.SetValue(specularColor);
            deferredGeometry.Visible = Enabled;
            return deferredGeometry;
        }
        private DeferredDirectionalLightMaterial deferredMaterial;
        private FullScreenQuad deferredGeometry;
    }
#endif
}