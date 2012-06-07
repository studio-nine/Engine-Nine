#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Windows;
using System.Windows.Controls;
using Nine.Studio.Shell.ViewModels;
#endregion

namespace Nine.Studio.Shell.Windows.Controls
{
    public class PropertyListViewStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var property = item as PropertyView;
            var element = container as FrameworkElement;

            Style style = null;
            if (element != null && property != null)
            {
                Type type = property.PropertyType;

                while (type != null && (style = element.TryFindResource(type) as Style) == null)
                {
                    type = type.BaseType;
                }
            }
            return style ?? base.SelectStyle(item, container);
        }
    }

    public class PropertyMenuStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var property = item as PropertyView;
            var element = container as FrameworkElement;

            Style style = null;
            if (element != null && property != null)
            {
                Type type = property.PropertyType;

                while (type != null && (style = element.TryFindResource(type) as Style) == null)
                {
                    type = type.BaseType;
                }
            }
            return style ?? base.SelectStyle(item, container);
        }
    }

    public class ViewMenuStyleSelector : PropertyMenuStyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is VisualizerView)
            {
                return App.Current.TryFindResource("DocumentVisualizerMenuStyle") as Style;
            }
            return base.SelectStyle(item, container);
        }
    }
}
