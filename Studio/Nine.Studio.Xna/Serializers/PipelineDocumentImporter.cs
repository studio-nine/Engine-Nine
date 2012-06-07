#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nine.Studio.Content;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Serializers
{
    public abstract class PipelineDocumentImporter<T> : Importer<T>
    {
        public abstract IContentImporter ContentImporter { get; }
        public abstract IContentProcessor ContentProcesser { get; }

        protected override T Import(Stream input)
        {
            FileStream fileStream = input as FileStream;
            if (fileStream == null)
                throw new NotSupportedException("PipelineDocumentSerializer only support FileStream");
            
            // Need to close and reopen the file by content importer.
            fileStream.Close();

            return PipelineBuilder.BuildAndLoad<T>(fileStream.Name, ContentImporter, ContentProcesser);
        }
    }
}
