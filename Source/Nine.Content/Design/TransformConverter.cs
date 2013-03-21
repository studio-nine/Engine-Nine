namespace Nine.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Microsoft.Xna.Framework;

    class TransformConverter : TypeConverter
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
                    var converter = TypeDescriptor.GetConverter(typeof(Vector3));
                    var transform = Transform.Identity;
                    var components = value.ToString().Split(';');
                    if (components.Length > 0 && !string.IsNullOrEmpty(components[0]))
                        transform.Scale = (Vector3)converter.ConvertFromInvariantString(context, components[0]);
                    if (components.Length > 1 && !string.IsNullOrEmpty(components[1]))
                        transform.Rotation = (Vector3)converter.ConvertFromInvariantString(context, components[1]);
                    if (components.Length > 2 && !string.IsNullOrEmpty(components[2]))
                        transform.Position = (Vector3)converter.ConvertFromInvariantString(context, components[2]);
                    if (components.Length > 3 && !string.IsNullOrEmpty(components[3]))
                        transform.RotationOrder = (RotationOrder)Enum.Parse(typeof(RotationOrder), components[3]);
                    return transform;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Invalid transform format.", e);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
