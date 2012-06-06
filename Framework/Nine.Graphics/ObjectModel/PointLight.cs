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
    public partial class PointLight : Light<IPointLight>, ISpatialQueryable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

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

        protected internal override void FindAll(Scene scene, IList<IDrawableObject> drawablesInViewFrustum, ICollection<IDrawableObject> result)
        {
            var boundingSphere = BoundingSphere;
            //scene.FindAll(ref boundingSphere, result);
        }

        public override void DrawFrustum(DrawingContext context)
        {
            //context.PrimitiveBatch.DrawSphere(BoundingSphere, 8, null, context.Settings.Debug.LightFrustumColor);
        }

        protected override void Enable(IPointLight light)
        {
            light.Position = AbsoluteTransform.Translation;
            light.DiffuseColor = DiffuseColor;
            light.SpecularColor = SpecularColor;
            light.Attenuation = Attenuation;
            light.Range = Range;            
        }

        protected override void Disable(IPointLight light)
        {
            light.DiffuseColor = Vector3.Zero;
            light.SpecularColor = Vector3.Zero;
        }

        public Vector3 Position { get { return AbsoluteTransform.Translation; } }

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
    }
}