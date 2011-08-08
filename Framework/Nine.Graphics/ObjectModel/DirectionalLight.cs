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

        protected internal override IEnumerable<Drawable> FindAffectedDrawables(ISceneManager<Drawable> allDrawables,
                                                                                IEnumerable<Drawable> drawablesInViewFrustum)
        {
            return drawablesInViewFrustum;
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