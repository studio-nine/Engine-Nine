#region File Description
//-----------------------------------------------------------------------------
// ManifestPipeline.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Content.Pipeline;

namespace Nine.Content.Pipeline.Importers
{
    // the importer is just a passthrough that gives the processor the filepath
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
