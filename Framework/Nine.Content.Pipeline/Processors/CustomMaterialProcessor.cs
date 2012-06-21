#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Nine.Content.Pipeline.Graphics;
using Nine.Content.Pipeline.Graphics.Materials;
using Nine.Graphics.Materials;
using Nine.Content.Pipeline.Xaml;

#endregion

namespace Nine.Content.Pipeline.Processors
{
    [DefaultContentProcessor]    
    public class CustomMaterialProcessor : ContentProcessor<CustomMaterial, CustomMaterial>
    {
        public override CustomMaterial Process(CustomMaterial input, ContentProcessorContext context)
        {
            if (!string.IsNullOrEmpty(input.Code))
            {
                if (input.Source != null)
                {
                    input.Source = null;
                    context.Logger.LogWarning(null, null, "Replacing custom material shaders");
                }

                var source = context.BuildAsset<EffectContent, CompiledEffectContent>(
                    new EffectContent { EffectCode = input.Code }, "CustomEffectProcessor");
                
                XamlSerializer.SerializationData[new PropertyInstance(input, "Source")] =
                    new ContentReference<CompiledEffectContent>(source.Filename);
            }
            return input;
        }
    }
}
