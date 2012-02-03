#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// Enables a centralized place where LinkedEffect can be compiled and cached.
    /// Use this library to eliminated duplicated LinkedEffects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContentProcessorContextExtensions
    {
        #region BuildAsset
        // This value will be used by ContentReference when serialize using
        // reference relocation path.
        internal static string ContentReferenceBasePath;

        internal const string DefaultOutputDirectory = @"Misc";

        /// <summary>
        /// Initiates a nested build of the input object.
        /// </summary>
        public static ExternalReference<TOutput> BuildAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName)
        {
            return BuildAsset<TInput, TOutput>(context, input, processorName, null, null);
        }

        /// <summary>
        /// Initiates a nested build of the input object.
        /// </summary>
        /// <remarks>
        /// The input object will be serialized in to an Xml asset file using IntermediateSerializer. 
        /// Then that file is imported and processed though the content pipeline. If an asset name is specified,
        /// the processed object will use that asset name as the output. Otherwise, an MD5 hash string of the
        /// input object will be calculated as the asset name, and the asset will be written to the "Misc" 
        /// folder of the output directory.
        /// All content references and external references are automatically relocated to match the new output 
        /// directory.
        /// </remarks>
        public static ExternalReference<TOutput> BuildAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName, OpaqueDataDictionary processorParameters, string assetName)
        {
            string sourceFile;
            PrepareAsset<TInput>(context, input, ref assetName, out sourceFile);

            // Build the source asset
            var result = context.BuildAsset<TInput, TOutput>(new ExternalReference<TInput>(sourceFile), processorName, processorParameters, null, assetName);
            context.Logger.LogImportantMessage("Building {0} -> {1}", sourceFile, result.Filename);
            return result;
        }

        private static void PrepareAsset<TInput>(ContentProcessorContext context, TInput input, ref string assetName, out string sourceFile)
        {
            ContentReferenceBasePath = Path.GetDirectoryName(context.OutputFilename);

            // There's currently no way to build from object, so we need to create a temperory file
            using (var stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    try
                    {
                        IntermediateSerializer.Serialize(writer, input, Directory.GetCurrentDirectory() + "\\");
                    }
                    finally
                    {
                        ContentReferenceBasePath = null;
                    }
                }

                FixExternalReference(context, stream);

                if (string.IsNullOrEmpty(assetName))
                {
                    var hashString = new StringBuilder();
                    stream.Seek(0, SeekOrigin.Begin);

                    var hash = MD5.Create().ComputeHash(stream);
                    for (int i = 0; i < hash.Length; i++)
                    {
                        hashString.Append(hash[i].ToString("X2"));
                    }

                    var name = hashString.ToString().ToUpperInvariant();
                    assetName = Path.Combine(DefaultOutputDirectory, name);
                    sourceFile = Path.Combine(context.IntermediateDirectory, input.GetType().Name + "-" + name + ".xml");
                }
                else
                {
                    sourceFile = Path.Combine(context.IntermediateDirectory, Path.GetFileName(assetName) + ".xml");
                }

                using (var assetFile = new FileStream(sourceFile, FileMode.Create))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.WriteTo(assetFile);
                }
            }
        }

        private static void FixExternalReference(ContentProcessorContext context, MemoryStream stream)
        {
            var outputFile = new Uri(Path.GetFullPath(context.OutputFilename));
            var outputDirectory = new Uri(Path.GetFullPath(context.OutputDirectory));
            var relativePath = Path.GetDirectoryName(outputDirectory.MakeRelativeUri(outputFile).OriginalString);
            
            stream.Seek(0, SeekOrigin.Begin);            
            var xml = XDocument.Load(stream);

            foreach (var externalReference in xml.Descendants("ExternalReferences").SelectMany(e => e.Descendants("ExternalReference")))
            {
                var original = externalReference.Value.Clone();
                externalReference.Value = Path.GetFullPath(Path.Combine(relativePath, externalReference.Value));
                context.Logger.LogMessage("Fixing external reference {0} => {1}", original, externalReference.Value);
            }

            stream.Seek(0, SeekOrigin.Begin);
            xml.Save(stream);
        }
        #endregion

        #region BuildAndLoadAsset
        /// <summary>
        /// Initiates a nested build of the specified asset and then loads the result into memory.
        /// </summary>
        public static TOutput BuildAndLoadAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName)
        {
            return BuildAndLoadAsset<TInput, TOutput>(context, input, processorName, null, null);
        }

        /// <summary>
        /// Initiates a nested build of the specified asset and then loads the result into memory.
        /// </summary>
        /// <remarks>
        /// The input object will be serialized in to an Xml asset file using IntermediateSerializer. 
        /// Then that file is imported and processed though the content pipeline. If an asset name is specified,
        /// the processed object will use that asset name as the output. Otherwise, an MD5 hash string of the
        /// input object will be calculated as the asset name, and the asset will be written to the "Misc" 
        /// folder of the output directory.
        /// All content references and external references are automatically relocated to match the new output 
        /// directory.
        /// </remarks>
        public static TOutput BuildAndLoadAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            string sourceFile;
            string assetName = null;
            PrepareAsset<TInput>(context, input, ref assetName, out sourceFile);

            // Build the source asset
            return context.BuildAndLoadAsset<TInput, TOutput>(new ExternalReference<TInput>(sourceFile), processorName, processorParameters, importerName);            
        }
        #endregion

        #region BuildCompileAndLoadAsset
        public static void LoadAsset<TOutput>(this ContentProcessorContext context, string assetFilename, GraphicsProfile profile, Action<TOutput, GraphicsDevice, ContentManager> output)
        {
            var baseDirectory = Path.GetDirectoryName(context.OutputFilename);

            using (var graphicsDevice = GraphicsExtensions.CreateHiddenGraphicsDevice(profile))
            {
                using (var contentManager = new PipelineContentManager(baseDirectory, new GraphicsDeviceServiceProvider(graphicsDevice)))
                {
                    try
                    {
                        // Sometimes the content manager will fail to load the content when it cannot find
                        // the assembly. Listen to the AssemblyResolve event will solve this.
                        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

                        var runtimeObject = contentManager.Load<TOutput>(assetFilename);

                        output(runtimeObject, graphicsDevice, contentManager);
                    }
                    finally
                    {
                        AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
                    }
                }
            }
        }

        /// <summary>
        /// Initiates a nested build of the input object and loads the runtime type though ContentManager.
        /// </summary>
        public static void BuildCompileAndLoadAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName, Action<TOutput, GraphicsDevice, ContentManager> output)
        {
            BuildCompileAndLoadAsset(context, input, processorName, null, null, GraphicsProfile.Reach, output);
        }

        /// <summary>
        /// Initiates a nested build of the input object and loads the runtime type though ContentManager.
        /// </summary>
        public static void BuildCompileAndLoadAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName, OpaqueDataDictionary processorParameters, string importerName, GraphicsProfile profile, Action<TOutput, GraphicsDevice, ContentManager> output)
        {
            var baseDirectory = Path.GetDirectoryName(context.OutputFilename);
            var assetFilename = Path.Combine(context.OutputDirectory, "Compiled-" + Guid.NewGuid().ToString("B") + ".xnb");
            var pipelineContext = new PipelineContentProcessorContext(context);

            try
            {
                var asset = pipelineContext.BuildAndLoadAsset<TInput, object>(input, processorName, processorParameters, importerName);
                //var asset = pipelineContext.Convert<TInput, object>(input, processorName, processorParameters);

                Compile(asset, assetFilename, context);

                using (var graphicsDevice = GraphicsExtensions.CreateHiddenGraphicsDevice(profile))
                {
                    using (var contentManager = new PipelineContentManager(baseDirectory, new GraphicsDeviceServiceProvider(graphicsDevice)))
                    {
                        try
                        {
                            // Sometimes the content manager will fail to load the content when it cannot find
                            // the assembly. Listen to the AssemblyResolve event will solve this.
                            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

                            var runtimeObject = contentManager.Load<TOutput>(assetFilename);

                            output(runtimeObject, graphicsDevice, contentManager);
                        }
                        finally
                        {
                            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
                        }
                    }
                }
            }
            finally
            {
                if (File.Exists(assetFilename))
                    File.Delete(assetFilename);
            }
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == e.Name)
                    return assembly;
            }
            return null;
        }

        /// <summary>
        /// Hacks into the Xna content pipeline to compiles the input content to an xnb file.
        /// </summary>
        private static void Compile(object content, string outputFilename, ContentProcessorContext context)
        {
            context.Logger.LogImportantMessage("Compiling {0}", outputFilename);

            using (var output = new FileStream(outputFilename, FileMode.Create))
            {
                var constructor = typeof(ContentCompiler).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
                ContentCompiler compiler = (ContentCompiler)constructor.Invoke(null);
                var method = compiler.GetType().GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(compiler, new object[] 
                {
                    output, content, context.TargetPlatform, context.TargetProfile, false, ".", ".",
                });
            }
        }

        class PipelineContentManager : ContentManager
        {
            string BaseDirectory;

            public PipelineContentManager(string baseDirectory, IServiceProvider services)
                : base(services) 
            {
                BaseDirectory = baseDirectory;
            }

            protected override Stream OpenStream(string assetName)
            {
                // In case Xna content pipeline does not support absolute path.
                if (Path.IsPathRooted(assetName))
                {
                    if (File.Exists(assetName))
                        return new FileStream(assetName, FileMode.Open);
                }
                else
                {
                    var assetFile = Path.Combine(BaseDirectory, assetName) + ".xnb";
                    if (File.Exists(assetFile))
                        return new FileStream(assetFile, FileMode.Open);
                }
                return base.OpenStream(assetName);
            }
        }

        class PipelineContentProcessorContext : ContentProcessorContext
        {
            ContentProcessorContext context;

            public PipelineContentProcessorContext(ContentProcessorContext context)
            {
                this.context = context;
            }

            public override void AddDependency(string filename)
            {
                context.AddDependency(filename);
            }

            public override void AddOutputFile(string filename)
            {
                context.AddOutputFile(filename);
            }

            public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
            {
                return context.BuildAndLoadAsset<TInput, TOutput>(sourceAsset, processorName, processorParameters, importerName);
            }

            public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
            {
                var output = context.BuildAsset<TInput, TOutput>(sourceAsset, processorName, processorParameters, importerName, assetName);
                context.Logger.LogImportantMessage("SSSSSSSSSSs {0} -> {1}", sourceAsset.Filename, output.Filename);
                return output;
            }

            public override string BuildConfiguration
            {
                get { return context.BuildConfiguration; }
            }

            public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
            {
                return context.Convert<TInput, TOutput>(input, processorName, processorParameters);
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

            public override string OutputFilename
            {
                get { return context.OutputFilename; }
            }

            public override OpaqueDataDictionary Parameters
            {
                get { return context.Parameters; }
            }

            public override TargetPlatform TargetPlatform
            {
                get { return context.TargetPlatform; }
            }

            public override GraphicsProfile TargetProfile
            {
                get { return context.TargetProfile; }
            }
        }
        #endregion
    }
}