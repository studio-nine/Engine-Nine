namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Linq;

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
        private Writer binaryWriter = new Writer();
        private Reader binaryReader = new Reader();
        private IServiceProvider currentServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySerializer"/> class.
        /// </summary>
        public BinarySerializer() { }

        /// <summary>
        /// Loads from the specified input stream.
        /// </summary>
        public object Load(Stream input)
        {
            return Load(input, null, null);
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
            var lastStream = binaryReader.GetStream();
            var lastServiceProvider = currentServiceProvider;

            try
            {
                currentServiceProvider = serviceProvider;
                binaryReader.SetStream(input);

                return ((IBinaryObjectSerializer)this).ReadObject(binaryReader, existingIntance, this);
            }
            finally
            {
                binaryReader.SetStream(lastStream);
                currentServiceProvider = lastServiceProvider;
            }
        }

        /// <summary>
        /// Saves the target object to the specified output stream.
        /// </summary>
        public void Save(Stream output, object value)
        {
            Save(output, value, null);
        }

        /// <summary>
        /// Saves the target object to the specified output stream.
        /// </summary>
        public void Save(Stream output, object value, IServiceProvider serviceProvider)
        {
            var lastStream = binaryWriter.GetStream();
            var lastServiceProvider = currentServiceProvider;

            try
            {
                currentServiceProvider = serviceProvider;
                binaryWriter.SetStream(output);
  
                WriteObject(binaryWriter, value, this);
            }
            finally
            {
                binaryWriter.SetStream(lastStream);
                currentServiceProvider = lastServiceProvider;
            }
        }

        object IBinaryObjectSerializer.ReadObject(BinaryReader input, object existingInstance, IServiceProvider services)
        {
            var hash = input.ReadInt32();
            if (hash == 0)
                return null;

            IBinaryObjectReader reader;
            if (!Readers.TryGetValue(hash, out reader))
                throw new NotSupportedException("Don't know how to read: " + hash);

            return reader.Read(input, existingInstance, this);
        }

        void IBinaryObjectSerializer.WriteObject(BinaryWriter output, object value, IServiceProvider services)
        {
            WriteObject(output, value, services);
        }

        private void WriteObject(BinaryWriter output, object value, IServiceProvider services)
        {
            if (value == null)
            {
                output.Write((int)0);
            }
            else
            {
                IBinaryObjectWriter writer;
                if (!Writers.TryGetValue(value.GetType(), out writer))
                    throw new NotSupportedException("Don't know how to write: " + value.GetType());

                output.Write(ComputeHash(writer.ReaderType));
                writer.Write(output, value, this);
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            object result;
            if (currentServiceProvider != null && (result = currentServiceProvider.GetService(serviceType)) != null)
                return result;
            if (serviceType == typeof(IBinaryObjectSerializer))
                return this;
            return null;
        }

        #region Static readers and writers
        static BinarySerializer()
        {
            UpdateReaders(FindImplementations<IBinaryObjectReader>());
            UpdateWriters(FindImplementations<IBinaryObjectWriter>());

#if !MonoGame
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
            {
                UpdateReaders(FindImplementations<IBinaryObjectReader>(e.LoadedAssembly));
                UpdateWriters(FindImplementations<IBinaryObjectWriter>(e.LoadedAssembly));
            };
#endif
        }

        static Dictionary<int, IBinaryObjectReader> Readers = new Dictionary<int, IBinaryObjectReader>();
        static Dictionary<Type, IBinaryObjectWriter> Writers = new Dictionary<Type, IBinaryObjectWriter>();
        static Dictionary<Type, int> TypeToHash = new Dictionary<Type, int>();

        private static void UpdateReaders(IEnumerable<IBinaryObjectReader> readers)
        {
            foreach (var reader in readers)
            {
                if (reader == null)
                    continue;

                var key = ComputeHash(reader.GetType());
                if (key == 0)
                {
                    throw new InvalidOperationException(
                        string.Format("{0} conflicts with NullReader", reader.GetType()));
                }

                IBinaryObjectReader existingReader;
                if (Readers.TryGetValue(key, out existingReader))
                {
                    throw new InvalidOperationException(string.Format(
                        "{0} conflicts with {1}", reader.GetType(), existingReader.GetType()));
                }
                Readers[key] = reader;
            }
        }

        private static void UpdateWriters(IEnumerable<IBinaryObjectWriter> writers)
        {
            foreach (var writer in writers)
            {
                if (writer == null)
                    continue;

                var key = writer.TargetType;
                IBinaryObjectWriter existingWriter;

                if (Writers.TryGetValue(key, out existingWriter))
                    throw new InvalidOperationException(string.Format(
                        "{0} conflicts with {1}", writer.GetType(), existingWriter.GetType()));

                Writers[key] = writer;
            }
        }

        private static int ComputeHash(Type type)
        {
            int result;
            if (TypeToHash.TryGetValue(type, out result))
                return result;

            var binarySerializableAttributes = type.GetCustomAttributes(typeof(BinarySerializableAttribute), false);
            if (binarySerializableAttributes.Length > 0)
            {
                var token = ((BinarySerializableAttribute)binarySerializableAttributes[0]).Token;
                if (token.HasValue)
                    return TypeToHash[type] = token.Value;
            }
            return TypeToHash[type] = GetHashCode(type.AssemblyQualifiedName);
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

        private static IEnumerable<T> FindImplementations<T>()
        {
#if !MonoGame
            try
            {
                return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                       from type in FindImplementations<T>(assembly)
                       select type;
            }
            catch
            {
                return Enumerable.Empty<T>();
            }
#else
            return Enumerable.Empty<T>();
#endif
        }

        private static IEnumerable<T> FindImplementations<T>(Assembly assembly)
        {
            try
            {
                return from type in assembly.GetTypes()
                       where !type.IsAbstract && !type.IsInterface && !type.IsGenericType && 
                             !type.IsGenericTypeDefinition && typeof(T).IsAssignableFrom(type)
                       select CreateInstance<T>(type);
            }
            catch
            {
                return Enumerable.Empty<T>();
            }
        }

        private static T CreateInstance<T>(Type type)
        {
            try
            {
                return (T)Activator.CreateInstance(type);
            }
            catch
            {
                return default(T);
            }
        }
        #endregion

        #region Stream abstraction
        static Encoding Encoding = new UTF8Encoding();

        class Writer : BinaryWriter
        {
            public Stream GetStream()
            {
                return base.OutStream;
            }

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

            public Stream GetStream()
            {
                return this.stream.InnerStream;
            }

            public void SetStream(Stream stream)
            {
                if (stream != this.stream)
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
        #endregion
    }
}