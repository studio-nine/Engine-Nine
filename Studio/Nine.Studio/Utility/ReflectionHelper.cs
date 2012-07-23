namespace Nine.Studio
{

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;



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
