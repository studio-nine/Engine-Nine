#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Graphics;


#if !WINDOWS_PHONE
using Nine.Graphics.Materials;
using Nine.Graphics.Materials.Deferred;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{/*
    public partial class DirectionalLight : IDeferredLight
    {
        DirectionalLightEffect multiPasseffect;
        public override Effect MultiPassEffect
        {
            get { return multiPasseffect ?? (multiPasseffect = GraphicsResources<DirectionalLightEffect>.GetInstance(GraphicsDevice)); }
        }

        DeferredDirectionalLight deferredLight;
        DeferredDirectionalLight GetDeferredLight()
        {
            return deferredLight ?? (deferredLight = GraphicsResources<DeferredDirectionalLight>.GetInstance(GraphicsDevice));
        }

        Effect IDeferredLight.Effect
        {
            get
            {
                GetDeferredLight();
                deferredLight.Direction = Direction;
                deferredLight.SpecularColor = SpecularColor;
                deferredLight.DiffuseColor = DiffuseColor;
                return ((IDeferredLight)GetDeferredLight()).Effect;
            }
        }

        IndexBuffer IDeferredLight.IndexBuffer
        {
            get { return ((IDeferredLight)GetDeferredLight()).IndexBuffer; }
        }

        VertexBuffer IDeferredLight.VertexBuffer
        {
            get { return ((IDeferredLight)GetDeferredLight()).VertexBuffer; }
        }
    }*/
}