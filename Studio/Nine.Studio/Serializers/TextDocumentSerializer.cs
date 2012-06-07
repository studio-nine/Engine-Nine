#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.IO;
using Nine.Studio.Extensibility;

#endregion

namespace Nine.Studio.Serializers
{
    public class TextDocumentSerializer : Serializer<string>
    {
        public TextDocumentSerializer()
        {
            FileExtensions.Add(".txt");
        }

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
