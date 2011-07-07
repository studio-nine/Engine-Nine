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
using Nine.Graphics;
using Nine.Graphics.Views;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
using Nine.Graphics.Effects.Deferred;
#endif
#endregion

namespace Nine.Graphics.Views
{
    public partial class DirectionalLightView : IDeferredLight
    {
        Effect effect;
        public override Effect Effect
        {
            get { return effect ?? (effect = new DirectionalLightEffect(GraphicsDevice)); }
        }

        DeferredDirectionalLight deferredLight;
        DeferredDirectionalLight GetDeferredLight()
        {
            return deferredLight ?? (deferredLight = new DeferredDirectionalLight(GraphicsDevice));
        }

        bool IDeferredLight.Contains(Vector3 point)
        {
            return ((IDeferredLight)deferredLight).Contains(point);
        }

        Effect IDeferredLight.Effect
        {
            get { return ((IDeferredLight)deferredLight).Effect; }
        }

        IndexBuffer IDeferredLight.IndexBuffer
        {
            get { return ((IDeferredLight)deferredLight).IndexBuffer; }
        }

        VertexBuffer IDeferredLight.VertexBuffer
        {
            get { return ((IDeferredLight)deferredLight).VertexBuffer; }
        }
    }
}