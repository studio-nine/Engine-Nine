namespace Nine.Studio.Serializers
{
    using System.IO;
    using Nine.Studio.Extensibility;

    public class TextSerializer : Serializer<string>
    {
        public TextSerializer()
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
