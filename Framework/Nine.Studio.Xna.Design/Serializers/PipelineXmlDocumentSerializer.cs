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
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Nine.Studio.Extensibility;
using Nine.Studio.Controls;
using Nine.Studio.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
#endregion

namespace Nine.Studio.Serializers
{
    [Export(typeof(IDocumentSerializer))]
    public class PipelineXmlDocumentSerializer : DocumentSerializer<object>
    {
        public PipelineXmlDocumentSerializer()
        {
            DisplayName = Strings.XnaXmlAsset;
            FileExtensions.Add("*.xml");
        }

        protected override void Serialize(Stream output, object value)
        {
            IntermediateSerializer.Serialize(XmlWriter.Create(output, new XmlWriterSettings { Indent = true }), value, null);
        }

        protected override object Deserialize(Stream input)
        {
            return IntermediateSerializer.Deserialize<object>(XmlReader.Create(input), null);
        }
    }
}
