namespace Nine.Content.Pipeline.Xaml
{
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// Defines a markup extension to reference external content.
    /// </summary>
    [ContentProperty("AssetNameName")]
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
        /// <param name="assetName">The name of the asset.</param>
        public ContentReference(string assetName) { AssetName = assetName; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return ResolveContentReference(AssetName);
        }

        /// <summary>
        /// Occurs when this serializer failed to create an instance from a content reference.
        /// </summary>
        public event Func<string, object> Resolve;

        /// <summary>
        /// Resolves the external reference.
        /// </summary>
        private object ResolveContentReference(string referenceName)
        {
            if (Resolve != null)
            {
                foreach (var invocation in Resolve.GetInvocationList())
                {
                    var resolve = (Func<string, object>)invocation;
                    if (resolve == null)
                        continue;

                    try
                    {
                        var result = resolve(referenceName);
                        if (result != null)
                            return result;
                    }
                    catch (Exception) { }
                }
            }
            return null;
        }
    }
}
