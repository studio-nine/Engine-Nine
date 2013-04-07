namespace Nine.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;
    using Nine.Graphics.UI;

    class GridLengthConverter : TypeConverter
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
                try
                {
                    if (value.ToString().ToLower().Contains("auto"))
                        return new GridLength(0, GridUnitType.Auto);
                    else if (value.ToString().Contains("*"))
                    {
                        if (value.ToString().Equals("*"))
                            return new GridLength(1, GridUnitType.Star);

                        var nv = value.ToString().Replace("*", "").Replace(".", ",");
                        return new GridLength(float.Parse(nv), GridUnitType.Star);
                    }
                    else
                    {
                        return new GridLength(float.Parse(value.ToString()));
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Invalid Format.", e);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
