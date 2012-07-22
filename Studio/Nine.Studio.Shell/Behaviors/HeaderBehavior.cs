namespace Nine.Studio.Shell.Behaviors
{
    using System.Windows;

    public static class HeaderBehavior
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached(
            "Header",
            typeof(string),
            typeof(HeaderBehavior),
            new UIPropertyMetadata(""));

        public static string GetHeader(DependencyObject obj)
        {
            return (string)obj.GetValue(HeaderProperty);
        }

        public static void SetHeader(DependencyObject obj, string value)
        {
            obj.SetValue(HeaderProperty, value);
        }
    }
}
