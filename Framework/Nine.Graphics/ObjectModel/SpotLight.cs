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
    public partial class SpotLight : Light<ISpotLight>
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        #region BoundingBox
        public override BoundingBox BoundingBox
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

        protected override void OnBoundingBoxChanged()
        {
            isBoundingFrustumDirty = true;
            isBoundingBoxDirty = true;
            base.OnBoundingBoxChanged();
        }
        #endregion

        #region BoundingFrustum
        public BoundingFrustum BoundingFrustum
        {
            get
            {
                if (isBoundingFrustumDirty || boundingFrustum == null)
                {
                    const float nearPlane = 0.01f;

                    Matrix projection = new Matrix();
                    Matrix view = Matrix.CreateLookAt(Position, Position + Direction, Vector3.UnitZ);
                    if (float.IsNaN(view.M11))
                        view = Matrix.CreateLookAt(Position, Position + Direction, Vector3.UnitY);
                    Matrix.CreatePerspectiveFieldOfView(outerAngle, 1, nearPlane, Math.Max(nearPlane, Range + nearPlane), out projection);
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
            Range = 10;
            Attenuation = MathHelper.E;
            InnerAngle = MathHelper.PiOver4;
            OuterAngle = MathHelper.PiOver2;
            Falloff = 1;
        }

#if !WINDOWS_PHONE
        protected override void OnTransformChanged()
        {
            isBoundingFrustumDirty = true;
            base.OnTransformChanged();
        }
#endif

        protected internal override IEnumerable<Drawable> FindAffectedDrawables(ISceneManager<Drawable> allDrawables,
                                                                                IEnumerable<Drawable> drawablesInViewFrustum)
        {
            return allDrawables.FindAll(BoundingFrustum);
        }

        public override void DrawFrustum(GraphicsContext context)
        {
            context.PrimitiveBatch.DrawFrustum(BoundingFrustum, null, context.Settings.Debug.LightFrustumColor);
        }

        protected override void Enable(ISpotLight light)
        {
            light.Direction = Transform.Forward;
            light.Position = Transform.Translation;
            light.DiffuseColor = DiffuseColor;
            light.SpecularColor = SpecularColor;
            light.Attenuation = Attenuation;
            light.Range = Range;
            light.InnerAngle = InnerAngle;
            light.OuterAngle = OuterAngle;     
            light.Falloff = Falloff;       
        }

        protected override void Disable(ISpotLight light)
        {
            light.DiffuseColor = Vector3.Zero;
            light.SpecularColor = Vector3.Zero;
        }

        public Vector3 Position { get { return AbsoluteTransform.Translation; } }
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


        [ContentSerializer(Optional = true)]
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


        [ContentSerializer(Optional = true)]
        public float Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; }
        }
        private float attenuation;


        [ContentSerializer(Optional = true)]
        public float InnerAngle
        {
            get { return innerAngle; }
            set { innerAngle = value; }
        }
        private float innerAngle;


        [ContentSerializer(Optional = true)]
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


        [ContentSerializer(Optional = true)]
        public float Falloff
        {
            get { return falloff; }
            set { falloff = value; }
        }
        private float falloff;
    }
}