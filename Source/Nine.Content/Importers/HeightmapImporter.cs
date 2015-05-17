namespace Nine.Serialization.Importers
{
    using Nine.Content.Pipeline;
    using Nine.Content.Pipeline.Processors;
    using System;
    using System.IO;

    public class HeightmapImporter : HeightmapProcessor, Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return ContentPipeline.LoadContent<Nine.Graphics.Heightmap>(stream, new Microsoft.Xna.Framework.Content.Pipeline.TextureImporter(), this);
        }

        public string[] SupportedFileExtensions
        {
            get { return null; }
        }
    }
}