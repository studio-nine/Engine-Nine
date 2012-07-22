namespace Nine.Studio.Shell.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Forms.Integration;
    using Nine.Studio.Shell.ViewModels;
    using Nine.Studio.Shell.Windows;


    public class ProjectItemViewToControlConverter : IValueConverter
    {
        NoDesignView noDesignView;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var projectItemView = value as ProjectItemView;
            if (projectItemView == null)
                return null;

            if (projectItemView.ActiveVisualizer != null)
            {
                // We'll create a new visualizer for each document.
                var content = projectItemView.ActiveVisualizer.Visualize();

                if (content is System.Windows.Forms.Control)
                    return new WindowsFormsHost() { Child = (System.Windows.Forms.Control)content };

                if (content is UIElement)
                    return content;
            }
            return noDesignView ?? (noDesignView = new NoDesignView());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
