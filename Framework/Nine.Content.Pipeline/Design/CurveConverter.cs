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
    class CurveConverter : TypeConverter
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
            if (destinationType == typeof(string))
            {
                if (value is LinearCurveContent)
                    return "Linear";
                if (value is SinCurveContent)
                    return "Sin";
                if (value is SmoothCurveContent)
                    return "Smooth";
                if (value is ExponentialCurveContent)
                    return "Exponential";
                if (value is ElasticCurveContent)
                    return "Elastic";
                if (value is BounceCurveContent)
                    return "Bounce";
                
                throw new InvalidContentException("Unknown curve: " + value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string curve = value.ToString();
                if (curve == "Linear")
                    return new LinearCurveContent();
                if (curve == "Sin")
                    return new SinCurveContent();
                if (curve == "Smooth")
                    return new SmoothCurveContent();
                if (curve == "Exponential")
                    return new ExponentialCurveContent();
                if (curve == "Elastic")
                    return new ElasticCurveContent();
                if (curve == "Bounce")
                    return new BounceCurveContent();

                throw new InvalidContentException("Unknown curve: " + value);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
