namespace Nine.Content.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;
    using IImporter = Microsoft.Xna.Framework.Content.Pipeline.IContentImporter;
    using IProcessor = Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor;

    /// <summary>
    /// Encapsulate commonly used XNA content pipeline functionalities.
    /// </summary>
    public static class ContentPipeline
    {
        static PipelineBuilder builder = new PipelineBuilder();

        public static void SaveContent(Stream output, object content)
        {
            builder.Compile(output, content);
        }

        public static T LoadContent<T>(string fileName, IImporter importer, IProcessor processor = null)
        {
            return builder.BuildAndLoad<T>(fileName, importer, processor);
        }
        
        public static T LoadContent<T>(Stream input, IImporter importer, IProcessor processor = null)
        {
            var file = input as FileStream;
            if (file == null)
                throw new NotSupportedException("Only FileStream is supported.");
            file.Close();
            return builder.BuildAndLoad<T>(file.Name, importer, processor);
        }

        public static T Load<T>(string fileName, IImporter importer, IProcessor processor = null, IServiceProvider serviceProvider = null)
        {
            builder.Compile(builder.OutputFilename, LoadContent<T>(fileName, importer, processor));
            return Singleton<IServiceProvider, PipelineContentManager>.GetInstance(serviceProvider).Load<T>(builder.OutputFilename);
        }

        public static T Load<T>(Stream input, IImporter importer, IProcessor processor = null, IServiceProvider serviceProvider = null)
        {
            builder.Compile(builder.OutputFilename, LoadContent<T>(input, importer, processor));
            return Singleton<IServiceProvider, PipelineContentManager>.GetInstance(serviceProvider).Load<T>(builder.OutputFilename);
        }

        internal static string[] GetSupportedFileExtensions(Type importerType)
        {
            return importerType.GetCustomAttributes(true)  
                               .OfType<ContentImporterAttribute>()
                               .SelectMany(x => x.FileExtensions).ToArray();
        }
    }
}
