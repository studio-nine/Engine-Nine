namespace Nine.Studio.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Data;
    using System.Windows.Markup;    

    public class ToExtension : MarkupExtension
    {
        private string convertMethod;

        public ToExtension(string convertMethod)
        {
            this.convertMethod = convertMethod;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var typeAndMethod = convertMethod.Split('.');
            var type = typeAndMethod.Length > 1 ? (Type)(new System.Windows.Markup.TypeExtension(typeAndMethod[0]).ProvideValue(serviceProvider)) : typeof(Converters);
            var method = type.GetMethod(typeAndMethod.Length > 1 ? typeAndMethod[1] : typeAndMethod[0]);

            Converter converter;
            if (!Converters.TryGetValue(method, out converter))
            {
                if (method.GetParameters().Length == 1)
                    converter = new Converter { Convert1 = (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), method) };
                else
                    converter = new Converter { Convert2 = (Func<object, Type, object>)Delegate.CreateDelegate(typeof(Func<object, Type, object>), method) };
                Converters[method] = converter;
            }
            return converter;
        }

        private static Dictionary<MethodInfo, Converter> Converters = new Dictionary<MethodInfo, Converter>();
        
        class Converter : IValueConverter
        {
            internal Func<object, object> Convert1;
            internal Func<object, Type, object> Convert2;

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return Convert1 != null ? Convert1(value) : Convert2(value, targetType);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
