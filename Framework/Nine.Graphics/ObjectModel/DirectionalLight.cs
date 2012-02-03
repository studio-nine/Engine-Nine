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


#endregion

namespace Nine.Graphics.ObjectModel
{
    public partial class DirectionalLight : Light<IDirectionalLight>
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public DirectionalLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
        }
        
        static Vector3[] Corners = new Vector3[BoundingBox.CornerCount];

        protected override bool GetShadowFrustum(GraphicsContext context,
                                                 HashSet<ISpatialQueryable> shadowCastersInLightFrustum,
                                                 HashSet<ISpatialQueryable> shadowCastersInViewFrustum,
                                                 out Matrix frustumMatrix)
        {
            if (shadowCastersInViewFrustum.Count <= 0)
            {
                frustumMatrix = new Matrix();
                return false;
            }

            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Direction, Vector3.UnitZ);
            if (float.IsNaN(view.M11))
                view = Matrix.CreateLookAt(Vector3.Zero, Direction, Vector3.UnitY);

            Vector3 point;
            float nearZ = float.MaxValue;
            float farZ = float.MinValue;

            float left = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MaxValue;
            float top = float.MinValue;

            foreach (var shadowCaster in shadowCastersInViewFrustum)
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

                    left = Math.Min(left, point.X);
                    right = Math.Max(right, point.X);
                    bottom = Math.Min(bottom, point.Y);
                    top = Math.Max(top, point.Y);

                    Corners[i] = point;
                }
            }

            Matrix projection;
            Matrix.CreateOrthographicOffCenter(left, right, bottom, top, nearZ, farZ, out projection);
            Matrix.Multiply(ref view, ref projection, out frustumMatrix);
            return true;
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
    }
}