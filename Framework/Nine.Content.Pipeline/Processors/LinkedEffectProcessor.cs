#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Content.Pipeline.Graphics.Effects;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    using StitchUp.Content.Pipeline;
    using StitchUp.Content.Pipeline.Graphics;
    using StitchUp.Content.Pipeline.Processors;
    using StitchUp.Content.Pipeline.FragmentLinking;
    using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;

    /// <summary>
    /// Processes the input LinkedEffectContent.
    /// </summary>
    [ContentProcessor(DisplayName = "Linked Effect Processor - Engine Nine")]
    public class LinkedEffectProcessor : ContentProcessor<LinkedEffectContent, LinkedEffectContent>
    {
        public override LinkedEffectContent Process(LinkedEffectContent input, ContentProcessorContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
                return input;

            StringBuilder content = new StringBuilder();

            foreach (LinkedEffectPartContent part in input.EffectParts)
            {
                part.EffectParts = input.EffectParts;
                part.Validate(context);

                string filename = Path.Combine(context.IntermediateDirectory, Path.GetRandomFileName() + ".Fragment");
                content.AppendLine(filename);
                File.WriteAllText(filename, part.Code);
            }

            string resultFile = Path.Combine(context.IntermediateDirectory, "LinkedEffect.stitchedeffect");
            File.WriteAllText(resultFile, content.ToString());

            StitchedEffectImporter importer = new StitchedEffectImporter();
            StitchedEffectContent stitchedContent = importer.Import(resultFile, new LinkedEffectContentImporterContext(context));

            StitchedEffectProcessor processor = new StitchedEffectProcessor();
            CompiledEffectContent compiledEffect = processor.Process(stitchedContent, context);

            List<string> uniqueNames = new List<string>();
            input.UniqueNames = processor.Symbols.StitchedFragments.Select<StitchedFragmentSymbol, string>(symbol => symbol.UniqueName + "_").ToArray<string>();
            input.EffectCode = compiledEffect.GetEffectCode();
            return input;
        }
    }

    internal class LinkedEffectContentImporterContext : ContentImporterContext
    {
        ContentProcessorContext context;

        public LinkedEffectContentImporterContext(ContentProcessorContext context)
        {
            this.context = context;
        }

        public override void AddDependency(string filename)
        {
            context.AddDependency(filename);
        }

        public override string IntermediateDirectory
        {
            get { return context.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return context.Logger; }
        }

        public override string OutputDirectory
        {
            get { return context.OutputDirectory; }
        }
    }
}
