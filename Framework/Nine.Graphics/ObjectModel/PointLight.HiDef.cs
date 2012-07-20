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
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{/*
    public partial class PointLight : IDeferredLight
    {
        PointLightEffect multiPasseffect;
        public override Effect MultiPassEffect
        {
            get { return multiPasseffect ?? (multiPasseffect = GraphicsResources<PointLightEffect>.GetInstance(GraphicsDevice)); }
        }

        DeferredPointLight deferredLight;
        DeferredPointLight GetDeferredLight()
        {
            return deferredLight ?? (deferredLight = GraphicsResources<DeferredPointLight>.GetInstance(GraphicsDevice));
        }

        Effect IDeferredLight.Effect
        {
            get
            {
                GetDeferredLight();
                deferredLight.Position = Position;
                deferredLight.Range = Range;
                deferredLight.Attenuation = Attenuation;
                deferredLight.SpecularColor = SpecularColor;
                deferredLight.DiffuseColor = DiffuseColor;
                return ((IDeferredLight)deferredLight).Effect; 
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