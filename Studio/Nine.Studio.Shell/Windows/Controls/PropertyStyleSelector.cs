#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Shell;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Win32;
using Nine.Studio.Shell.Windows;
using Nine.Studio.Extensibility;
using WPG.Data;
#endregion

namespace Nine.Studio.Shell.Windows.Controls
{
    public class PropertyStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var property = item as Property;
            var element = container as FrameworkElement;

            if (element != null && property != null)
            {
                var style = element.TryFindResource(property.PropertyType) as Style;
                if (style != null)
                    return style;
            }
            return base.SelectStyle(item, container);
        }
    }
}
