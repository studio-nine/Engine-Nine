namespace Nine.Design
{
    using System;
    using System.IO;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Nine.Content.Importers;
    using Nine.Studio.Extensibility;
    using System.Collections.Generic;

    [Export(typeof(IFactory))]
    [LocalizedCategory("General")]
    [LocalizedDisplayName("Scene", typeof(Resources))]
    public class SceneFactory : Factory<Scene, object>
    {

    }

    [Export(typeof(IImporter))]
    [LocalizedCategory("General")]
    [LocalizedDisplayName("Scene", typeof(Resources))]
    [ExportMetadata(Class = "Scenes", IsDefault = true)]
    public class SceneImporter : PipelineImporter<Scene>
    {
        public override IContentImporter ContentImporter
        {
            get { return new XamlImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return null; }
        }

        public override IEnumerable<string> GetSupportedFileExtensions()
        {
            yield return ".xaml";
        }
    }

    [Export(typeof(IExporter))]
    [LocalizedCategory("General")]
    [LocalizedDisplayName("Scene", typeof(Resources))]
    public class SceneExporter : Exporter<Scene>
    {
        public override IEnumerable<string> GetSupportedFileExtensions()
        {
            yield return ".xaml";
        }

        protected override void Export(Stream output, Scene value)
        {
            new Nine.Content.XamlSerializer().Save(output, value);
        }
    }
}
