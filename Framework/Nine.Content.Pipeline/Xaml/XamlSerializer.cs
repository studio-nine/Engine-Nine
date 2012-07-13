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
using System.Reflection;
using System.ComponentModel.Design.Serialization;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    #region XamlSerializer
    /// <summary>
    /// Provides extended Xaml load and save methods.
    /// </summary>
    public class XamlSerializer
    {
        internal Stack<MarkupExtension> MarkupExtensions = new Stack<MarkupExtension>();
        internal Stack<XamlMember> Members = new Stack<XamlMember>();

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
            var writer = new ObjectWriter(this, new SchemaContext(this));
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
            var reader = new ObjectReader(this, value);
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
        /// Occurs when this xaml serializer tried to save an object to markup extension.
        /// </summary>
        public event Func<object, MarkupExtension> MarkupExtensionResolve;

        internal MarkupExtension ResolveMarkupExtension(object value)
        {
            if (MarkupExtensionResolve != null)
            {
                foreach (var invocation in MarkupExtensionResolve.GetInvocationList())
                {
                    var resolve = (Func<object, MarkupExtension>)invocation;
                    if (resolve == null)
                        continue;

                    try
                    {
                        var result = resolve(value);
                        if (result != null)
                            return result;
                    }
                    catch
                    {

                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the serialization data used during the serialization.
        /// </summary>
        public static IDictionary<PropertyInstance, object> SerializationData { get; private set; }

        static XamlSerializer()
        {
            SerializationData = new Dictionary<PropertyInstance, object>();
        }
    }
    #endregion

    #region ObjectWriter
    class ObjectWriter : XamlObjectWriter
    {
        private object currentObject;
        private XamlSerializer xamlSerializer;

        public ObjectWriter(XamlSerializer xamlSerializer, XamlSchemaContext schemaContext) : base(schemaContext)
        {
            this.xamlSerializer = xamlSerializer;
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            base.WriteStartObject(new XamlFactoryType(xamlSerializer, xamlType));
        }

        public override void WriteStartMember(XamlMember property)
        {
            xamlSerializer.Members.Push(property);
            base.WriteStartMember(property);
        }

        public override void WriteEndMember()
        {
            base.WriteEndMember();

            var member = xamlSerializer.Members.Pop();
            if (xamlSerializer.MarkupExtensions.Count > 0)
                XamlSerializer.SerializationData.Add(new PropertyInstance(currentObject, member.Name), xamlSerializer.MarkupExtensions.Pop());
        }

        protected override bool OnSetValue(object eventSender, XamlMember member, object value)
        {
            currentObject = eventSender;
            return base.OnSetValue(eventSender, member, value);
        }
    }
    #endregion

    #region ObjectReader
    class ObjectReader : XamlObjectReader
    {
        private XamlSerializer xamlSerializer;

        public ObjectReader(XamlSerializer xamlSerializer, object value) 
            : base(value, new SchemaContext(xamlSerializer))
        {
            this.xamlSerializer = xamlSerializer;
        }
    }
    #endregion

    #region XamlFactoryType
    class XamlFactoryType : XamlType
    {
        private XamlSerializer xamlSerializer;

        public XamlFactoryType(XamlSerializer xamlSerializer, XamlType xamlType) 
            : base(xamlType.UnderlyingType, xamlType.SchemaContext)
        {
            this.xamlSerializer = xamlSerializer;
        }

        private bool HasDefaultConstructor()
        {
            return !ConstructionRequiresArguments || UnderlyingType.IsPrimitive || UnderlyingType.IsValueType ||
                    UnderlyingType == typeof(Enum) || UnderlyingType == typeof(ValueType);
        }

        protected override XamlTypeInvoker LookupInvoker()
        {
            return new XamlFactoryInvoker(xamlSerializer, base.LookupInvoker(), this);
        }

        protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
        {
            if (HasDefaultConstructor())
                return base.LookupTypeConverter();
            return new XamlValueConverter<TypeConverter>(typeof(InstanceDescriptorConverter), this);
        }
    }
    #endregion

    #region XamlFactoryInvoker
    class XamlFactoryInvoker : XamlTypeInvoker
    {
        private XamlType xamlType;
        private XamlSerializer xamlSerializer;
        private XamlTypeInvoker xamlTypeInvoker;

        public XamlFactoryInvoker(XamlSerializer xamlSerializer, XamlTypeInvoker xamlTypeInvoker, XamlType xamlType)
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

        public override MethodInfo GetAddMethod(XamlType contentType)
        {
            return xamlTypeInvoker.GetAddMethod(contentType);
        }

        public override MethodInfo GetEnumeratorMethod()
        {
            return xamlTypeInvoker.GetEnumeratorMethod();
        }

        public override System.Collections.IEnumerator GetItems(object instance)
        {
            return xamlTypeInvoker.GetItems(instance);
        }

        public override object CreateInstance(object[] arguments)
        {
            object result = null;
            try
            {
                result = xamlTypeInvoker.CreateInstance(arguments);
            }
            catch (MissingMethodException e)
            {
                result = xamlSerializer.ResolveInstance(xamlType.UnderlyingType, arguments);
            }

            if (result == null)
                throw new MissingMethodException();
            if (result is MarkupExtension)
                xamlSerializer.MarkupExtensions.Push((MarkupExtension)result);
            return result;
        }
    }
    #endregion

    #region SchemaContext
    class SchemaContext : XamlSchemaContext
    {
        private XamlSerializer xamlSerializer;

        public SchemaContext(XamlSerializer xamlSerializer, XamlSchemaContext xamlSchemaContext)
            : base(xamlSchemaContext.ReferenceAssemblies)
        {
            this.xamlSerializer = xamlSerializer;
        }

        public SchemaContext(XamlSerializer xamlSerializer)
            : base(new XamlSchemaContext().ReferenceAssemblies)
        {
            this.xamlSerializer = xamlSerializer;
        }

        public override XamlType GetXamlType(Type type)
        {
            return new XamlFactoryType(xamlSerializer, base.GetXamlType(type));
        }
    }
    #endregion

    #region InstanceDescriptorConverter
    class InstanceDescriptorConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return new InstanceDescriptor(new NullConstructorInfo( value.GetType().GetConstructors()[0]), new object[0]);
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    #endregion

    #region NullConstructorInfo
    class NullConstructorInfo : ConstructorInfo
    {
        ConstructorInfo info;

        public NullConstructorInfo(ConstructorInfo info)
        {
            this.info = info;
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override MethodAttributes Attributes
        {
            get { return info.Attributes; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return info.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return new ParameterInfo[0];
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return info.MethodHandle; }
        }

        public override Type DeclaringType
        {
            get { return info.DeclaringType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return info.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return info.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return info.IsDefined(attributeType, inherit);
        }

        public override string Name
        {
            get { return info.Name; }
        }

        public override Type ReflectedType
        {
            get { return info.ReflectedType; }
        }
    }
    #endregion
}
