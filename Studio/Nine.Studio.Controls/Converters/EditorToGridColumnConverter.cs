namespace Nine.Studio.Controls.Converters
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Data;

    public class EditorToGridColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is RadioButton || value is CheckBox) ? 0 : 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
