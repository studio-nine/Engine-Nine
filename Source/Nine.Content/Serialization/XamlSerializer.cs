namespace Nine.Serialization
{
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml;

    #region XamlSerializer
    /// <summary>
    /// Provides extended Xaml load and save methods.
    /// </summary>
    public class XamlSerializer : IContentImporter
    {
        internal const string DefaultNamespace = "http://schemas.microsoft.com/nine/2011/xaml";

        internal IServiceProvider ServiceProvider;
        internal ISerializationOverride SerializationOverride;
        
        /// <summary>
        /// Initializes a new instance of XamlSerializer.
        /// </summary>
        public XamlSerializer() { }
                
        /// <summary>
        /// Loads the object graph from a stream.
        /// </summary>
        public object Load(Stream stream, IServiceProvider serviceProvider)
        {
            InitializeServices(serviceProvider);
            var reader = new XamlXmlReader(XmlReader.Create(stream, null, new XmlParserContext(null, new DefaultNamespaceManager(), null, XmlSpace.Default)));
            var writer = new ObjectWriter(this, new SchemaContext(this));
            XamlServices.Transform(reader, writer, false);
            return writer.Result;
        }

        /// <summary>
        /// Saves the specified object to a stream.
        /// </summary>
        public void Save(Stream stream, object value, IServiceProvider serviceProvider)
        {
            InitializeServices(serviceProvider);

            var reader = new ObjectReader(this, value);
            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                var writer = new XamlXmlWriter(xmlWriter, reader.SchemaContext);
                XamlServices.Transform(reader, writer, true);
            }
        }

        private void InitializeServices(IServiceProvider serviceProvider)
        {
            if ((this.ServiceProvider = serviceProvider) != null)
                SerializationOverride = serviceProvider.TryGetService<ISerializationOverride>();
        }

        internal object ResolveInstance(Type type, object[] arguments)
        {
            var result = Extensions.CreateInstance<object>(type, ServiceProvider);
            if (result != null)
                return result;

            try
            {
                return Activator.CreateInstance(type, ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice);
            }
            catch (MissingMethodException)
            {
                throw new InvalidOperationException("Cannot create an instance of type " + type.ToString());
            }
        }

        object IContentImporter.Import(Stream stream, IServiceProvider serviceProvider)
        {
            return Load(stream, serviceProvider);
        }

        string[] IContentImporter.SupportedFileExtensions
        {
            get { return SupportedFileExtensions; }
        }
        static readonly string[] SupportedFileExtensions = new[] { ".xaml" };
    }
    #endregion

    #region DefaultNamespaceReader
    class DefaultNamespaceManager : XmlNamespaceManager
    {
        public DefaultNamespaceManager() : base(new NameTable()) { }
        public override string LookupNamespace(string prefix)
        {
            var result = base.LookupNamespace(prefix);
            return string.IsNullOrEmpty(result) ? XamlSerializer.DefaultNamespace : result;
        }
    }
    #endregion

    #region ObjectWriter
    class ObjectWriter : XamlObjectWriter
    {
        private XamlSerializer xamlSerializer;

        public ObjectWriter(XamlSerializer xamlSerializer, XamlSchemaContext schemaContext) : base(schemaContext)
        {
            this.xamlSerializer = xamlSerializer;
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            if (xamlType.UnderlyingType == null)
                throw new InvalidOperationException("Cannot create an object of type " + xamlType.Name);
            base.WriteStartObject(new XamlFactoryType(xamlSerializer, xamlType));
        }
    }
    #endregion

    #region ObjectReader
    class ObjectReader : XamlObjectReader
    {
        private XamlSerializer xamlSerializer;

        public ObjectReader(XamlSerializer xamlSerializer, object value) : base(value, new SchemaContext(xamlSerializer))
        {
            this.xamlSerializer = xamlSerializer;
        }
    }
    #endregion

    #region XamlFactoryType
    class XamlFactoryType : XamlType
    {
        private XamlSerializer xamlSerializer;

        public XamlFactoryType(XamlSerializer xamlSerializer, XamlType xamlType) : base(xamlType.UnderlyingType, xamlType.SchemaContext)
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
            catch (MissingMethodException)
            {
                result = xamlSerializer.ResolveInstance(xamlType.UnderlyingType, arguments);
            }

            if (result == null)
                throw new MissingMethodException();
            return result;
        }
    }
    #endregion

    #region SchemaContext
    class SchemaContext : XamlSchemaContext, IServiceProvider
    {
        private XamlSerializer xamlSerializer;

        public SchemaContext(XamlSerializer xamlSerializer, XamlSchemaContext xamlSchemaContext) : base(xamlSchemaContext.ReferenceAssemblies)
        {
            this.xamlSerializer = xamlSerializer;
        }

        public SchemaContext(XamlSerializer xamlSerializer) : base(new XamlSchemaContext().ReferenceAssemblies)
        {
            this.xamlSerializer = xamlSerializer;
        }

        public override XamlType GetXamlType(Type type)
        {
            return new XamlFactoryType(xamlSerializer, base.GetXamlType(type));
        }

        public object GetService(Type serviceType)
        {
            return xamlSerializer.ServiceProvider.GetService(serviceType);
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
