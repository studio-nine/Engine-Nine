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
    public partial class AmbientLight : Light<IAmbientLight>
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        
        public AmbientLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            AmbientLightColor = Vector3.One * 0.2f;
        }

        public override void DrawShadowMap(GraphicsContext context, ISpatialQuery<IDrawableObject> drawables, IEnumerable<ISpatialQueryable> drawablesInLightFrustum, IEnumerable<ISpatialQueryable> drawablesInViewFrustum)
        {
            throw new NotSupportedException();
        }

        protected override void Enable(IAmbientLight light)
        {
            light.AmbientLightColor = AmbientLightColor;
        }

        protected override void Disable(IAmbientLight light)
        {
            light.AmbientLightColor = Vector3.Zero;
        }

        [ContentSerializer(Optional = true)]
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; }
        }
        private Vector3 ambientLightColor;
    }
}