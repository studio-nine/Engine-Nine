namespace Nine.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;
    
    class RangeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null)
            {
                return value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var destinationTypeProvider = context.GetService<IDestinationTypeProvider>();
                var destinationType = destinationTypeProvider.GetDestinationType();
                if (!destinationType.IsGenericType || destinationType.GetGenericTypeDefinition() != typeof(Range<>))
                    throw new InvalidOperationException("RangeConverter must be applied to Range<T>");

                try
                {
                    var innerType = destinationType.GetGenericArguments()[0];
                    var converter = TypeDescriptor.GetConverter(innerType);
                    if (value.ToString().Contains("~"))
                    {
                        var minMax = value.ToString().Split('~');
                        var min = converter.ConvertFrom(context, culture, minMax[0]);
                        var max = converter.ConvertFrom(context, culture, minMax[1]);
                        return Activator.CreateInstance(destinationType, min, max);
                    }
                    return Activator.CreateInstance(destinationType, converter.ConvertFrom(context, culture, value));
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Invalid range format. Range must be formatted to \"Min ~ Max\"", e);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
