namespace Nine
{
    using System;
    using System.Windows.Markup;
    using System.Xaml;
    using Nine.Serialization;

    /// <summary>
    /// Defines a markup extension to reference external content.
    /// </summary>
    [BinarySerializable]
    [ContentProperty("AssetName")]
    public class ContentReference : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the name of the content.
        /// This value should be set to the asset name instead of filename when used with Xna content pipeline.
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        public ContentReference() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        /// <param name="fileName">The name of the asset.</param>
        public ContentReference(string fileName) { AssetName = fileName; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
#if WINDOWS
            var xamlSchemaContextProvider = serviceProvider.TryGetService<IXamlSchemaContextProvider>();
            if (xamlSchemaContextProvider != null)
                serviceProvider = xamlSchemaContextProvider.SchemaContext as IServiceProvider;
#endif
#if !MonoGame
            var loader = serviceProvider.TryGetService<ContentLoader>();
            if (loader != null)
            {
                var result = loader.Load<object>(AssetName);
                var serializationOverride = serviceProvider.TryGetService<ISerializationOverride>();
                if (serializationOverride != null)
                    serializationOverride.SetOverride(result, this);
                return result;
            }
#endif
            return null;
        }
    }
}