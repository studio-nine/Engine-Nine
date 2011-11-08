using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Nine.Content.Pipeline.Silverlight
{
    [ContentImporter(".fx", ".slfx", DisplayName = "Effect - Silverlight", DefaultProcessor = "SilverlightEffectProcessor")]
    public class SilverlightEffectImporter : ContentImporter<EffectContent>
    {
        public override EffectContent Import(string filename, ContentImporterContext context)
        {
            ExceptionHelper.Filename = filename;

            try
            {
                string sourceCode = File.ReadAllText(filename);

                return new EffectContent() { EffectCode = sourceCode, Identity = new ContentIdentity() { SourceFilename = filename } };
            }
            catch (Exception ex)
            {
                ExceptionHelper.RaiseException("Unable to read effect.", ex);                
            }

            return null;
        }
    }
}
