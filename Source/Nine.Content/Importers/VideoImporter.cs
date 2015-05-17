namespace Nine.Serialization.Importers
{
    using Nine.Content.Pipeline;
    using System;
    using System.IO;

    /// <remarks>
    /// Due to the limitation of XNA video content pipeline, videos cannot be loaded
    /// from binary packages. The .wmv file has to be placed into the file system.
    /// </remarks>
    public class VideoImporter : Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return ContentPipeline.Load<Microsoft.Xna.Framework.Media.Video>(stream, new Microsoft.Xna.Framework.Content.Pipeline.WmvImporter(), null, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return ContentPipeline.GetSupportedFileExtensions(typeof(Microsoft.Xna.Framework.Content.Pipeline.WmvImporter)); }
        }
    }

    class VideoWriter : PipelineObjectWriter<Microsoft.Xna.Framework.Media.Video> { }
}