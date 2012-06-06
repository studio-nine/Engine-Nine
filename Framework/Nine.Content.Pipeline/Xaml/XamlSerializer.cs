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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using System.Collections.Generic;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    /// <summary>
    /// Provides extended Xaml load and save methods.
    /// </summary>
    public class XamlSerializer : IDisposable
    {
        SchemaContext schemaContext;

        /// <summary>
        /// Loads the object graph from a xaml file.
        /// </summary>
        public object Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                return Load(stream);
            }
        }

        /// <summary>
        /// Loads the object graph from a stream.
        /// </summary>
        public object Load(Stream stream)
        {
            var reader = new XamlXmlReader(stream);
            var writer = new ObjectWriter(this, schemaContext = new SchemaContext(this, reader.SchemaContext));
            XamlServices.Transform(reader, writer, false);
            return writer.Result;
        }

        /// <summary>
        /// Saves the specified object to a file.
        /// </summary>
        public void Save(string filename, object value)
        {
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                Save(stream, value);
            }
        }

        /// <summary>
        /// Saves the specified object to a stream.
        /// </summary>
        public void Save(Stream stream, object value)
        {
            var reader = new ObjectReader(this, value, schemaContext);
            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                var writer = new XamlXmlWriter(xmlWriter, reader.SchemaContext);
                XamlServices.Transform(reader, writer, true);
            }
        }

        /// <summary>
        /// Occurs when this serializer failed to create an instance of the specified type and arguments.
        /// </summary>
        public event Func<Type, object[], object> InstanceResolve;

        /// <summary>
        /// Occurs when this serializer failed to create an instance from a content reference.
        /// </summary>
        public event Func<string, object> ContentReferenceResolve;

        /// <summary>
        /// Resolves the instance using InstanceResolve event.
        /// </summary>
        internal object ResolveInstance(Type type, object[] arguments)
        {
            if (InstanceResolve != null)
            {
                foreach (var invocation in InstanceResolve.GetInvocationList())
                {
                    var resolve = (Func<Type, object[], object>)invocation;
                    if (resolve == null)
                        continue;

                    try
                    {
                        var result = resolve(type, arguments);
                        if (result != null)
                            return result;
                    }
                    catch
                    {

                    }
                }
            }
            throw new InvalidOperationException("Cannot create an instance of type " + type.ToString());
        }

        /// <summary>
        /// Resolves the external reference.
        /// </summary>
        internal object ResolveContentReference(string referenceName)
        {
            if (ContentReferenceResolve != null)
            {
                foreach (var invocation in ContentReferenceResolve.GetInvocationList())
                {
                    var resolve = (Func<string, object>)invocation;
                    if (resolve == null)
                        continue;

                    try
                    {
                        var result = resolve(referenceName);
                        if (result != null)
                            return result;
                    }
                    catch (Exception) { }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the serialization data used during the serialization.
        /// </summary>
        public static Hashtable SerializationData { get; private set; }

        static XamlSerializer()
        {
            SerializationData = new Hashtable();
        }
        
        public void Dispose()
        {

        }
    }

    class ObjectWriter : XamlObjectWriter
    {
        public static object LastContentReference;

        private bool isContentReference;
        private XamlSerializer xamlSerializer;

        public ObjectWriter(XamlSerializer xamlSerializer, XamlSchemaContext schemaContext) : base(schemaContext)
        {
            this.xamlSerializer = xamlSerializer;
        }

        protected override void OnBeforeProperties(object value)
        {
            base.OnBeforeProperties(value);
        }

        protected override void OnAfterProperties(object value)
        {
            base.OnAfterProperties(value);
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            isContentReference = xamlType.UnderlyingType == typeof(ContentReference);
            if (isContentReference || xamlType.ConstructionRequiresArguments)
                base.WriteStartObject(new XamlFactoryType(xamlSerializer, xamlType));
            else
                base.WriteStartObject(xamlType);
        }

        public override void WriteGetObject()
        {
            base.WriteGetObject();
        }

        protected override bool OnSetValue(object eventSender, XamlMember member, object value)
        {
            if (isContentReference)
            {
                XamlSerializer.SerializationData.Add(new PropertyInstance(eventSender, member.Name), LastContentReference);
                LastContentReference = null;
                isContentReference = false;
            }
            return base.OnSetValue(eventSender, member, value);
        }
    }

    class ObjectReader : XamlObjectReader
    {
        private XamlSerializer xamlSerializer;

        public ObjectReader(XamlSerializer xamlSerializer, object value, XamlSchemaContext schemaContext) 
            : base(value, schemaContext)
        {
            this.xamlSerializer = xamlSerializer;
        }
    }

    class XamlFactoryType : XamlType
    {
        private XamlSerializer xamlSerializer;

        public XamlFactoryType(XamlSerializer xamlSerializer, XamlType xamlType) 
            : base(xamlType.UnderlyingType, xamlType.SchemaContext)
        {
            this.xamlSerializer = xamlSerializer;
        }
        
        protected override XamlTypeInvoker LookupInvoker()
        {
            return new XamlInvoker(xamlSerializer, base.LookupInvoker(), this);
        }

        protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
        {
            return new XamlValueConverter<TypeConverter>(typeof(MarkupExtensionConverter), this);
        }
    }

    class XamlInvoker : XamlTypeInvoker
    {
        private XamlType xamlType;
        private XamlSerializer xamlSerializer;
        private XamlTypeInvoker xamlTypeInvoker;

        public XamlInvoker(XamlSerializer xamlSerializer, XamlTypeInvoker xamlTypeInvoker, XamlType xamlType)
        {
            this.xamlType = xamlType;
            this.xamlSerializer = xamlSerializer;
            this.xamlTypeInvoker = xamlTypeInvoker;
        }

        public override void AddToCollection(object instance, object item)
        {
            xamlTypeInvoker.AddToCollection(instance, item);
        }

        public override void AddToDictionary(object instance, object key, object item)
        {
            xamlTypeInvoker.AddToDictionary(instance, key, item);
        }

        public override System.Reflection.MethodInfo GetAddMethod(XamlType contentType)
        {
            return xamlTypeInvoker.GetAddMethod(contentType);
        }

        public override System.Reflection.MethodInfo GetEnumeratorMethod()
        {
            return xamlTypeInvoker.GetEnumeratorMethod();
        }

        public override System.Collections.IEnumerator GetItems(object instance)
        {
            return xamlTypeInvoker.GetItems(instance);
        }

        public override object CreateInstance(object[] arguments)
        {
            try
            {
                if (xamlType.UnderlyingType == typeof(ContentReference))
                    return (ObjectWriter.LastContentReference = xamlTypeInvoker.CreateInstance(arguments));
                return xamlTypeInvoker.CreateInstance(arguments);
            }
            catch (MissingMethodException e)
            {
                var result = xamlSerializer.ResolveInstance(xamlType.UnderlyingType, arguments);
                if (result == null)
                    throw e;
                return result;
            }
        }
    }

    class SchemaContext : XamlSchemaContext, IContentReferenceProvider
    {
        private XamlSerializer xamlSerializer;

        public SchemaContext(XamlSerializer xamlSerializer, XamlSchemaContext xamlSchemaContext)
            : base(xamlSchemaContext.ReferenceAssemblies)
        {
            this.xamlSerializer = xamlSerializer;
        }

        public override XamlType GetXamlType(Type type)
        {
            var xamlType = base.GetXamlType(type);
            if (xamlType.ConstructionRequiresArguments && xamlType.UnderlyingType == typeof(Microsoft.Xna.Framework.Graphics.TextureCube))
                return new XamlFactoryType(xamlSerializer, xamlType);
            return xamlType;
        }

        object IContentReferenceProvider.ResolveContentReference(string name)
        {
            return xamlSerializer.ResolveContentReference(name);
        }
    }

    class MarkupExtensionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
            {
                var markupExtension = XamlSerializer.SerializationData[value];
                if (markupExtension != null)
                    return markupExtension;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
