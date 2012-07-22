namespace Nine.Studio.Serializers
{
    using System;
    using System.IO;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Nine.Content.Pipeline;
    using Nine.Studio.Extensibility;

    public abstract class PipelineImporter<T> : Importer<T>
    {
        public abstract IContentImporter ContentImporter { get; }
        public abstract IContentProcessor ContentProcesser { get; }

        protected override T Import(Stream input)
        {
            FileStream fileStream = input as FileStream;
            if (fileStream == null)
                throw new NotSupportedException();
            
            // Need to close and reopen the file by content importer.
            fileStream.Close();

            return PipelineBuilder.Load<T>(fileStream.Name, ContentImporter, ContentProcesser);
        }
    }
}
