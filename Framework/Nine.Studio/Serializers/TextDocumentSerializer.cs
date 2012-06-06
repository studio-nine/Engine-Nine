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
using System.Xml.Serialization;
using System.ComponentModel.Composition;
#endregion

namespace Nine.Studio.Serializers
{
    public class TextDocumentSerializer : DocumentSerializer<string>
    {
        protected override void Serialize(Stream output, string value)
        {
            var writer = new StreamWriter(output);
            writer.Write(value);
            writer.Flush();
        }

        protected override string Deserialize(Stream input)
        {
            return new StreamReader(input).ReadToEnd();
        }
    }
}
