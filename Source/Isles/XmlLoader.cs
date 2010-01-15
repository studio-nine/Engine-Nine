#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles
{
    public interface IXmlLoader
    {
        object Load(XmlElement input, IServiceProviderEx services);
    }


    public sealed class XmlLoaderAttribute : Attribute
    {
        public string Name { get; set; }

        public XmlLoaderAttribute(string name)
        {
            Name = name;
        }

        public XmlLoaderAttribute(Type type)
        {
            Name = type.Name;
        }
    }


    public sealed class XmlLoader : IXmlLoader
    {
        #region Standard Loaders
        static IDictionary<string, IXmlLoader> standardLoaders = null;

        public static IDictionary<string, IXmlLoader> StandardLoaders
        {
            get 
            {
                if (standardLoaders == null)
                    InitializeStandardLoaders();

                return standardLoaders; 
            }
        }


        private static void InitializeStandardLoaders()
        {
            List<Type> types = new List<Type>(32);
            List<Assembly> assemblies = new List<Assembly>(16);
            List<string> assemblyNames = new List<string>(16);
            Stack<Assembly> stack = new Stack<Assembly>(16);
            

            stack.Push(Assembly.GetEntryAssembly());


            while (stack.Count > 0)
            {
                Assembly asm = stack.Pop();

                assemblies.Add(asm);

                foreach (AssemblyName name in asm.GetReferencedAssemblies())
                {
                    if (name.FullName.StartsWith("System") ||
                        name.FullName.StartsWith("Microsoft") ||
                        name.FullName.StartsWith("mscorlib"))
                        continue;

                    if (!assemblyNames.Contains(name.FullName))
                    {
                        assemblyNames.Add(name.FullName);

                        stack.Push(Assembly.Load(name.FullName));
                    }
                }
            }


            foreach (Assembly assembly in assemblies)
                foreach (Type type in assembly.GetTypes())
                    types.Add(type);


            standardLoaders = LoaderFromTypes(types.ToArray());
        }

        private static Dictionary<string, IXmlLoader> LoaderFromTypes(Type[] types)
        {
            Dictionary<string, IXmlLoader> result = new Dictionary<string, IXmlLoader>();

            foreach (Type type in types)
            {
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);

                object[] attributes = type.GetCustomAttributes(typeof(XmlLoaderAttribute), false);

                if (constructor != null &&
                    attributes != null && attributes.Length == 1 &&
                    type.GetInterface(typeof(IXmlLoader).Name) != null)
                {
                    string name = (attributes[0] as XmlLoaderAttribute).Name;

                    if (!string.IsNullOrEmpty(name))
                    {
                        IXmlLoader loader = constructor.Invoke(null) as IXmlLoader;

                        if (loader != null)
                            result.Add(name, loader);
                    }
                }
            }

            return result;
        }
        #endregion


        public T Load<T>(XmlElement input, IServiceProviderEx services)
        {
            return (T)Load(input, services);
        }


        public object Load(XmlElement input, IServiceProviderEx services)
        {
            if (StandardLoaders == null)
                InitializeStandardLoaders();

            return Load(input, services, StandardLoaders);
        }


        public object Load(XmlElement input, IServiceProviderEx services, Type[] loaders)
        {
            return Load(input, services, LoaderFromTypes(loaders));
        }


        public object Load(XmlElement input, IServiceProviderEx services, IDictionary<string, IXmlLoader> loaders)
        {
            services.AddService<IXmlLoader>(null, this);

            IXmlLoader objectLoader = null;

            if (!loaders.TryGetValue(input.Name, out objectLoader))
                return null;

            if (objectLoader != null)
                return objectLoader.Load(input, services);

            return null;
        }
    }
}