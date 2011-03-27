#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// A replacement for ExternalReference to allow referencing the output *.xnb file
    /// without providing a full path.
    /// </summary>
    public class ContentReference<T> : ContentItem
    {
        /// <summary>
        /// Gets and sets the file name of an External.
        /// </summary>
        public string Filename { get; set; }

        public ContentReference() { }
        public ContentReference(string filename) { Filename = filename; }
    }

    [ContentTypeWriter]
    class ContentReferenceWriter<T> : ContentTypeWriter<ContentReference<T>>
    {
        ContentTypeWriter targetWriter;

        protected override void Initialize(ContentCompiler compiler)
        {
            targetWriter = compiler.GetTypeWriter(typeof(T));
        }

        protected override void Write(ContentWriter output, ContentReference<T> value)
        {
            if (value.Filename.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
                output.WriteExternalReference<T>(new ExternalReference<T>(value.Filename));
            else
                output.Write(value.Filename);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return targetWriter.GetRuntimeType(targetPlatform);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return (typeof(ContentTypeReader).Namespace + ".ExternalReferenceReader");
        }
    }
    
    [ContentTypeSerializer]
    class ContentReferenceSerializer<T> : ContentTypeSerializer<ContentReference<T>>
    {
        protected override ContentReference<T> Deserialize(IntermediateReader input, ContentSerializerAttribute format, ContentReference<T> existingInstance)
        {
            return new ContentReference<T>(input.Xml.ReadContentAsString());
        }

        protected override void Serialize(IntermediateWriter output, ContentReference<T> value, ContentSerializerAttribute format)
        {
            output.Xml.WriteString(value.Filename);
        }
    }
}
