namespace Nine.Studio.Shell.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;


    public class ProjectItemViewToControlConverter : IValueConverter
    {
        //NoDesignView noDesignView;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {/*
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
          */
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
