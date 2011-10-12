#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Xaml;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nine.Content.Pipeline.Graphics;
#endregion

namespace Nine.Content.Pipeline.Design
{
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
                if (BlendStateSerializer.BlendStateEquals(blendState, BlendState.Additive))
                    return "Additive";
                if (BlendStateSerializer.BlendStateEquals(blendState, BlendState.AlphaBlend))
                    return "AlphaBlend";
                if (BlendStateSerializer.BlendStateEquals(blendState, BlendState.NonPremultiplied))
                    return "NonPremultiplied";
                if (BlendStateSerializer.BlendStateEquals(blendState, BlendState.Opaque))
                    return "Opaque";
                
                throw new InvalidContentException("Unknown BlendState: " + value);
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

                throw new InvalidContentException("Unknown BlendState: " + value);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
