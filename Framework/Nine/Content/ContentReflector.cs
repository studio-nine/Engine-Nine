#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Content
{
    class ContentMember
    {
        PropertyInfo property;
        FieldInfo field;

        public string Name
        {
            get { return property != null ? property.Name : field.Name; }
        }

        public Type MemberType
        {
            get { return property != null ? property.PropertyType : field.FieldType; }
        }

        public object GetValue(object target)
        {
            if (property != null)
                return property.GetValue(target, null);
            return field.GetValue(target);
        }

        public void SetValue(object target, object value)
        {
            if (property != null)
                property.SetValue(target, value, null);
            else
                field.SetValue(target, value);
        }

        public ContentMember(PropertyInfo property)
        {
            this.property = property;
        }

        public ContentMember(FieldInfo field)
        {
            this.field = field;
        }

        public override string ToString()
        {
            return MemberType.Name + " " + Name;
        }
    }

    static class ContentReflector
    {
        public static IEnumerable<ContentMember> FindSerializableMembers(Type type)
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var properties = type.GetProperties(bindingFlags);
            var fields = type.GetFields(bindingFlags);

            return (from property in properties
                    where IsContentSerializable(property)
                    select new ContentMember(property))

            .Concat(from field in fields
                    where IsContentSerializable(field)
                    select new ContentMember(field));
        }

        private static bool IsContentSerializable(PropertyInfo property)
        {
            if (property.GetIndexParameters().Length > 0 || HasContentSerializableIgnore((MemberInfo)property))
                return false;
            if (property.GetGetMethod() != null && (IsCollection(property.PropertyType) || IsDictionary(property.PropertyType)))
                return true;
            return (property.GetGetMethod() != null && property.GetSetMethod() != null) || HasContentSerializable(property);
        }

        private static bool IsContentSerializable(FieldInfo field)
        {
            return (field.IsPublic || HasContentSerializable(field)) && !HasContentSerializableIgnore((MemberInfo)field);
        }

        private static bool HasContentSerializableIgnore(MemberInfo member)
        {
            return member.GetCustomAttributes(false).OfType<ContentSerializerIgnoreAttribute>().Any();
        }

        private static bool HasContentSerializable(MemberInfo member)
        {
            return member.GetCustomAttributes(false).OfType<ContentSerializerAttribute>().Any();
        }

        private static bool IsCollection(Type type)
        {
            return !IsDictionary(type) && !type.Name.Contains("ReadOnly") && 
                   (typeof(ICollection).IsAssignableFrom(type) ||
                    type.FindInterfaces((t, o) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>), null).Length > 0);
        }

        private static bool IsDictionary(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type) || FindDictionaryInterface(type) != null;
        }

        private static Type FindDictionaryInterface(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return type;
            return type.FindInterfaces((t, o) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>), null).FirstOrDefault();
        }
    }
}
