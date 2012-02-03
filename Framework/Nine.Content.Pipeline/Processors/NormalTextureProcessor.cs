#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// The NormalMapTextureProcessor takes in an encoded normal map, and outputs
    /// a texture in the NormalizedByte4 format.  Every pixel in the source texture
    /// is remapped so that values ranging from 0 to 1 will range from -1 to 1.
    /// </summary>
    [ContentProcessor(DisplayName="Normal Texture - Engine Nine")]
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
