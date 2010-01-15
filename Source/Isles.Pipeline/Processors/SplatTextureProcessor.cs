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
    [ContentProcessor(DisplayName="Splat Texture Processor - Isles")]
    public class SplatTextureProcessor : ContentProcessor<SplatTextureContent, TextureContent>
    {
        public override TextureContent Process(SplatTextureContent input, ContentProcessorContext context)
        {
            int width = 0;
            int height = 0;

            Texture2DContent texture = null;

            PixelBitmapContent<float> bitmapR = null;
            PixelBitmapContent<float> bitmapG = null;
            PixelBitmapContent<float> bitmapB = null;
            PixelBitmapContent<float> bitmapA = null;


            if (!string.IsNullOrEmpty(input.LayerA))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input.LayerA), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapA = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapA.Width;
                height = bitmapA.Height;
            }

            if (!string.IsNullOrEmpty(input.LayerR))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input.LayerR), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapR = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapR.Width;
                height = bitmapR.Height;
            }

            if (!string.IsNullOrEmpty(input.LayerG))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input.LayerG), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapG = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapG.Width;
                height = bitmapG.Height;
            }

            if (!string.IsNullOrEmpty(input.LayerB))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input.LayerB), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapB = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapB.Width;
                height = bitmapB.Height;
            }


            PixelBitmapContent<Vector4> bitmap = new PixelBitmapContent<Vector4>(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bitmap.SetPixel(x, y, new Vector4(
                        bitmapR != null ? bitmapR.GetPixel(x, y) : 0,
                        bitmapG != null ? bitmapG.GetPixel(x, y) : 0,
                        bitmapB != null ? bitmapB.GetPixel(x, y) : 0,
                        bitmapA != null ? bitmapA.GetPixel(x, y) : 0));
                }
            }

            Texture2DContent result = new Texture2DContent();

            result.Mipmaps = new MipmapChain(bitmap);
            result.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            
            return result;
        }
    }
}
