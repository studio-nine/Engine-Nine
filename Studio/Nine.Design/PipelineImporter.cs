namespace Nine.Design
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

        protected override T Import(string fileName)
        {
            var builder = new PipelineBuilder(GraphicsDevice);
            builder.ExternalReferenceResolve += externalReference =>
            {
                Dependencies.Add(externalReference);
                return externalReference;
            };
            return builder.Load<T>(fileName, ContentImporter, ContentProcesser);
        }
    }
}
