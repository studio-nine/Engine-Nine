#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Windows.Data;
using System.Windows.Media;

#endregion

namespace Nine.Studio.Shell.Converters
{
    public class ColorToInvertBrushConverter : IValueConverter
    {
        static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Color.FromRgb(192, 192, 192));
        static readonly SolidColorBrush BlackBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var color = (Color)value;
            return color.R * 0.299f + color.G * 0.587f + color.B * 0.114f > 128 ? BlackBrush : WhiteBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
