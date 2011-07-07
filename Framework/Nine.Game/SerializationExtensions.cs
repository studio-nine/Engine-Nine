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

    static class SerializationExtensions
    {
        public static List<Type> FindSerializableTypes(this Assembly assembly)
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

        public static XmlAttributeOverrides XmlAttributeOverrides { get; private set; }

        static SerializationExtensions()
        {
            XmlAttributeOverrides = new XmlAttributeOverrides();
            XmlAttributeOverrides.Add(typeof(Matrix), "Up", new XmlAttributes() { XmlIgnore = true });
            XmlAttributeOverrides.Add(typeof(Matrix), "Down", new XmlAttributes() { XmlIgnore = true });
            XmlAttributeOverrides.Add(typeof(Matrix), "Left", new XmlAttributes() { XmlIgnore = true });
            XmlAttributeOverrides.Add(typeof(Matrix), "Right", new XmlAttributes() { XmlIgnore = true });
            XmlAttributeOverrides.Add(typeof(Matrix), "Forward", new XmlAttributes() { XmlIgnore = true });
            XmlAttributeOverrides.Add(typeof(Matrix), "Backward", new XmlAttributes() { XmlIgnore = true });
            XmlAttributeOverrides.Add(typeof(Matrix), "Translation", new XmlAttributes() { XmlIgnore = true });
        }

        public static Type[] FindUnknownTypes(object serializable)
        {
            List<Type> types = new List<Type>();
            FindUnknownTypes(serializable, types);
            return types.Where(type => !type.IsPrimitive && type != typeof(string)).ToArray();
        }

        private static void FindUnknownTypes(object serializable, List<Type> types)
        {
            if (serializable is IXmlSerializable)
                return;

            if (serializable is IEnumerable)
            {
                foreach (var item in (IEnumerable)serializable)
                {
                    if (item != null)
                        types.Add(item.GetType());
                }
                return;
            }

            Type type = serializable.GetType();

            foreach (var field in type.GetFields())
            {
                object value = field.GetValue(serializable);
                if (value != null && value.GetType() != field.FieldType)
                {
                    types.Add(value.GetType());
                }
                if (value is IEnumerable)
                    FindUnknownTypes(value, types);
            }

            foreach (var property in type.GetProperties())
            {
                object value = property.GetValue(serializable, null);
                if (value != null && value.GetType() != property.PropertyType)
                {
                    types.Add(value.GetType());
                }
                if (value is IEnumerable)
                    FindUnknownTypes(value, types);
            }
        }
    }
}