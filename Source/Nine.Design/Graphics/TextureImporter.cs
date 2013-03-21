namespace Nine.Graphics.Design
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Studio.Extensibility;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using XnaImporter = Microsoft.Xna.Framework.Content.Pipeline.TextureImporter;
    using XnaProcessor = Microsoft.Xna.Framework.Content.Pipeline.Processors.TextureProcessor;


    [Export(typeof(IImporter))]
    [LocalizedDisplayName("Texture")]
    [ExportMetadata(Class = "Textures", IsDefault = true)]
    public class TextureImporter : Nine.Design.PipelineImporter<Texture2D>
    {
        public override IContentImporter ContentImporter
        {
            get { return new XnaImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new XnaProcessor() { ColorKeyEnabled = false }; }
        }

        public override IEnumerable<string> GetSupportedFileExtensions()
        {
            yield return ".png";
            yield return ".gif";
            yield return ".jpg";
            yield return ".bmp";
            yield return ".tga";
            yield return ".dds";
        }

        protected override Texture2D Import(string fileName)
        {
            var processor = ContentProcesser as XnaProcessor;
            var needContentPipeline = processor.ColorKeyEnabled || processor.GenerateMipmaps || processor.ResizeToPowerOfTwo || processor.TextureFormat == TextureProcessorOutputFormat.DxtCompressed;

            if (!needContentPipeline && (
                fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return Texture2D.FromStream(GraphicsDevice, stream);
                }
            }
            return base.Import(fileName);
        }
    }
}
