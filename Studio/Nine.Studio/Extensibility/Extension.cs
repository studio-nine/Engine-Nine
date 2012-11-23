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
        public Editor Editor { get; private set; }
        public T Value { get; internal set; }
        public string DisplayName { get; set; }
        public string Category { get; set; }
        public string Class { get; set; }
        public object Icon { get; set; }
        public bool IsDefault { get; set; }

        public Extension(Editor editor, T value)
        {
            Verify.IsNotNull(editor, "editor");
            Verify.IsNotNull(value, "value");
            
            var attributes = editor.Extensions.GetCustomAttributes(value.GetType())
                                       .Concat(value.GetType().GetCustomAttributes(true));

            foreach (var metadata in attributes.OfType<ExportMetadataAttribute>())
            {
                IsDefault |= metadata.IsDefault;
                Icon = Icon != null ? metadata.Icon: Icon;
            }

            Editor = editor;
            DisplayName = editor.Extensions.GetDisplayName(value);
            Category = editor.Extensions.GetCategory(value);
            Class = editor.Extensions.GetClass(value);
            Value = value;
        }
        
        public override string ToString()
        {
            return string.Format("{0} - [{1}][{2}]", Value.GetType().AssemblyQualifiedName, DisplayName ?? "", Category ?? "");
        }
    }
}
