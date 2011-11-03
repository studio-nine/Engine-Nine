#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a object factory that can create a object of the specified
    /// type based on the name of the object.
    /// </summary>
    [Serializable]
    public class Template : IXmlSerializable
    {
        /// <summary>
        /// New templates would be created from this persisted data rather then
        /// cloning from existing instances.
        /// </summary>
        private byte[] persisted;   

        /// <summary>
        /// Gets the name of this object.
        /// </summary>        
        public string Name { get; private set; }

        /// <summary>
        /// Gets the target type of this template.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
        /// </summary>
        internal Template() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="reader">The reader.</param>
        public Template(string name, Type type, XmlReader reader)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (type == null)
                throw new ArgumentNullException("type");
            if (reader == null)
                throw new ArgumentNullException("reader");
            Name = name;
            Type = type;
            ((IXmlSerializable)this).ReadXml(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="persist">The persist.</param>
        public Template(string name, Type type, byte[] persist)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (type == null)
                throw new ArgumentNullException("type");
            if (persist == null)
                throw new ArgumentNullException("persist");
            Name = name;
            Type = type;
            persisted = persist;
        }
        
        /// <summary>
        /// Creates a duplicated instance of this template.
        /// </summary>
        public T CreateInstance<T>()
        {
            var result = CreateInstance();
            if (result == null)
                return default(T);
            if (result is T)
                return (T)CreateInstance();
            
            throw new InvalidOperationException(string.Format(
                "The instance created is {0}, but trying to convert it to {1}", 
                result.GetType().Name, typeof(T).Name));
        }

        /// <summary>
        /// Creates a duplicated instance of this template.
        /// </summary>
        public virtual object CreateInstance()
        {
            using (var stream = new MemoryStream(persisted))
            {
                throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #region IXmlSerializable
        XmlSchema IXmlSerializable.GetSchema() { return null; }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            persisted = Encoding.UTF8.GetBytes(reader.ReadInnerXml());
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteRaw(Encoding.UTF8.GetString(persisted, 0, persisted.Length));
        }
        #endregion
    }
}