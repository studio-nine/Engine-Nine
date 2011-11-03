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
            if (existingInstance == null)
                existingInstance = new BlendState();

            existingInstance.AlphaBlendFunction = (BlendFunction)input.ReadByte();
            existingInstance.AlphaDestinationBlend = (Blend)input.ReadByte();
            existingInstance.AlphaSourceBlend = (Blend)input.ReadByte();
            existingInstance.BlendFactor = input.ReadColor();
            existingInstance.ColorBlendFunction = (BlendFunction)input.ReadByte();
            existingInstance.ColorDestinationBlend = (Blend)input.ReadByte();
            existingInstance.ColorSourceBlend = (Blend)input.ReadByte();
            existingInstance.ColorWriteChannels = (ColorWriteChannels)input.ReadByte();
#if !SILVERLIGHT
            existingInstance.ColorWriteChannels1 = (ColorWriteChannels)input.ReadByte();
            existingInstance.ColorWriteChannels2 = (ColorWriteChannels)input.ReadByte();
            existingInstance.ColorWriteChannels3 = (ColorWriteChannels)input.ReadByte();
#endif
            existingInstance.MultiSampleMask = input.ReadInt32();
            return existingInstance;
        }
    }
}
