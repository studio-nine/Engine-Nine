namespace Nine
{
    using System;
    using System.Reflection;

    static class Embedded
    {
        public static void EnsureAssembliesInitialized()
        {
            if (!handlerInstalled)
            {
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
                handlerInstalled = true;
            }
        }

        static bool handlerInstalled;
        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == "Ionic.Zip.Reduced, Version=1.9.1.8, Culture=neutral, PublicKeyToken=edbe51ad942a3f5c")
                return Assembly.Load(EmbeddedAssemblies.Ionic_Zip_Reduced);
            return null;
        }
    }
}
