#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.IO;
using System.Xml.Serialization;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Serializers
{
    public class XmlDocumentSerializer<T> : Serializer<T>
    {
        public XmlDocumentSerializer()
        {
            FileExtensions.Add(".xml");
        }

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
