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
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LoaderIgnoreAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
    public sealed class LoaderAttribute : Attribute
    {
        public string Name { get; set; }

        /// <summary>
        /// Indicates that the field is a content and should be loaded from the specifed ContentManager.
        /// </summary>
        public bool IsContent { get; set; }

        /// <summary>
        /// Indicates that the field is a service and should be initialized from the specfied IServiceProvider.
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// Indicates that the field is should be initialized from the object with the specified name.
        /// </summary>
        public bool IsReference { get; set; }

        /// <summary>
        /// Indicates that the field should be loaded using a custom loader.
        /// </summary>
        public Type Serializer { get; set; }

        public LoaderAttribute() { }
        public LoaderAttribute(string name)
        {
            Name = name;
        }
    }


    [XmlLoader("Object")]
    internal sealed class XmlReflectiveLoader : IXmlLoader
    {
        ContentManager Content = null;
        IServiceProvider Services = null;

        public object Load(XmlElement input, IServiceProvider services)
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
            Content = services.GetService(typeof(ContentManager)) as ContentManager;


            // Add the object to INamedObjectManager if it has a name attribute
            INamedObjectManager nameManager = 
                Services.GetService(typeof(INamedObjectManager)) as INamedObjectManager;

            if (nameManager != null)
            {
                string name;

                if (!string.IsNullOrEmpty(name = input.GetAttribute("Name")))
                    nameManager.Add(name, instance);
            }


            // Process LoaderAttribute of the given type
            Dictionary<string, string> nameRemap;
            List<MemberInfo> serviceFields;

            ProcessLoaderAttribute(type, out nameRemap, out serviceFields);


            // Initialize service fields
            foreach (MemberInfo member in serviceFields)
                SetServiceField(instance, member, services);

            
            // Initialie other fields
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

        private void ProcessLoaderAttribute(Type type, 
                                        out Dictionary<string, string> nameRemap,
                                        out List<MemberInfo> serviceFields)
        {
            serviceFields = new List<MemberInfo>();
            nameRemap = new Dictionary<string, string>();

            foreach (MemberInfo member in type.GetMembers(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                LoaderAttribute loaderInfo = GetLoaderAttribute(member);

                if (loaderInfo != null && !string.IsNullOrEmpty(loaderInfo.Name))
                    nameRemap.Add(loaderInfo.Name, member.Name);

                if (loaderInfo != null && loaderInfo.IsService)
                    serviceFields.Add(member);
            }
        }

        private LoaderAttribute GetLoaderAttribute(MemberInfo member)
        {
            object[] attributes = member.GetCustomAttributes(typeof(LoaderAttribute), true);

            if (attributes != null && attributes.Length == 1)
                return attributes[0] as LoaderAttribute;

            return null;
        }

        private void SetServiceField(object instance, MemberInfo member, IServiceProvider services)
        {
            if (member is FieldInfo)
            {
                FieldInfo field = member as FieldInfo;

                field.SetValue(instance, services.GetService(field.FieldType));
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo property = member as PropertyInfo;

                property.SetValue(instance, services.GetService(property.PropertyType), null);
            }
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

        private object GetObjectValue(Type type, XmlElement input, LoaderAttribute loaderInfo)
        {
            if (loaderInfo != null)
            {
                if (loaderInfo.IsService && Services != null)
                    throw new InvalidDataException("Services cannot be initialized: " + type.Name);

                if (loaderInfo.IsContent && Content != null)
                    return Content.Load<object>(input.InnerXml);

                if (loaderInfo.IsReference && Services != null)
                {
                    INamedObjectManager nameTable = 
                        Services.GetService(typeof(INamedObjectManager)) as INamedObjectManager;

                    if (nameTable != null)
                        return nameTable.GetObjectByName(input.InnerXml);

                    return null;
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
                return ParseExtensions.ToMatrix(input.InnerXml);

            if (type == typeof(Quaternion))
                return ParseExtensions.ToQuaternion(input.InnerXml);

            if (type == typeof(Vector2))
                return ParseExtensions.ToVector2(input.InnerXml);

            if (type == typeof(Vector3))
                return ParseExtensions.ToVector3(input.InnerXml);

            if (type == typeof(Vector4))
                return ParseExtensions.ToVector4(input.InnerXml);

            if (type == typeof(Color))
                return ParseExtensions.ToColor(input.InnerXml);

            if (type == typeof(Point))
                return ParseExtensions.ToPoint(input.InnerXml);

            if (type == typeof(Rectangle))
                return ParseExtensions.ToRectangle(input.InnerXml);

            return null;
        }

        private object GetCollectionValue(XmlElement input)
        {
            throw new NotImplementedException();
        }
    }
}