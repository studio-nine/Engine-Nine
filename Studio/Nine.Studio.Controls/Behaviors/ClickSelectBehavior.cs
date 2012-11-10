namespace Nine.Studio.Shell.Behaviors
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Modified from http://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox
    /// </summary>
    public static class ClickSelectBehavior
    {
        public static readonly DependencyProperty IsClickSelectProperty = DependencyProperty.RegisterAttached(
            "IsClickSelect",
            typeof(bool),
            typeof(ClickSelectBehavior),
            new UIPropertyMetadata(false, OnIsClickSelectChanged));

        private static void OnIsClickSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBoxBase;
            if (textBox != null)
            {
                if ((bool)e.NewValue)
                {
                    textBox.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
                    textBox.AddHandler(UIElement.GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
                    textBox.AddHandler(Control.MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
                }
                else
                {
                    textBox.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
                    textBox.RemoveHandler(UIElement.GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText));
                    textBox.RemoveHandler(Control.MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText));
                }
            }
        }

        public static bool GetIsClickSelect(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsClickSelectProperty);
        }

        public static void SetIsClickSelect(DependencyObject obj, bool value)
        {
            obj.SetValue(IsClickSelectProperty, value);
        }

        private static void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox 
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBoxBase))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBoxBase)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and 
                    // stop further processing of this click event. 
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBoxBase;
            if (textBox != null)
                textBox.SelectAll();
        } 
    }
}
