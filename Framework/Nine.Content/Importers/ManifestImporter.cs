namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;

    [ContentImporter(".manifest", DisplayName = "Manifest Importer - XNA Framework", DefaultProcessor = "ManifestProcessor")]
    public class ManifestImporter : ContentImporter<string>
    {
        public override string Import(string filename, ContentImporterContext context)
        {
            // just give the processor the filename needed to do the processing
            return filename;
        }
    }
}
