#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Xaml;
using Nine.Studio.Extensibility;

#endregion

namespace Nine.Studio.Serializers
{
    //[Export(typeof(IImporter))]
    //[Export(typeof(IExporter))]
    //[LocalizedDisplayName("XnaXamlAssert")]
    public class XamlSerializer : Serializer<object>
    {
        public XamlSerializer()
        {
            FileExtensions.Add(".xaml");
        }

        public override bool CheckSupported(byte[] header)
        {
            string xml = Encoding.UTF8.GetString(header);
            return xml.ToLowerInvariant().Contains("<");
        }

        protected override void Serialize(Stream output, object value)
        {
            var serializer = new Nine.Content.Pipeline.Xaml.XamlSerializer();
            serializer.Save(output, value);
        }

        protected override object Deserialize(Stream input)
        {
            var serializer = new Nine.Content.Pipeline.Xaml.XamlSerializer();
            return serializer.Load(input);
        }
    }
}
