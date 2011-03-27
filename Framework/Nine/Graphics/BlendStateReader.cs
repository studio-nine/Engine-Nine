#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
    class BlendStateReader : ContentTypeReader<BlendState>
    {
        protected override BlendState Read(ContentReader input, BlendState existingInstance)
        {
            BlendState state = new BlendState();
            state.AlphaBlendFunction = (BlendFunction)input.ReadByte();
            state.AlphaDestinationBlend = (Blend)input.ReadByte();
            state.AlphaSourceBlend = (Blend)input.ReadByte();
            state.BlendFactor = input.ReadColor();
            state.ColorBlendFunction = (BlendFunction)input.ReadByte();
            state.ColorDestinationBlend = (Blend)input.ReadByte();
            state.ColorSourceBlend = (Blend)input.ReadByte();
            state.ColorWriteChannels = (ColorWriteChannels)input.ReadByte();
            state.ColorWriteChannels1 = (ColorWriteChannels)input.ReadByte();
            state.ColorWriteChannels2 = (ColorWriteChannels)input.ReadByte();
            state.ColorWriteChannels3 = (ColorWriteChannels)input.ReadByte();
            state.MultiSampleMask = input.ReadInt32();
            return state;
        }
    }
}
