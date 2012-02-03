#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
#endregion

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// ContentReference is used to reference the output xnb file.
    /// ExternalReference is used to reference the source asset file.
    /// </summary>
    [TypeConverter(typeof(Nine.Content.Pipeline.Design.ContentReferenceConverter))]
    public class ContentReference<T> : ContentItem
    {
        /// <summary>
        /// Gets and sets the file name of an External.
        /// </summary>
        public string Filename { get; set; }

        public ContentReference() { }
        public ContentReference(string filename) { Filename = filename; }
        public ContentReference(ExternalReference<T> externalReference) { Filename = externalReference.Filename; }
        
        public static implicit operator ContentReference<T>(string value)
        {
            return new ContentReference<T>(value);
        }

        public static implicit operator ContentReference<T>(ExternalReference<T> value)
        {
            return new ContentReference<T>(value);
        }
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
            if (existingInstance == null)
                existingInstance = new ContentReference<T>();
            existingInstance.Filename = input.Xml.ReadContentAsString();
            return existingInstance;
        }

        protected override void Serialize(IntermediateWriter output, ContentReference<T> value, ContentSerializerAttribute format)
        {
            var path = ContentProcessorContextExtensions.ContentReferenceBasePath;
            if (string.IsNullOrEmpty(path))
                output.Xml.WriteString(value.Filename);
            else if (value.Filename.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
                output.Xml.WriteString(value.Filename);
            else
                output.Xml.WriteString(new Uri(Path.Combine(path, value.Filename + ".xnb")).LocalPath);
        }
    }
}
