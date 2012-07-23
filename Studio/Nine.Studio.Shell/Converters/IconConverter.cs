namespace Nine.Studio.Shell.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    public class IconConverter : IValueConverter
    {
        const int IconSize = 16;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Image)
            {
                ((Image)value).Width = IconSize;
                ((Image)value).Height = IconSize;
                return value;
            }

            if (value is ImageSource)
                return new Image() { Source = (ImageSource)value, Width=IconSize, Height=IconSize };

            if (value is System.Drawing.Bitmap)
            {
                Image image = new Image() { Width = IconSize, Height = IconSize };
                System.Drawing.Bitmap source = (System.Drawing.Bitmap)value;
                image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                               System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                return image;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
