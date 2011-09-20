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
    [ContentProcessor(DisplayName = "Linked Effect - Engine Nine")]
    public class LinkedEffectProcessor : ContentProcessor<LinkedEffectContent, LinkedEffectContent>
    {
        public override LinkedEffectContent Process(LinkedEffectContent input, ContentProcessorContext context)
        {
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

            string identity = Path.GetRandomFileName();
            string resultStitchupFile = Path.Combine(context.IntermediateDirectory, identity + ".stitchedeffect");
            string resultEffectFile = Path.Combine(Path.GetTempPath(), identity + ".fx");
            string resultAsmFile = Path.Combine(context.IntermediateDirectory, identity + ".asm");
            File.WriteAllText(resultStitchupFile, content.ToString());

            StitchedEffectImporter importer = new StitchedEffectImporter();
            StitchedEffectContent stitchedContent = importer.Import(resultStitchupFile, new LinkedEffectContentImporterContext(context));
            
            StitchedEffectProcessor processor = new StitchedEffectProcessor();
            CompiledEffectContent compiledEffect = processor.Process(stitchedContent, context);

            List<string> uniqueNames = new List<string>();
            input.UniqueNames = processor.Symbols.StitchedFragments.Select(symbol => symbol.UniqueName + "_").ToArray();
            input.EffectCode = compiledEffect.GetEffectCode();
            input.Token = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(processor.EffectCode.ToString()));

            Disassemble(resultEffectFile, resultAsmFile, context);

            return input;
        }

        private void Disassemble(string effectFile, string asmFile, ContentProcessorContext context)
        {
            string dxSdkDir = Environment.GetEnvironmentVariable("DXSDK_DIR");

            if (!string.IsNullOrEmpty(dxSdkDir))
            {
                Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = context.IntermediateDirectory;
                process.StartInfo.FileName = Path.Combine(dxSdkDir, @"Utilities\bin\x86\fxc.exe");
                process.StartInfo.Arguments = "/nologo /Tfx_2_0 /O3 /Fc" + asmFile + " \"" + effectFile + "\"";
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
