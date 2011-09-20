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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Collections;
using System.Xml;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Reflection;
#endregion

namespace Nine
{

#if WINDOWS_PHONE || XBOX
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    public class SerializableAttribute : Attribute { }
#endif

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Serialization
    {
        /// <summary>
        /// Gets a collection of known attribute overrides assemblies.
        /// </summary>
        public static XmlAttributeOverrides KnownAttributeOverrides { get; private set; }

        /// <summary>
        /// Gets a collection of known assemblies.
        /// Types marked as Serializable in the known assemblies can be serialized and
        /// deserialized using <c>Save</c> and <c>FromFile</c>.
        /// </summary>
        public static ICollection<Assembly> KnownAssemblies { get { return KnownAssemblies; } }

        /// <summary>
        /// Gets a collection of known types that can be serialized and
        /// deserialized using <c>Save</c> and <c>FromFile</c>.
        /// </summary>
        public static ICollection<Type> KnownTypes { get { return knownTypes; } }

        private static List<Assembly> knownAssemblies = new List<Assembly>();
        private static List<Type> knownTypes = new List<Type>();
        private static Dictionary<Assembly, List<Type>> knownTypesDictionary = new Dictionary<Assembly, List<Type>>();
        private static XmlAttributeOverrides knownAttributeOverrides = new XmlAttributeOverrides();
        
        /// <summary>
        /// Gets the serializer for the specified type.
        /// </summary>
        internal static XmlSerializer CreateSerializer(Type type)
        {
            return new XmlSerializer(type, KnownAttributeOverrides, GetExtraTypes(), null, null);
        }

        private static Type[] GetExtraTypes()
        {
            var extraTypes = knownTypes.Concat(knownAssemblies.SelectMany(assembly =>
            {
                List<Type> types;
                if (!knownTypesDictionary.TryGetValue(assembly, out types))
                    knownTypesDictionary[assembly] = types = assembly.FindSerializableTypes();
                return types;
            })).ToArray();
            return extraTypes;
        }

        private static List<Type> FindSerializableTypes(this Assembly assembly)
        {
            List<Type> types = new List<Type>();
            foreach (var type in assembly.GetTypes().Where(x => IsTypeSerializable(x)))
            {
                var attibutes = type.GetCustomAttributes(typeof(SerializableAttribute), false);
                if (attibutes != null && attibutes.Length > 0)
                    types.Add(type);
            }
            return types;
        }

        private static bool IsTypeSerializable(Type type)
        {
            TypeAttributes canHaveFlags = TypeAttributes.Public | TypeAttributes.NestedPublic;
            TypeAttributes cannotHaveFlags = TypeAttributes.Interface | TypeAttributes.Abstract;

            return !type.ContainsGenericParameters &&
                   (type.Attributes & canHaveFlags) != 0 &&
                   (type.Attributes & cannotHaveFlags) == 0;
        }
        
        static Serialization()
        {
#if WINDOWS
            knownAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(
                assembly =>
                    !assembly.FullName.StartsWith("Microsoft.Xna.Framework") &&
                    !assembly.FullName.StartsWith("System") &&
                    !assembly.FullName.StartsWith("mscorlib")));
#else
            knownAssemblies.Add(Assembly.GetExecutingAssembly());
#endif
            foreach (var assembly in knownAssemblies.Distinct())
            {
                knownTypesDictionary.Add(assembly, assembly.FindSerializableTypes());
            }

            knownAttributeOverrides = new XmlAttributeOverrides();
            knownAttributeOverrides.Add(typeof(Matrix), "Up", new XmlAttributes() { XmlIgnore = true });
            knownAttributeOverrides.Add(typeof(Matrix), "Down", new XmlAttributes() { XmlIgnore = true });
            knownAttributeOverrides.Add(typeof(Matrix), "Left", new XmlAttributes() { XmlIgnore = true });
            knownAttributeOverrides.Add(typeof(Matrix), "Right", new XmlAttributes() { XmlIgnore = true });
            knownAttributeOverrides.Add(typeof(Matrix), "Forward", new XmlAttributes() { XmlIgnore = true });
            knownAttributeOverrides.Add(typeof(Matrix), "Backward", new XmlAttributes() { XmlIgnore = true });
            knownAttributeOverrides.Add(typeof(Matrix), "Translation", new XmlAttributes() { XmlIgnore = true });
        }
    }
}