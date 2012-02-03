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
using Nine.Graphics.Effects;
using Nine.Graphics.Effects.Deferred;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{
    public partial class SpotLight : IDeferredLight
    {
        SpotLightEffect multiPasseffect;
        public override Effect MultiPassEffect
        {
            get { return multiPasseffect ?? (multiPasseffect = GraphicsResources<SpotLightEffect>.GetInstance(GraphicsDevice)); }
        }

        DeferredSpotLight deferredLight;
        DeferredSpotLight GetDeferredLight()
        {
            return deferredLight ?? (deferredLight = GraphicsResources<DeferredSpotLight>.GetInstance(GraphicsDevice));
        }

        Effect IDeferredLight.Effect
        {
            get
            {
                GetDeferredLight();
                deferredLight.Position = Position;
                deferredLight.Direction = Direction;
                deferredLight.Range = Range;
                deferredLight.Attenuation = Attenuation;
                deferredLight.SpecularColor = SpecularColor;
                deferredLight.DiffuseColor = DiffuseColor;
                deferredLight.Falloff = Falloff;
                deferredLight.InnerAngle = InnerAngle;
                deferredLight.OuterAngle = OuterAngle;
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
    }
}