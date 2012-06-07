#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Nine.Studio.Shell.Behaviors
{
    /// <summary>
    /// http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/73ac639f-afdc-4582-805d-9b3bc8a33fe5/
    /// </summary>
    public static class GroupStyleBehavior
    {
        public static GroupStyle GetGroupStyle(DependencyObject obj)
        {
            return (GroupStyle)obj.GetValue(GroupStyleProperty);
        }

        public static void SetGroupStyle(DependencyObject obj, GroupStyle value)
        {
            obj.SetValue(GroupStyleProperty, value);
        }

        public static readonly DependencyProperty GroupStyleProperty =
            DependencyProperty.RegisterAttached("GroupStyle", typeof(GroupStyle), typeof(GroupStyleBehavior), new UIPropertyMetadata(null, OnGroupStyleChanged));
        
        private static void OnGroupStyleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var itemsControl = sender as ItemsControl;
            
            if (itemsControl == null) return;

            var groupStyle = args.OldValue as GroupStyle;
            if (groupStyle != null)
                itemsControl.GroupStyle.Remove(groupStyle);

            groupStyle = args.NewValue as GroupStyle;
            if (groupStyle != null)
                itemsControl.GroupStyle.Add(groupStyle);
        }
    }
}
