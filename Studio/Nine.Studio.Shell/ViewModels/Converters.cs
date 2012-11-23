namespace Nine.Studio.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public static class Converters
    {
        public static object ToView(this object viewModel)
        {
            return ToView(viewModel, null);
        }

        public static object ToView(this object viewModel, string viewName)
        {
            if (viewModel == null)
                return null;

            if (viewModel is UIElement)
                return viewModel;

            var viewLocator = string.IsNullOrEmpty(viewName) ?
                string.Concat(typeof(App).Namespace, ".", viewModel.GetType().Name, "View") :
                string.Concat(typeof(App).Namespace, ".", viewName);
            
            object component;
            var type = Type.GetType(viewLocator);
            if (type != null)
            {
                component = Activator.CreateInstance(type);
            }
            else
            {
                viewLocator = string.IsNullOrEmpty(viewName) ? viewModel.GetType().Name + "View" : viewName;
                component = Application.LoadComponent(new Uri(@"/Nine.Studio.Shell;component/views/" + viewLocator + ".xaml", UriKind.Relative));
            }

            var frameworkElement = component as FrameworkElement;
            if (frameworkElement != null)
                frameworkElement.DataContext = viewModel;

            return component;
        }

        public static object EnumValues(object value)
        {
            return Enum.GetValues(value.GetType());
        }

        public static object ToLower(object value)
        {
            return value != null ? value.ToString().ToLowerInvariant() : "";
        }

        public static object ToUpper(object value)
        {
            return value != null ? value.ToString().ToUpperInvariant() : "";
        }

        public static object HasAny(object value, Type targetType)
        {
            if (targetType == typeof(bool))
                return System.Convert.ToInt32(value) > 0;
            return System.Convert.ToInt32(value) > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public static object IsNull(object value, Type targetType)
        {
            if (targetType == typeof(bool))
                return value == null;
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public static object IsNotNull(object value, Type targetType)
        {
            if (targetType == typeof(bool))
                return value != null;
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public static object IsNeitherNullNorEmpty(object value, Type targetType)
        {
            if (targetType == typeof(bool))
                return !string.IsNullOrEmpty(value.ToString());
            return string.IsNullOrEmpty(value.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static object TruncatePath(object value)
        {
            return value != null ? TruncatePath(value.ToString(), 32) : null;
        }

        public static object ToFileName(object value)
        {
            return value != null ? Path.GetFileNameWithoutExtension(value.ToString()) : "";
        }

        /// <summary>
        /// http://www.pinvoke.net/default.aspx/shlwapi/PathCompactPathEx.html
        /// </summary>
        private static string TruncatePath(string path, int length)
        {
            StringBuilder sb = new StringBuilder(length + 1);
            PathCompactPathEx(sb, path, length + 1, 0);
            return sb.ToString();
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);


        public static object InvertColor(object value, Type targetType)
        {
            var color = (Color)value;
            var black = color.R * 0.299f + color.G * 0.587f + color.B * 0.114f > 128;
            if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
                return black ? BlackBrush : WhiteBrush;
            return black ? BlackColor : WhiteColor;
        }

        private static readonly Color WhiteColor = Color.FromRgb(192, 192, 192);
        private static readonly Color BlackColor = Color.FromRgb(64, 64, 64);
        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(WhiteColor);
        private static readonly SolidColorBrush BlackBrush = new SolidColorBrush(BlackColor);


        public static object ToIcon(object value)
        {
            if (value is Image)
            {
                ((Image)value).Width = IconSize;
                ((Image)value).Height = IconSize;
                return value;
            }

            if (value is ImageSource)
                return new Image() { Source = (ImageSource)value, Width = IconSize, Height = IconSize };

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

        const int IconSize = 16;

        
        public static object ToControl(object value)
        {
            /*
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
            return null;
        }
        private static NoDesignView NoDesignView;
    }
}
