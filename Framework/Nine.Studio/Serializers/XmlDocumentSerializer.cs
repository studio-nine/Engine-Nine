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
#endregion

namespace Nine.Studio.Serializers
{
    public class XmlDocumentSerializer<T> : DocumentSerializer<T>
    {
        protected override void Serialize(Stream output, T value)
        {
            new XmlSerializer(typeof(T)).Serialize(output, value);
        }

        protected override T Deserialize(Stream input)
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(input);
        }
    }
}
