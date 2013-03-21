namespace Nine.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;

    class RectangleConverter : Microsoft.Xna.Framework.Design.RectangleConverter
    {
        public RectangleConverter()
        {
            supportStringConvert = true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            if (str != null)
            {
                var components =str.Split(',');
                if (components.Length == 4)
                {
                    return new Microsoft.Xna.Framework.Rectangle(
                        int.Parse(components[0]), int.Parse(components[1]),
                        int.Parse(components[2]), int.Parse(components[3]));
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
