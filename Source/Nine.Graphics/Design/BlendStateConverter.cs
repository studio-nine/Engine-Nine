namespace Nine.Graphics.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Microsoft.Xna.Framework.Graphics;

    class BlendStateConverter : TypeConverter
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
            if (destinationType == typeof(string) && value is BlendState)
            {
                var blendState = (BlendState)value;
                if (BlendStateEquals(blendState, BlendState.Additive))
                    return "Additive";
                if (BlendStateEquals(blendState, BlendState.AlphaBlend))
                    return "AlphaBlend";
                if (BlendStateEquals(blendState, BlendState.NonPremultiplied))
                    return "NonPremultiplied";
                if (BlendStateEquals(blendState, BlendState.Opaque))
                    return "Opaque";

                throw new InvalidOperationException("Unknown BlendState: " + value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string blendState = value.ToString();
                if (blendState == "Additive")
                    return BlendState.Additive;
                if (blendState == "AlphaBlend")
                    return BlendState.AlphaBlend;
                if (blendState == "NonPremultiplied")
                    return BlendState.NonPremultiplied;
                if (blendState == "Opaque")
                    return BlendState.Opaque;

                throw new InvalidOperationException("Unknown BlendState: " + value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        private static bool BlendStateEquals(BlendState a, BlendState b)
        {
            return a.AlphaBlendFunction == b.AlphaBlendFunction &&
                   a.AlphaDestinationBlend == b.AlphaDestinationBlend &&
                   a.AlphaSourceBlend == b.AlphaSourceBlend &&
                   a.BlendFactor == b.BlendFactor &&
                   a.ColorBlendFunction == b.ColorBlendFunction &&
                   a.ColorDestinationBlend == b.ColorDestinationBlend &&
                   a.ColorSourceBlend == b.ColorSourceBlend &&
                   a.ColorWriteChannels == b.ColorWriteChannels &&
                   a.MultiSampleMask == b.MultiSampleMask;
        }
    }
}
