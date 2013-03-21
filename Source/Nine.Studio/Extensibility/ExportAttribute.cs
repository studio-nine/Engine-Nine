namespace Nine.Studio.Extensibility
{
    using System;
        
    /// <summary>
    /// Allows strongly typed metadata exports
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportAttribute : Attribute
    {
        public Type ContractType { get; private set; }

        public ExportAttribute(Type contractType)
        {
            this.ContractType = contractType;
        }
    }

    /// <summary>
    /// Allows strongly typed metadata exports
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class ExportMetadataAttribute : Attribute
    {
        /// <summary>
        /// Gets the localized name that will be displayed on the UI.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the localized category name that will be displayed on the UI.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets the class of the object that is not localized. E.g. Models, Textures, Misc.
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <remarks>
        /// The icon can be a <c>System.Drawing.Bitmap</c> or <c>System.Windows.Media.ImageSource</c>.
        /// </remarks>
        public object Icon { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is default.
        /// </summary>
        public bool IsDefault { get; set; }        
    }
}
