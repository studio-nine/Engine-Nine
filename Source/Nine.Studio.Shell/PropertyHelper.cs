namespace Nine.Studio.Shell
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    static class PropertyHelper
    {
        public static IEnumerable<PropertyDescriptor> GetBrowsableProperties(object value)
        {
            if (value == null)
                return Enumerable.Empty<PropertyDescriptor>();

            return from property in TypeDescriptor.GetProperties(value).OfType<PropertyDescriptor>()
                   where !property.IsReadOnly && IsEditorBrowsable(property)
                   select property;
        }

        public static bool IsEditorBrowsable(PropertyDescriptor property)
        {
            if (property == null)
                return false;
            var attribute = property.Attributes.OfType<EditorBrowsableAttribute>().FirstOrDefault();
            return attribute != null && attribute.State != EditorBrowsableState.Never;
        }
    }
}
