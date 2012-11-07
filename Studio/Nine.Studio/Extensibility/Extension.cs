namespace Nine.Studio.Extensibility
{
    using System;
    using System.Linq;
    using System.ComponentModel;

    /// <summary>
    /// Defines the metadata that can be exported from the extensions.
    /// </summary>
    public class Extension<T> where T : class
    {
        public T Value { get; private set; }
        public string DisplayName { get; set; }
        public string Category { get; set; }
        public string Class { get; set; }
        public object Icon { get; set; }
        public bool IsDefault { get; set; }

        public Extension(EditorExtensions extensions, T value)
        {
            Verify.IsNotNull(value, "value");
            
            var attributes = extensions.GetCustomAttributes(value.GetType())
                                       .Concat(value.GetType().GetCustomAttributes(true));

            foreach (var metadata in attributes.OfType<ExportMetadataAttribute>())
            {
                IsDefault |= metadata.IsDefault;
                Icon = Icon != null ? metadata.Icon: Icon;
            }

            DisplayName = extensions.GetDisplayName(value);
            Category = extensions.GetCategory(value);
            Class = extensions.GetClass(value);
            Value = value;
        }
        
        public override string ToString()
        {
            return string.Format("{0} - [{1}][{2}]", Value.GetType().AssemblyQualifiedName, DisplayName ?? "", Category ?? "");
        }
    }
}
