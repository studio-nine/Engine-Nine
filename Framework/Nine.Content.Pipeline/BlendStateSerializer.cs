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
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Content.Pipeline
{    
    /*
    [ContentTypeSerializer]
    class BlendStateSerializer<T> : ContentTypeSerializer<BlendState>
    {
        protected override BlendState Deserialize(IntermediateReader input, ContentSerializerAttribute format, BlendState existingInstance)
        {
            output.Xml.WriteString(value.Filename);
        }

        protected override void Serialize(IntermediateWriter output, BlendState value, ContentSerializerAttribute format)
        {
            return new ContentReference<T>(input.Xml.ReadString());
        }
    }
     */
}
