#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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
using Isles.Graphics.Models;
#endregion


namespace Isles.Pipeline.Processors
{
    /// <summary>
    /// The NormalMapTextureProcessor takes in an encoded normal map, and outputs
    /// a texture in the NormalizedByte4 format.  Every pixel in the source texture
    /// is remapped so that values ranging from 0 to 1 will range from -1 to 1.
    /// </summary>
    [ContentProcessor(DisplayName="Gray Scale Texture Processor - Isles")]
    public class GrayScaleTextureProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        /// <summary>
        /// Process converts the encoded normals to the NormalizedByte4 format and 
        /// generates mipmaps.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            input.ConvertBitmapType(typeof(PixelBitmapContent<Alpha8>));
            input.GenerateMipmaps(false);

            return input;
        }
    }
}
