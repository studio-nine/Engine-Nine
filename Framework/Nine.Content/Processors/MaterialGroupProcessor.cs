﻿namespace Nine.Serialization.Processors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Graphics.Materials;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MaterialGroupProcessor : ContentProcessor<MaterialGroup, MaterialGroup>
    {
        public override MaterialGroup Process(MaterialGroup input, ContentProcessorContext context)
        {
            input.Reference = Build(input, MaterialUsage.Default, context);
            input.ExtendedMaterials = new Dictionary<MaterialUsage, MaterialGroup>();
            foreach (MaterialUsage usage in Enum.GetValues(typeof(MaterialUsage)))
            {
                if (usage == MaterialUsage.Default)
                    continue;

                var extendedMaterial = input.Clone();
                extendedMaterial.ExtendedMaterials = null;
                extendedMaterial.Reference = Build(extendedMaterial, usage, context);
                if (extendedMaterial.Reference != null)
                    input.ExtendedMaterials[usage] =  extendedMaterial;
            }
            return input;
        }

        private static string Build(MaterialGroup input, MaterialUsage usage, ContentProcessorContext context)
        {
            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
            {
                context.Logger.LogWarning(null, null, "Material group is not supported on Windows Phone.");
                return null;
            }

            var compiledEffect = MaterialGroupBuilder.Build(input, usage, context);
            if (compiledEffect == null)
                return null;

            return context.BuildAsset<CompiledEffectContent, CompiledEffectContent>((CompiledEffectContent)compiledEffect, null, null,
                       Path.Combine(ContentProcessorContextExtensions.DefaultOutputDirectory, MaterialGroupBuilder.LastIdentity)).Filename;
        }
    }
}