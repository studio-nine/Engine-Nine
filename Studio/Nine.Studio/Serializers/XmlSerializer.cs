namespace Nine.Studio.Serializers
{
    using System.IO;
    using System.Text;
    using System.Xml;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
    using Nine.Studio.Extensibility;

    //[Export(typeof(IImporter))]
    //[Export(typeof(IExporter))]
    //[LocalizedDisplayName("XnaXmlAssert")]
    public class XmlSerializer : Serializer<object>
    {
        public XmlSerializer()
        {
            FileExtensions.Add(".xml");
        }

        public override bool CheckSupported(byte[] header)
        {
            string xml = Encoding.UTF8.GetString(header);
            return xml.ToLowerInvariant().Contains("?xml");
        }

        protected override void Serialize(Stream output, object value)
        {
            using (XmlWriter writer = XmlWriter.Create(output, new XmlWriterSettings() { Indent = true, CloseOutput = false }))
            {
                IntermediateSerializer.Serialize(writer, value, null);
            }
        }

        protected override object Deserialize(Stream input)
        {
            using (XmlReader reader = XmlReader.Create(input))
            {
                return IntermediateSerializer.Deserialize<object>(reader, null);
            }
        }
    }
}
