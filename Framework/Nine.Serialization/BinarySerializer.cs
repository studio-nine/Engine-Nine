namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Forces a type or member to be serializable by the BinarySerializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
    public class BinarySerializableAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the token to identify the target object during binary serialization.
        /// </summary>
        public int? Token { get; set; }

        public BinarySerializableAttribute() { }
        public BinarySerializableAttribute(int token) { this.Token = token; }
    }

    /// <summary>
    /// Forces a type or member to be not serializable by the BinarySerializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class NotBinarySerializableAttribute : Attribute { }

    /// <summary>
    /// Defines an object reader that reads an object from a binary stream.
    /// </summary>
    public interface IBinaryObjectReader
    {
        /// <summary>
        /// Creates an object from the input binary reader.
        /// </summary>
        object Read(BinaryReader input, object existingInstance, IServiceProvider serviceProvider);
    }

    /// <summary>
    /// Defines an binary writer that writes an object to a binary stream.
    /// </summary>    
    public interface IBinaryObjectWriter
    {
        /// <summary>
        /// Gets the type of IBinaryObjectReader that reads the output of this writer.
        /// </summary>
        Type ReaderType { get; }

        /// <summary>
        /// Gets the target object type that can be written by this writer.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Saves an object to the output binary reader.
        /// </summary>
        void Write(BinaryWriter output, object value, IServiceProvider serviceProvider);
    }

    /// <summary>
    /// A service used by IBinaryObjectReader and IBinaryObjectWriter to load and save arbitrary object. 
    /// </summary>
    public interface IBinaryObjectSerializer
    {
        /// <summary>
        /// Creates an arbitrary object from the input binary reader.
        /// </summary>        
        object ReadObject(BinaryReader input, object existingInstance, IServiceProvider serviceProvider);

        /// <summary>
        /// Saves an arbitrary object to the output binary reader.
        /// </summary>
        void WriteObject(BinaryWriter output, object value, IServiceProvider serviceProvider);
    }

    /// <summary>
    /// Loads and saves an arbitrary object.
    /// </summary>
    public partial class BinarySerializer: IBinaryObjectSerializer, IServiceProvider
    {
        /// <summary>
        /// Gets a collection of binary object reader types.
        /// </summary>
        public IList<Type> ReaderTypes { get { return readerTypes; } }

        /// <summary>
        /// Gets a collection of binary object writer types.
        /// </summary>     
        public IList<Type> WriterTypes { get { return writerTypes; } }
        
        private bool readersNeedsUpdate;
        private bool writersNeedsUpdate;
        private List<Type> readerTypes = new List<Type>();
        private List<Type> writerTypes = new List<Type>();
        private Dictionary<int, IBinaryObjectReader> readers = new Dictionary<int, IBinaryObjectReader>();
        private Dictionary<Type, IBinaryObjectWriter> writers = new Dictionary<Type, IBinaryObjectWriter>();
        private Dictionary<Type, int> typeToHash = new Dictionary<Type, int>();
        private Stack<IServiceProvider> serviceProviderStack = new Stack<IServiceProvider>();
        private Writer binaryWriter = new Writer();
        private Reader binaryReader = new Reader();

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySerializer"/> class.
        /// </summary>
        public BinarySerializer() : this(true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySerializer"/> class.
        /// </summary>
        public BinarySerializer(bool createReadersAndWriters)
        {
            if (createReadersAndWriters)
            {
                this.readerTypes.AddRange(FindImplementations(typeof(IBinaryObjectReader)));
                this.writerTypes.AddRange(FindImplementations(typeof(IBinaryObjectWriter)));
            }
            this.readersNeedsUpdate = true;
            this.writersNeedsUpdate = true;
        }

        /// <summary>
        /// Loads from the specified input stream.
        /// </summary>
        public object Load(Stream input)
        {
            binaryReader.SetStream(input);
            return ((IBinaryObjectSerializer)this).ReadObject(binaryReader, null, this);
        }
        
        /// <summary>
        /// Loads from the specified input stream.
        /// </summary>
        public object Load(Stream input, IServiceProvider serviceProvider)
        {
            return Load(input, null, serviceProvider);
        }

        /// <summary>
        /// Loads from the specified input stream.
        /// </summary>
        public object Load(Stream input, object existingIntance, IServiceProvider serviceProvider)
        {
            try
            {
                serviceProviderStack.Push(serviceProvider);
                binaryReader.SetStream(input);
                return ((IBinaryObjectSerializer)this).ReadObject(binaryReader, existingIntance, this);
            }
            finally
            {
                serviceProviderStack.Pop();
            }
        }

        /// <summary>
        /// Saves to the specified output stream.
        /// </summary>
        public void Save(Stream output, object value)
        {
            binaryWriter.SetStream(output);
            ((IBinaryObjectSerializer)this).WriteObject(binaryWriter, value, this);
        }

        /// <summary>
        /// Saves to the specified output stream.
        /// </summary>
        public void Save(Stream output, object value, IServiceProvider serviceProvider)
        {
            try
            {
                serviceProviderStack.Push(serviceProvider);
                binaryWriter.SetStream(output);
                ((IBinaryObjectSerializer)this).WriteObject(binaryWriter, value, this);
            }
            finally
            {
                serviceProviderStack.Pop();
            }
        }

        private void UpdateReaders()
        {
            readers.Clear();
            foreach (var readerType in readerTypes)
            {
                var key = ComputeHash(readerType);
                if (key == 0)
                {
                    throw new InvalidOperationException(
                        string.Format("{0} conflicts with NullReader", readerType));
                }

                IBinaryObjectReader existingReader;
                if (readers.TryGetValue(key, out existingReader))
                {
                    throw new InvalidOperationException(string.Format(
                        "{0} conflicts with {1}", readerType, existingReader.GetType()));
                }
                readers[key] = (IBinaryObjectReader)Activator.CreateInstance(readerType);
            }
        }

        private void UpdateWriters()
        {
            writers.Clear();
            foreach (var writerType in writerTypes)
            {
                var writer = (IBinaryObjectWriter)Activator.CreateInstance(writerType);
                var key = writer.TargetType;
                IBinaryObjectWriter existingWriter;
                
                if (writers.TryGetValue(key, out existingWriter))
                    throw new InvalidOperationException(string.Format(
                        "{0} conflicts with {1}", writer.GetType(), existingWriter.GetType()));

                writers[key] = writer;
            }
        }

        private int ComputeHash(Type type)
        {
            int result;
            if (typeToHash.TryGetValue(type, out result))
                return result;

            var binarySerializableAttributes = type.GetCustomAttributes(typeof(BinarySerializableAttribute), false);
            if (binarySerializableAttributes.Length > 0)
            {
                var token = ((BinarySerializableAttribute)binarySerializableAttributes[0]).Token;
                if (token.HasValue)
                    return typeToHash[type] = token.Value;
            }
            return typeToHash[type] = GetHashCode(type.AssemblyQualifiedName);
        }

        /// <summary>
        /// djb2 hash with Xor
        /// http://www.cse.yorku.ca/~oz/hash.html
        /// </summary>
        private static int GetHashCode(string str)
        {
            int hash = 5381;
            foreach (char c in str)
                hash = ((hash << 5) + hash) ^ c; // hash * 33 + c
            return hash;
        }

        object IBinaryObjectSerializer.ReadObject(BinaryReader input, object existingInstance, IServiceProvider services)
        {
            if (readersNeedsUpdate)
            {
                UpdateReaders();
                readersNeedsUpdate = false;
            }

            var hash = input.ReadInt32();
            if (hash == 0)
                return null;
            return readers[hash].Read(input, existingInstance, this);
        }

        void IBinaryObjectSerializer.WriteObject(BinaryWriter output, object value, IServiceProvider services)
        {
            if (value == null)
            {
                output.Write((int)0);
            }
            else
            {
                if (writersNeedsUpdate)
                {
                    UpdateWriters();
                    writersNeedsUpdate = false;
                }

                IBinaryObjectWriter writer;

                var targetType = value.GetType();
                if (!writers.TryGetValue(targetType, out writer))
                    throw new NotSupportedException("Don't know how to write: " + targetType);

                output.Write(ComputeHash(writer.ReaderType));
                writer.Write(output, value, this);
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            object result;
            var serviceProvider = serviceProviderStack.Count > 0 ? serviceProviderStack.Peek() : null;
            if (serviceProvider != null && (result = serviceProvider.GetService(serviceType)) != null)
                return result;
            if (serviceType == typeof(IBinaryObjectSerializer))
                return this;
            return null;
        }

        private static IEnumerable<Type> FindImplementations(Type baseType)
        {
            Type[] types;
            Assembly[] assemblies;

            try { assemblies = AppDomain.CurrentDomain.GetAssemblies(); }
            catch { yield break; }

            foreach (var assembly in assemblies)
            {
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsInterface &&
                        !type.IsGenericType && !type.IsGenericTypeDefinition &&
                        baseType.IsAssignableFrom(type))
                    {
                        yield return type;
                    }
                }
            }
        }
        
        static Encoding Encoding = new UTF8Encoding();

        class Writer : BinaryWriter
        {
            public void SetStream(Stream stream)
            {
                base.OutStream = stream;
            }
        }

        class Reader : BinaryReader
        {
            private StreamWrapper stream;

            public Reader() : base(new StreamWrapper { InnerStream = Stream.Null }, Encoding)
            {
                this.stream = (StreamWrapper)base.BaseStream;
            }

            public void SetStream(Stream stream)
            {
                this.stream.InnerStream = stream;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        class StreamWrapper : Stream
        {
            internal Stream InnerStream;

            public override bool CanRead { get { return InnerStream.CanRead; } }
            public override bool CanSeek { get { return InnerStream.CanSeek; } }
            public override bool CanWrite { get { return InnerStream.CanWrite; } }
            public override void Flush() { InnerStream.Flush(); }
            public override long Length { get { return InnerStream.Length; } }
            public override long Position { get { return InnerStream.Position; } set { InnerStream.Position = value; } }
            public override int Read(byte[] buffer, int offset, int count) { return InnerStream.Read(buffer, offset, count); }
            public override long Seek(long offset, SeekOrigin origin) { return InnerStream.Seek(offset, origin); }
            public override void SetLength(long value) { InnerStream.SetLength(value); }
            public override void Write(byte[] buffer, int offset, int count) { InnerStream.Write(buffer, offset, count); }
        }
    }
}