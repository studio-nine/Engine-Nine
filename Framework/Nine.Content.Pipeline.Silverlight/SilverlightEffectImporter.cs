// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace SilverlightContentPipeline
{
    [ContentImporter(".fx", ".slfx", DisplayName = "Effect - Silverlight", DefaultProcessor = "SilverlightEffectProcessor")]
    public class SilverlightEffectImporter : ContentImporter<EffectSourceCode>
    {
        public override EffectSourceCode Import(string filename, ContentImporterContext context)
        {
            ExceptionHelper.Filename = filename;

            try
            {
                string sourceCode = File.ReadAllText(filename);

                return new EffectSourceCode(sourceCode);
            }
            catch (Exception ex)
            {
                ExceptionHelper.RaiseException("Unable to read effect.", ex);                
            }

            return null;
        }
    }
}
