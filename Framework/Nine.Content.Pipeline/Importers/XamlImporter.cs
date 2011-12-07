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
    [ContentImporter(".xaml", DisplayName = "Xaml Importer - Engine Nine", DefaultProcessor="DefaultContentProcessor")]
    public class XamlImporter : ContentImporter<object>
    {
        public override object Import(string filename, ContentImporterContext context)
        {
            try
            {
                var result = XamlServices.Load(filename);

                // Try to populate the identity of content items.
                ObjectGraph.ForEachProperty(result, (type, input) =>
                {
                    var contentItem = input as ContentItem;
                    if (contentItem != null)
                    {
                        contentItem.Identity = new ContentIdentity(filename, typeof(XamlImporter).Name, null);
                    }
                    return input;
                });
                return result;
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
