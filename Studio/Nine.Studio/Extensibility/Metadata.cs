namespace Nine.Studio.Extensibility
{
    using System;
    using System.ComponentModel;


    using System.ComponentModel.Composition;

    /// <summary>
    /// Defines the metadata that can be exported from the extensions.
    /// </summary>
    public interface IMetadata
    {
        /// <summary>
        /// Gets the display name.
        /// </summary>
        [LocalizedDefaultValue("NoName")]
        string DisplayName { get; }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        [LocalizedDefaultValue("General")]
        string Category { get; }

        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <remarks>
        /// The icon can be a <c>System.Drawing.Bitmap</c> or <c>System.Windows.Media.ImageSource</c>.
        /// </remarks>
        [DefaultValue(null)]
        object Icon { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is default.
        /// </summary>
        [DefaultValue(false)]
        bool IsDefault { get; }

        /// <summary>
        /// Gets the name of the folder.
        /// </summary>
        [DefaultValue("Misc")]
        string FolderName { get; }
    }

    /// <summary>
    /// Allows strongly typed metadata exports
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportExtensionAttribute : ExportAttribute, IMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportExtensionAttribute"/> class.
        /// </summary>
        /// <param name="contractType">Type of the contract.</param>
        public ExportExtensionAttribute(Type contractType) : base(contractType) 
        {
            DisplayName = Strings.NoName;
            Category = Strings.General;
            IsDefault = false;
            FolderName = "Misc";
            Icon = null;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        public string Category { get; set; }

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

        /// <summary>
        /// Gets the name of the folder.
        /// </summary>
        public string FolderName { get; set; }
    }

    class Metadata : IMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportExtensionAttribute"/> class.
        /// </summary>
        /// <param name="contractType">Type of the contract.</param>
        public Metadata()
        {
            DisplayName = Strings.NoName;
            Category = Strings.General;
            IsDefault = false;
            FolderName = "Misc";
            Icon = null;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        public string Category { get; set; }

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

        /// <summary>
        /// Gets the name of the folder.
        /// </summary>
        public string FolderName { get; set; }
    }
}
