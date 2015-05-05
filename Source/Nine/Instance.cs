namespace Nine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Nine.Animations;
    using Nine.Serialization;
    using Nine.AttachedProperty;

    /// <summary>
    /// Defines a instance that is created from a template.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    [ContentProperty("Properties")]
    public class Instance : Transformable, IObjectFactory
    {
        /// <summary>
        /// Gets or sets the file name that contains the template.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Gets a dictionary of properties that will override the properties
        /// in the created instance.
        /// </summary>
        public IDictionary<string, object> Properties
        {
            get { return properties; }
        }
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of Instance class.
        /// </summary>
        public Instance() { }

        /// <summary>
        /// Initializes a new instance of Instance class.
        /// </summary>        
        public Instance(string template) 
        {
            this.Template = template;
        }

        /// <summary>
        /// Create a new instance of the object from template.
        /// </summary>
        public T CreateInstance<T>(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Template))
                return default(T);

            // TODO: MonoGame Content Pipeline
#if !MonoGame
            var contentLoader = serviceProvider.GetService<ContentLoader>();
            var createdInstance = ApplyProperties(contentLoader.Create<T>(Template));

            // Replace the name of the created instance
            var instanceName = createdInstance as Nine.Object;
            if (instanceName != null)
                instanceName.name = name;

            // Replace the transform of the created instance
            var instanceTransform = createdInstance as Transformable;
            if (instanceTransform != null)
                instanceTransform.Transform = transform;

            // Apply attached properties
            var instanceAttachedProperties = createdInstance as IAttachedPropertyStore;
            if (instanceAttachedProperties != null)
            {
                var store = (IAttachedPropertyStore)this;
                if (store.PropertyCount > 0)
                {
                    var attachedProperties = new KeyValuePair<AttachableMemberIdentifier, object>[store.PropertyCount];
                    store.CopyPropertiesTo(attachedProperties, 0);
                    for (var i = 0; i < attachedProperties.Length; ++i)
                        instanceAttachedProperties.SetProperty(attachedProperties[i].Key, attachedProperties[i].Value);
                }
            }

            return createdInstance;
#else
            return default(T);
#endif
        }

        /// <summary>
        /// Create a new instance of the object from template.
        /// </summary>
        public object CreateInstance(IServiceProvider serviceProvider)
        {
            return CreateInstance<object>(serviceProvider);
        }

        private T ApplyProperties<T>(T result)
        {
            foreach (var pair in properties)
            {
                new PropertyExpression<object>(result, pair.Key).Value = pair.Value;
            }
            return result;
        }
    }
}