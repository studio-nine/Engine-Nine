namespace Nine.Content.Pipeline.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;


    class ContentReferenceConverter : TypeConverter
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
                dynamic contentReference = value;
                return contentReference.Filename;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                // http://stackoverflow.com/questions/1723263/typeconverter-for-generic-type-used-in-xaml
                var destinationTypeProvider = context.GetService<IDestinationTypeProvider>();
                if (destinationTypeProvider == null)
                {
                    throw new NotSupportedException("IDestinationTypeProvider not found");
                }
                var destinationType = destinationTypeProvider.GetDestinationType();
                return Activator.CreateInstance(destinationType, value.ToString());
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
