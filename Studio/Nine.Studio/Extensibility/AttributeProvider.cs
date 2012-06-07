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
using System.Linq;
#endregion

namespace Nine.Studio.Extensibility
{
    /// <summary>
    /// Defines an interface that provides custom attributes to a type.
    /// </summary>
    public interface IAttributeProvider
    {
        /// <summary>
        /// Gets all the attributes attached to the target type.
        /// </summary>
        IEnumerable<Attribute> GetCustomAttributes(Type type);

        /// <summary>
        /// Gets all the attributes attached to the target type.
        /// </summary>
        IEnumerable<Attribute> GetCustomAttributes(Type type, string member);
    }

    /// <summary>
    /// Generic base class implementing IImporter
    /// </summary>
    public abstract class AttributeProvider<T> : IAttributeProvider
    {
        private List<Attribute> attributes = new List<Attribute>();
        private Dictionary<string, List<Attribute>> memberAttributes = new Dictionary<string, List<Attribute>>();

        protected void AddCustomAttribute(params Attribute[] customAttributes)
        {
            Verify.IsNotNull(customAttributes, "customAttributes");

            attributes.AddRange(customAttributes);
        }

        protected void AddCustomAttribute(string member, params Attribute[] customAttributes)
        {
            Verify.IsNeitherNullNorEmpty(member, "member");
            Verify.IsNotNull(customAttributes, "customAttributes");

            List<Attribute> att;
            if (!memberAttributes.TryGetValue(member, out att))
                memberAttributes[member] = (att = new List<Attribute>());
            att.AddRange(customAttributes);
        }

        IEnumerable<Attribute> IAttributeProvider.GetCustomAttributes(Type type, string member)
        {
            List<Attribute> att;
            if (type == typeof(T) && memberAttributes.TryGetValue(member, out att))
                return att;
            return Enumerable.Empty<Attribute>();
        }

        IEnumerable<Attribute> IAttributeProvider.GetCustomAttributes(Type type)
        {
            if (type == typeof(T))
                return attributes;
            return Enumerable.Empty<Attribute>();
        }
    }
}
