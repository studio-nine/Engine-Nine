namespace Nine.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;

    class ThicknessConverter : TypeConverter
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
                    var Values = value.ToString().Trim().Split(',');

                    switch (Values.Length)
                    {
                        case 1:
                            return new Nine.Graphics.UI.Thickness(float.Parse(Values[0]));
                        case 2:
                            return new Nine.Graphics.UI.Thickness(float.Parse(Values[0]), float.Parse(Values[1]));
                        case 4:
                            return new Nine.Graphics.UI.Thickness(float.Parse(Values[0]), float.Parse(Values[1]), float.Parse(Values[2]), float.Parse(Values[3]));
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Invalid format. Must be formatted like \"Left, Top, Right, Bottom\"", e);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
