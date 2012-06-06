#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio
{
    internal static class EditorHelper
    {
        public static bool MatchFileExtension(this IDocumentSerializer serializer, string match)
        {
            if (serializer.FileExtensions == null || serializer.FileExtensions.Count() <= 0)
                return true;
            foreach (var ext in serializer.FileExtensions)
            {
                if (GetNormalizedFileExtension(match) == GetNormalizedFileExtension(ext))
                    return true;
            }
            return false;
        }

        public static string GetNormalizedFileExtension(this string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                return null;
            fileExtension = fileExtension.Trim(' ', '.', '|', '*');
            fileExtension = fileExtension.Insert(0, ".");
            return fileExtension.ToLowerInvariant();
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (T item in values)
                collection.Add(item);
        }
    }
}
