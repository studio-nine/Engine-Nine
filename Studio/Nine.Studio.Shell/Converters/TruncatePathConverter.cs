#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;
#endregion

namespace Nine.Studio.Shell.Converters
{
    public class TruncatePathConverter : IValueConverter
    {
        /// <summary>
        /// http://www.pinvoke.net/default.aspx/shlwapi/PathCompactPathEx.html
        /// </summary>
        static string TruncatePath(string path, int length)
        {
            StringBuilder sb = new StringBuilder(length + 1);
            PathCompactPathEx(sb, path, length + 1, 0);
            return sb.ToString();
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        public int Length { get; set; }

        public TruncatePathConverter()
        {
            Length = 30;
        }
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return TruncatePath(value.ToString(), Length);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
