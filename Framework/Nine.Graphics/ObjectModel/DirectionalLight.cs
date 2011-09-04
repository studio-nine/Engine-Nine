#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics;

#endregion

namespace Nine.Graphics.ObjectModel
{
    public partial class DirectionalLight : Light<IDirectionalLight>
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public override BoundingBox BoundingBox
        {
            get { return BoundingBoxExtensions.Max; }
        }

        public DirectionalLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
        }
        
        protected internal override IEnumerable<ISpatialQueryable> Find(ISpatialQuery<ISpatialQueryable> allObjects, IEnumerable<ISpatialQueryable> objectsInViewFrustum)
        {
            return objectsInViewFrustum;
        }

        static Vector3[] Corners = new Vector3[BoundingBox.CornerCount];

        protected override void GetShadowFrustum(GraphicsContext context,
                                                 IEnumerable<ISpatialQueryable> drawablesInLightFrustum,
                                                 IEnumerable<ISpatialQueryable> drawablesInViewFrustum,
                                                 out Matrix frustumMatrix)
        {
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
            
            foreach (var drawable in drawablesInViewFrustum)
            {
                drawable.BoundingBox.GetCorners(Corners);
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
        }

        protected override void Enable(IDirectionalLight light)
        {
            light.Direction = Transform.Forward;
            light.DiffuseColor = DiffuseColor;
            light.SpecularColor = SpecularColor;
        }

        protected override void Disable(IDirectionalLight light)
        {
            light.DiffuseColor = Vector3.Zero;
            light.SpecularColor = Vector3.Zero;
        }

        public Vector3 Direction { get { return Transform.Forward; } }

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