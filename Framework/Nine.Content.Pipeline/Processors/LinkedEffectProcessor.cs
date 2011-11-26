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
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;
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
    [ContentProcessor(DisplayName = "Linked Effect - Engine Nine")]
    public class LinkedEffectProcessor : ContentProcessor<LinkedEffectContent, LinkedEffectContent>
    {
        const string LinkedEffectWorkingPath = "LinkedEffects";

        public override LinkedEffectContent Process(LinkedEffectContent input, ContentProcessorContext context)
        {
            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
                return input;
            
            // Process standard effect
            Process(input, context, false);

            // Process the deferred effect
            var deferred = new LinkedEffectContent();
            deferred.EffectParts.AddRange(input.EffectParts);
            if (deferred.EffectParts.Any(part => part is DeferredLightsEffectPartContent))
            {
                Process(deferred, context, true);
                input.GraphicsBufferEffect = deferred;
            }
            return input;
        }

        private void Process(LinkedEffectContent input, ContentProcessorContext context, bool deferred)
        {
            StringBuilder content = new StringBuilder();

            if (!Directory.Exists(Path.Combine(context.IntermediateDirectory, LinkedEffectWorkingPath)))
                Directory.CreateDirectory(Path.Combine(context.IntermediateDirectory, LinkedEffectWorkingPath));

            int currentName = 0;
            int[] nameMapping = new int[input.EffectParts.Count];
            for (int i = 0; i < input.EffectParts.Count; i++)
            {
                var part = input.EffectParts[i];
                part.EffectParts = input.EffectParts;
                part.Validate(context);

                var effectCode = part.EffectCode;
                var graphicsBufferCode = part.GraphicsBufferEffectCode;
                var code = deferred ? graphicsBufferCode : effectCode;
                if (string.IsNullOrEmpty(effectCode) && !string.IsNullOrEmpty(graphicsBufferCode))
                    throw new InvalidContentException(string.Format("EffectPart {0} contains graphics buffer effect code but does not have any effect code", part));

                if (!string.IsNullOrEmpty(code))
                {
                    var filename = "LinkedFragment-" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName()).ToUpperInvariant() + ".fragment";
                    filename = Path.Combine(context.IntermediateDirectory, LinkedEffectWorkingPath, filename);
                    content.AppendLine(filename);
                    File.WriteAllText(filename, deferred ? part.GraphicsBufferEffectCode : part.EffectCode);
                    nameMapping[i] = currentName++;
                }
                else
                {
                    nameMapping[i] = -1;
                }
            }

            string identity = "LinkedEffect-" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName()).ToUpperInvariant();
            string resultStitchupFile = Path.Combine(context.IntermediateDirectory, LinkedEffectWorkingPath, identity + ".stitchedeffect");
            string resultEffectFile = Path.Combine(context.IntermediateDirectory, LinkedEffectWorkingPath, identity + ".fx");
            string resultAsmFile = Path.Combine(context.IntermediateDirectory, LinkedEffectWorkingPath, identity + ".asm");
            File.WriteAllText(resultStitchupFile, content.ToString());

            StitchedEffectImporter importer = new StitchedEffectImporter();
            StitchedEffectContent stitchedContent = importer.Import(resultStitchupFile, new LinkedEffectContentImporterContext(context));

            StitchedEffectProcessor processor = new StitchedEffectProcessor();
            CompiledEffectContent compiledEffect = processor.Process(stitchedContent, context);

            var uniqueNames = processor.Symbols.StitchedFragments.Select(symbol => symbol.UniqueName + "_").ToArray();
            input.UniqueNames = Enumerable.Range(0, input.EffectParts.Count).Select(i => nameMapping[i] >= 0 ? uniqueNames[nameMapping[i]] : null).ToArray();
            input.EffectCode = compiledEffect.GetEffectCode(); 
            input.Token = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(processor.EffectCode.ToString()));

            Disassemble(resultEffectFile, resultAsmFile, context);
        }

        private void Disassemble(string effectFile, string asmFile, ContentProcessorContext context)
        {
            string dxSdkDir = Environment.GetEnvironmentVariable("DXSDK_DIR");

            if (!string.IsNullOrEmpty(dxSdkDir))
            {
                Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = Path.Combine(context.IntermediateDirectory, LinkedEffectWorkingPath);
                process.StartInfo.FileName = Path.Combine(dxSdkDir, @"Utilities\bin\x86\fxc.exe");
                process.StartInfo.Arguments = "/nologo /Tfx_2_0 /O3 /Fc \"" + asmFile + "\" \"" + effectFile + "\"";
                context.Logger.LogMessage(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                
                process.Start();
                process.WaitForExit();
                context.Logger.LogImportantMessage(string.Format("{0} : (double-click this message to view disassembly file).", asmFile));
            }
        }
    }

    class LinkedEffectContentImporterContext : ContentImporterContext
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
