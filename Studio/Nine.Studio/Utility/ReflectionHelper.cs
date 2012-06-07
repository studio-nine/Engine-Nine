#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;
using System.ComponentModel;
using Nine.Studio.Extensibility;
using System.Diagnostics;
using System.Collections.Specialized;

#endregion

namespace Nine.Studio
{
    static class ReflectionHelper
    {
        public static IDictionary<string, object> SaveProperties(object target)
        {
            return GetProperties(target).ToDictionary(pd => pd.Name, pd =>pd.GetValue(target));
        }

        public static void LoadProperties(object target, IDictionary<string, object> properties)
        {
            GetProperties(target).ForEach(pd =>
            {
                object value;
                if (properties.TryGetValue(pd.Name, out value))
                    pd.SetValue(target, value);
            });
        }

        private static IEnumerable<PropertyDescriptor> GetProperties(object target)
        {
            Verify.IsNotNull(target, "target");

            return from pd in TypeDescriptor.GetProperties(target).Cast<PropertyDescriptor>()
                   where !pd.IsBrowsable || pd.IsReadOnly || pd.Converter == null ||
                         !pd.Converter.CanConvertFrom(typeof(string)) ||
                         !pd.Converter.CanConvertTo(typeof(string))
                   select pd;
        }
    }
}
