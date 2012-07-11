#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Nine.Content.Pipeline.Graphics.Materials;
using Nine.Graphics.Materials;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    [DefaultContentProcessor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MaterialGroupProcessor : ContentProcessor<MaterialGroup, MaterialGroup>
    {
        public override MaterialGroup Process(MaterialGroup input, ContentProcessorContext context)
        {
            input.Reference = Build(input, MaterialUsage.Default, context).Filename;
            input.ExtendedMaterials = new Dictionary<MaterialUsage, MaterialGroup>();
            foreach (MaterialUsage usage in Enum.GetValues(typeof(MaterialUsage)))
            {
                if (usage == MaterialUsage.Default)
                    continue;

                var extendedMaterial = (MaterialGroup)input.Clone();
                extendedMaterial.ExtendedMaterials = null;
                extendedMaterial.Reference = Build(extendedMaterial, usage, context).Filename;
                if (extendedMaterial.Reference != null)
                    input.ExtendedMaterials.Add(usage, extendedMaterial);
            }
            return input;
        }

        private static ExternalReference<CompiledEffectContent> Build(MaterialGroup input, MaterialUsage usage, ContentProcessorContext context)
        {
            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
            {
                context.Logger.LogWarning(null, null, Strings.MaterialGroupNotSupported);
                return new ExternalReference<CompiledEffectContent>(Strings.MaterialGroupNotSupported);
            }

            var compiledEffect = MaterialGroupBuilder.Build(input, usage, context);
            if (compiledEffect == null)
                return new ExternalReference<CompiledEffectContent>(null);
            return context.BuildAsset<CompiledEffectContent, CompiledEffectContent>(compiledEffect, null, null,
                   Path.Combine(ContentProcessorContextExtensions.DefaultOutputDirectory, MaterialGroupBuilder.LastIdentity));
        }
    }
}
