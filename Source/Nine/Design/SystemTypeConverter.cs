namespace Nine.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;
    
    class SystemTypeConverter : TypeConverter
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
                // TODO: this is not extensible...
                var typeString = value.ToString();
                return Type.GetType(typeString) ??
                       Type.GetType(string.Format("Nine.Graphics.{0}, Nine.Graphics", typeString)) ??
                       Type.GetType(string.Format("Microsoft.Xna.Framework.Graphics.{0}, Microsoft.Xna.Framework.Graphics", typeString));
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
