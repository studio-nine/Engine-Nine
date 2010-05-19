#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
#endregion

namespace Nine.Design
{
    public class TypeScanner
    {
        public Assembly[] References { get; private set; }
        
        public TypeScanner(Assembly[] references)
        {
            if (references == null)
                throw new ArgumentNullException();
            
            References = references;
        }

        public IEnumerable<Type> GetAllTypes()
        {
            foreach (Assembly asm in References)
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.GetConstructor(Type.EmptyTypes) != null)
                        yield return type;
                }
            }
        }

        public IEnumerable<Type> GetDerivedTypes(Type baseType)
        {
            foreach (Type type in GetAllTypes())
            {
                if (baseType.IsAssignableFrom(type))
                {
                    yield return type;
                }
            }
        }
    }


    public static class TypeExtensions
    {
        public static T GetAttribute<T>(this Type type, bool inherit) where T : class
        {
            object[] attributes = type.GetCustomAttributes(typeof(T), inherit);

            return attributes.Length > 0 ? attributes[0] as T : null;
        }
    }
}