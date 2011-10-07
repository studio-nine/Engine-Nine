#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Xaml;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
#endregion

namespace Nine.Content.Pipeline.Importers
{
    /// <summary>
    /// Imports object graph from Xaml files.
    /// </summary>
    [ContentImporter(".xaml", DisplayName = "Xaml Importer - Engine Nine")]
    public class XamlImporter : ContentImporter<object>
    {
        public override object Import(string filename, ContentImporterContext context)
        {
            try
            {
                return XamlServices.Load(filename);
            }
            catch (Exception e)
            {
                try
                {
                    // Sometimes this line will throw an exception if the InnerException is not in a correct format
                    // required by string.Format.
                    context.Logger.LogImportantMessage(e.InnerException.ToString());
                }
                catch { }
                throw;
            }
        }
    }
}
