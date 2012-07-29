namespace Nine.Graphics.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Microsoft.Xna.Framework.Graphics;

    class SamplerStateConverter : TypeConverter
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
            if (destinationType == typeof(string) && value is SamplerState)
            {
                var samplerState = (SamplerState)value;
                if (SamplerStateEquals(samplerState, SamplerState.AnisotropicClamp))
                    return "AnisotropicClamp";
                if (SamplerStateEquals(samplerState, SamplerState.AnisotropicWrap))
                    return "AnisotropicWrap";
                if (SamplerStateEquals(samplerState, SamplerState.LinearClamp))
                    return "LinearClamp";
                if (SamplerStateEquals(samplerState, SamplerState.LinearWrap))
                    return "LinearWrap";
                if (SamplerStateEquals(samplerState, SamplerState.PointClamp))
                    return "PointClamp";
                if (SamplerStateEquals(samplerState, SamplerState.PointWrap))
                    return "PointWrap";

                throw new InvalidOperationException("Unknown SamplerState: " + value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string samplerState = value.ToString();
                if (samplerState == "AnisotropicClamp")
                    return SamplerState.AnisotropicClamp;
                if (samplerState == "AnisotropicWrap")
                    return SamplerState.AnisotropicWrap;
                if (samplerState == "LinearClamp")
                    return SamplerState.LinearClamp;
                if (samplerState == "LinearWrap")
                    return SamplerState.LinearWrap;
                if (samplerState == "PointClamp")
                    return SamplerState.PointClamp;
                if (samplerState == "PointWrap")
                    return SamplerState.PointWrap;

                throw new InvalidOperationException("Unknown SamplerState: " + value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        private static bool SamplerStateEquals(SamplerState a, SamplerState b)
        {
            return a.AddressU == b.AddressU &&
                   a.AddressV == b.AddressV &&
                   a.AddressW == b.AddressW &&
                   a.Filter == b.Filter &&
                   a.MaxAnisotropy == b.MaxAnisotropy &&
                   a.MaxMipLevel == b.MaxMipLevel &&
                   a.MipMapLevelOfDetailBias == b.MipMapLevelOfDetailBias;
        }
    }
}
