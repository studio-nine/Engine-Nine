namespace Nine.Studio.Shell.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    public class IntToVisibilityConverter : IValueConverter
    {
        public int VisibleCount { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToInt32(value) > VisibleCount ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
