#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Primitives;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
    public partial class DeferredAmbientLight : IDeferredLight, IAmbientLight
    {
        Quad primitive;

        private void OnCreated()
        {
            AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            primitive = GraphicsResources<Quad>.GetInstance(GraphicsDevice);
        }

        private void OnClone(DeferredAmbientLight cloneSource) { }
        
        private void OnApplyChanges()
        {
            halfPixel = new Vector2(0.5f / GraphicsDevice.Viewport.Width, 0.5f / GraphicsDevice.Viewport.Height);
        }
        
        VertexBuffer IDeferredLight.VertexBuffer
        {
            get { return primitive.VertexBuffer; }
        }

        IndexBuffer IDeferredLight.IndexBuffer
        {
            get { return primitive.IndexBuffer; }
        }

        Effect IDeferredLight.Effect
        {
            get { return this; }
        }
    }
}