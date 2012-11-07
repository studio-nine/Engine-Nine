namespace Nine.Studio.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines an interface that provides custom attributes to a type.
    /// </summary>
    public interface IAttributeProvider
    {
        /// <summary>
        /// Gets the type that this attribute provider works with.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets all the attributes attached to the target type.
        /// </summary>
        IEnumerable<Attribute> GetCustomAttributes();

        /// <summary>
        /// Gets all the attributes attached to the target type.
        /// </summary>
        IEnumerable<Attribute> GetCustomAttributes(string member);
    }

    /// <summary>
    /// Generic base class implementing IImporter
    /// </summary>
    public abstract class AttributeProvider<T> : IAttributeProvider
    {
        private List<Attribute> attributes = new List<Attribute>();
        private Dictionary<string, List<Attribute>> memberAttributes = new Dictionary<string, List<Attribute>>();

        public Type TargetType { get { return typeof(T); } }

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

        IEnumerable<Attribute> IAttributeProvider.GetCustomAttributes(string member)
        {
            List<Attribute> att;
            if (memberAttributes.TryGetValue(member, out att))
                return att;
            return Enumerable.Empty<Attribute>();
        }

        IEnumerable<Attribute> IAttributeProvider.GetCustomAttributes()
        {
            return attributes;
        }
    }
}
