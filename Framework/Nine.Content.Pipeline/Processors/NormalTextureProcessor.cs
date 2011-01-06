#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// The NormalMapTextureProcessor takes in an encoded normal map, and outputs
    /// a texture in the NormalizedByte4 format.  Every pixel in the source texture
    /// is remapped so that values ranging from 0 to 1 will range from -1 to 1.
    /// </summary>
    [ContentProcessor(DisplayName="Normal Texture Processor - Engine Nine")]
    public class NormalTextureProcessor : TextureProcessor
    {
        public NormalTextureProcessor()
        {
            TextureFormat = TextureProcessorOutputFormat.DxtCompressed;
            PremultiplyAlpha = false;
            GenerateMipmaps = true;
            ColorKeyEnabled = false;
        }
    }
}
