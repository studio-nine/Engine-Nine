#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nine.Studio.Shell
{
    public class Bootstrapper
    {
        [STAThread]
        public static void Main(string[] args)
        {
            EnsureThreadCulture();

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                string resourceName = "Nine.Studio.Shell.References." + new AssemblyName(e.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        return null;
                    }
                    Trace.TraceInformation("Found Embeded assembly: {0}", e.Name);
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            App.Main();
            
            Trace.Flush();
        }

        static CultureInfo CurrentCulture;

        public static void EnsureThreadCulture()
        {
            if (CurrentCulture == null)
            {
                try
                {
                    CurrentCulture = new CultureInfo(Global.GetProperty("Culture"));
                }
                catch
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture;
                }
            }

            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            Nine.Studio.Strings.Culture = CurrentCulture;
            Nine.Studio.Shell.Strings.Culture = CurrentCulture;
        }
    }
}
