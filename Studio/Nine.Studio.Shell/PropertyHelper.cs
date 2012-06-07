#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#endregion

namespace Nine.Studio.Shell
{
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
