#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows;

#endregion

namespace Nine.Studio.Shell.Behaviors
{
    public static class ResourceBehavior
    {
        public static object GetResource(DependencyObject obj)
        {
            return obj.GetValue(ResourceProperty);
        }

        public static void SetResource(DependencyObject obj, object value)
        {
            obj.SetValue(ResourceProperty, value);
        }

        public static readonly DependencyProperty ResourceProperty =
            DependencyProperty.RegisterAttached("Resource", typeof(object), typeof(ResourceBehavior), new UIPropertyMetadata(null, OnResourceChanged));


        private static void OnResourceChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var itemsControl = sender as FrameworkElement;
            
            if (itemsControl == null) return;

            var style = args.OldValue as Style;
            if (style != null)
                itemsControl.Resources.Remove(style.TargetType);

            style = args.NewValue as Style;
            if (style != null)
                itemsControl.Resources.Add(style.TargetType, style);
        }
    }
}
