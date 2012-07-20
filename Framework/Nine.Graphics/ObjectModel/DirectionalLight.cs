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
using Nine.Graphics.Drawing;


#endregion

namespace Nine.Graphics.ObjectModel
{
    [ContentSerializable]
    public partial class DirectionalLight : Light<IDirectionalLight>, ISceneObject
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
        /// Gets the empty directional light.
        /// </summary>
        public static readonly DirectionalLight Empty = new DirectionalLight() { DiffuseColor = Vector3.Zero, SpecularColor = Vector3.Zero, Direction = Vector3.Down };

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionalLight"/> class.
        /// </summary>
        public DirectionalLight()
        {
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
        }
        
        public override bool GetShadowFrustum(BoundingFrustum viewFrustum, IList<IDrawableObject> drawablesInViewFrustum, out Matrix shadowFrustum)
        {
            var shadowCastersInViewFrustum = FindShadowCasters(drawablesInViewFrustum);
            if (shadowCastersInViewFrustum.Count <= 0)
            {
                shadowFrustum = Matrix.Identity;
                return false;
            }

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

            for (int caster = 0; caster < shadowCastersInViewFrustum.Count; caster++)
            {
                shadowCastersInViewFrustum[caster].BoundingBox.GetCorners(Corners);
                for (int i = 0; i < BoundingBox.CornerCount; i++)
                {
                    Vector3.Transform(ref Corners[i], ref view, out point);

                    float z = -point.Z;
                    if (z < nearZ)
                        nearZ = z;
                    if (z > farZ)
                        farZ = z;

                    left = Math.Min(left, point.X);
                    right = Math.Max(right, point.X);
                    bottom = Math.Min(bottom, point.Y);
                    top = Math.Max(top, point.Y);

                    Corners[i] = point;
                }
            }

            Matrix projection;
            Matrix.CreateOrthographicOffCenter(left, right, bottom, top, nearZ, farZ, out projection);
            Matrix.Multiply(ref view, ref projection, out shadowFrustum);
            return true;
        }
        
        private FastList<ISpatialQueryable> FindShadowCasters(IList<IDrawableObject> drawables)
        {
            if (shadowCasters == null)
                shadowCasters = new FastList<ISpatialQueryable>();

            shadowCasters.Clear();
            for (int currentDrawable = 0; currentDrawable < drawables.Count; currentDrawable++)
            {
                var drawable = drawables[currentDrawable];
                var lightable = drawable as ILightable;
                if (lightable != null && lightable.CastShadow)
                {
                    var parentSpatialQueryable = ContainerTraverser.FindParentContainer<ISpatialQueryable>(drawable);
                    if (parentSpatialQueryable != null)
                        shadowCasters.Add(parentSpatialQueryable);
                }
            }
            return shadowCasters;
        }

        protected override void Enable(IDirectionalLight light)
        {
            light.Direction = AbsoluteTransform.Forward;
            light.DiffuseColor = DiffuseColor;
            light.SpecularColor = SpecularColor;
        }

        protected override void Disable(IDirectionalLight light)
        {
            light.DiffuseColor = Vector3.Zero;
            light.SpecularColor = Vector3.Zero;
        }

        protected override void OnAdded(DrawingContext context)
        {
            context.DirectionalLights.Add(this);
        }

        protected override void OnRemoved(DrawingContext context)
        {
            context.DirectionalLights.Remove(this);
        }
    }
}