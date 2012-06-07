#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio
{
    internal static class EditorHelper
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (T item in values)
                collection.Add(item);
        }

        public static void ForEach<T>(this IList<T> collection, Action<T> action)
        {
            // Avoid modify collection during enumeration
            for (int i = 0; i < collection.Count; i++)
                action(collection[i]);
        }

        public static string GetDisplayName(this Editor editor, object target)
        {
            var attribute = editor.Extensions.GetCustomAttribute(target.GetType()).OfType<DisplayNameAttribute>().FirstOrDefault();
            if (attribute != null)
                return attribute.DisplayName;
            attribute = target.GetType().GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault();
            return attribute != null ? attribute.DisplayName : target.GetType().Name;
        }

        public static string GetCategory(this Editor editor, object target)
        {
            var attribute = editor.Extensions.GetCustomAttribute(target.GetType()).OfType<CategoryAttribute>().FirstOrDefault();
            if (attribute != null)
                return attribute.Category;
            attribute = target.GetType().GetCustomAttributes(false).OfType<CategoryAttribute>().FirstOrDefault();
            return attribute != null ? attribute.Category : Strings.General;
        }

        public static string GetDescription(this Editor editor, object target)
        {
            var attribute = editor.Extensions.GetCustomAttribute(target.GetType()).OfType<DescriptionAttribute>().FirstOrDefault();
            if (attribute != null)
                return attribute.Description;
            attribute = target.GetType().GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault();
            return attribute != null ? attribute.Description : "";
        }

        public static string GetFolderName(this Editor editor, object target)
        {
            var attribute = editor.Extensions.GetCustomAttribute(target.GetType()).OfType<FolderNameAttribute>().FirstOrDefault();
            if (attribute != null)
                return attribute.FolderName;
            attribute = target.GetType().GetCustomAttributes(false).OfType<FolderNameAttribute>().FirstOrDefault();
            return attribute != null ? attribute.FolderName : "Misc";
        }
    }
}
