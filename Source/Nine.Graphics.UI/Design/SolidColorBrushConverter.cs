namespace Nine.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Xaml;

    class SolidColorBrushConverter : TypeConverter
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
                            var ColorClass = typeof(Microsoft.Xna.Framework.Color);
                            var Color = (Microsoft.Xna.Framework.Color)ColorClass.GetProperty(Values[0], BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase).GetValue(null, null);
                            return new Nine.Graphics.UI.Media.SolidColorBrush((Microsoft.Xna.Framework.Color)Color);return new Nine.Graphics.UI.Media.SolidColorBrush((Microsoft.Xna.Framework.Color)Color);
                        case 3:
                            return new Nine.Graphics.UI.Media.SolidColorBrush(
                                float.Parse(Values[0].Replace('.', ',')),
                                float.Parse(Values[1].Replace('.', ',')),
                                float.Parse(Values[2].Replace('.', ',')));
                        case 4:
                            return new Nine.Graphics.UI.Media.SolidColorBrush(
                                float.Parse(Values[0].Replace('.', ',')),
                                float.Parse(Values[1].Replace('.', ',')),
                                float.Parse(Values[2].Replace('.', ',')),
                                float.Parse(Values[3].Replace('.', ',')));
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Invalid Value/Color.", e);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
