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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
    public sealed class LoaderAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsContent { get; set; }
        public bool IsService { get; set; }
        public Type Serializer { get; set; }

        public LoaderAttribute() { }
        public LoaderAttribute(string name)
        {
            Name = name;
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LoaderIgnoreAttribute : Attribute { }


    [XmlLoader("Object")]
    internal sealed class XmlReflectiveLoader : IXmlLoader
    {
        ContentManager Content = null;
        IServiceProviderEx Services = null;

        public object Load(XmlElement input, IServiceProviderEx services)
        {
            string typeName;

            if (string.IsNullOrEmpty(typeName = input.GetAttribute("Type")))
                return null;

            Type type = Type.GetType(typeName);

            // Convert from inner xml to basic types
            if (type.IsPrimitive || type.IsEnum || type.IsValueType)
                return GetObjectValue(type, input, null);


            // Reflect each field for complex types
            object instance = Activator.CreateInstance(type);

            Services = services;
            Content = services.GetService<ContentManager>(null);

            Dictionary<string, string> nameRemap = BuildNameRemap(type);
            
            foreach (XmlNode child in input.ChildNodes)
            {
                XmlElement element = child as XmlElement;

                if (element != null)
                {
                    string name;

                    if (!nameRemap.TryGetValue(element.Name, out name))
                        name = element.Name;

                    if (!SetField(instance, name, element))
                        throw new InvalidDataException("Invalid Xml Element: " + element.Name);
                }
            }

            return instance;
        }

        private Dictionary<string, string> BuildNameRemap(Type type)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (MemberInfo member in type.GetMembers(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                LoaderAttribute loaderInfo = GetLoaderAttribute(member);

                if (loaderInfo != null && !string.IsNullOrEmpty(loaderInfo.Name))
                    result.Add(loaderInfo.Name, member.Name);
            }

            return result;
        }

        private bool SetField(object instance, string propertyName, XmlElement input)
        {
            if (SetProperty(instance, propertyName, input))
                return true;

            if (SetMember(instance, propertyName, input))
                return true;

            return false;
        }

        private bool SetMember(object instance, string propertyName, XmlElement input)
        {
            FieldInfo member = instance.GetType().GetField(
                propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (member == null || member.IsStatic)
                return false;

            if (member.IsDefined(typeof(LoaderIgnoreAttribute), true))
                return false;

            LoaderAttribute loaderInfo = GetLoaderAttribute(member);

            if (loaderInfo == null)
                return false;

            member.SetValue(instance,
                GetObjectValue(member.FieldType, input, loaderInfo));

            return true;
        }

        private bool SetProperty(object instance, string propertyName, XmlElement input)
        {
            PropertyInfo property = instance.GetType().GetProperty(
                propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


            // Ignore non-get and settable properties
            if (property == null)
                return false;

            // Ignore static and non-settable properties
            MethodInfo set = property.GetSetMethod();
            MethodInfo get = property.GetGetMethod();

            if (set == null || set.IsStatic || get == null || get.IsStatic)
                return false;


            // Ignore LoaderIgnore properties
            if (property.IsDefined(typeof(LoaderIgnoreAttribute), true))
                return false;


            // Get loader property
            LoaderAttribute loaderInfo = GetLoaderAttribute(property);

            
            // Ignore non-public properties
            if (loaderInfo == null && (!set.IsPublic || !get.IsPublic))
                return false;


            // Set property value
            property.SetValue(instance, 
                GetObjectValue(property.PropertyType, input, loaderInfo), null);

            return true;
        }

        private LoaderAttribute GetLoaderAttribute(MemberInfo member)
        {
            object[] attributes = member.GetCustomAttributes(typeof(LoaderAttribute), true);

            if (attributes != null && attributes.Length == 1)
                return attributes[0] as LoaderAttribute;

            return null;
        }

        private object GetObjectValue(Type type, XmlElement input, LoaderAttribute loaderInfo)
        {
            if (loaderInfo != null)
            {
                if (loaderInfo.IsContent && Content != null)
                {
                    return Content.Load<object>(input.InnerXml);
                }

                if (loaderInfo.IsService && Services != null)
                {
                    return Services.GetService<object>(input.InnerXml);
                }

                if (loaderInfo.Serializer != null)
                {
                    IXmlLoader loader;

                    if ((loader = Activator.CreateInstance(loaderInfo.Serializer) as IXmlLoader) != null)
                        return loader.Load(input, Services);
                }
            }


            if (type.IsPrimitive)
                return Convert.ChangeType(input.InnerXml, type, null);

            if (type.IsEnum)
                return Enum.Parse(type, input.InnerXml);

            if (type == typeof(Matrix))
                return ParseHelper.ToMatrix(input.InnerXml);

            if (type == typeof(Quaternion))
                return ParseHelper.ToQuaternion(input.InnerXml);

            if (type == typeof(Vector2))
                return ParseHelper.ToVector2(input.InnerXml);

            if (type == typeof(Vector3))
                return ParseHelper.ToVector3(input.InnerXml);

            if (type == typeof(Vector4))
                return ParseHelper.ToVector4(input.InnerXml);

            if (type == typeof(Color))
                return ParseHelper.ToColor(input.InnerXml);

            if (type == typeof(Point))
                return ParseHelper.ToPoint(input.InnerXml);

            if (type == typeof(Rectangle))
                return ParseHelper.ToRectangle(input.InnerXml);

            return null;
        }

        private object GetCollectionValue(XmlElement input)
        {
            throw new NotImplementedException();
        }
    }
}