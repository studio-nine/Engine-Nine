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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Nine.Studio.Extensibility;
using Nine.Studio.Controls;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nine.Studio.Content;
#endregion

namespace Nine.Studio.Serializers
{
    public abstract class PipelineDocumentSerializer<T> : DocumentSerializer<T>
    {
        PipelineImporterContext importerContext = new PipelineImporterContext();
        PipelineProcessorContext processorContext = new PipelineProcessorContext();

        public abstract IContentImporter ContentImporter { get; }
        public abstract IContentProcessor ContentProcesser { get; }

        protected PipelineDocumentSerializer()
        {
            CanSerialize = false;
        }

        protected override void Serialize(Stream output, T value)
        {
            throw new NotSupportedException();
        }

        protected override T Deserialize(Stream input)
        {
            FileStream fileStream = input as FileStream;
            if (fileStream == null)
                throw new NotSupportedException("PipelineDocumentSerializer only support FileStream");
            
            // Need to close and reopen the file by content importer.
            fileStream.Close();            

            object content = ContentImporter.Import(fileStream.Name, importerContext);
            if (ContentProcesser != null)
                content = ContentProcesser.Process(content, processorContext);
            return (T)content;
        }
    }
}
