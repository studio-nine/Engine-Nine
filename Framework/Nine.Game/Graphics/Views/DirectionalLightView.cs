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
using Nine.Graphics.Views;
#endregion

namespace Nine.Graphics.Views
{
    public partial class DirectionalLightView : LightView<IDirectionalLight>, IDirectionalLight
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public DirectionalLightView(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;

            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.One;
        }

        protected override IEnumerable<IDrawableView> FindLitObjects(ISpatialQuery<IDrawableView> drawables)
        {
            return drawables;
        }

        protected override void Light(IDirectionalLight light)
        {
            light.DiffuseColor = DiffuseColor;
            light.Direction = Direction;
            light.SpecularColor = SpecularColor;
        }

        protected override void Dark(IDirectionalLight light)
        {
            light.DiffuseColor = Vector3.Zero;
            light.SpecularColor = Vector3.Zero;
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor { get; set; }
#if WINDOWS_PHONE
        [ContentSerializer(Optional=true)]
        public Vector3 DiffuseColor { get; set; }        
        [ContentSerializer(Optional=true)]
        public Vector3 Direction { get; set; }
#else
        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return deferredLight.DiffuseColor; }
            set { deferredLight.DiffuseColor = value; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 Direction
        {
            get { return deferredLight.Direction; }
            set { deferredLight.Direction = value; }
        }
#endif
    }
}