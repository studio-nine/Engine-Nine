namespace Nine.Serialization.CodeGeneration
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
            if (args.Name == "ILRepack, Version=1.20.0.0, Culture=neutral, PublicKeyToken=null")
                return Assembly.Load(EmbeddedAssemblies.ILRepack);
            return null;
        }
    }
}
