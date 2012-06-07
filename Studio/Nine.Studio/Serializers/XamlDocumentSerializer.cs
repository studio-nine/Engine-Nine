#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.IO;
using System.Xaml;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Serializers
{
    public class XamlDocumentSerializer<T> : Serializer<T>
    {
        public XamlDocumentSerializer()
        {
            FileExtensions.Add(".xaml");
        }

        protected override void Serialize(Stream output, T value)
        {
            XamlServices.Save(output, value);
        }

        protected override T Deserialize(Stream input)
        {
            return (T)XamlServices.Load(input);
        }
    }
}
