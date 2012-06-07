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
using System.ComponentModel;
using System.Linq;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio
{
    /*
    class TypeDescriptorHelper
    {
        static Dictionary<Type, TypeDescriptionProvider> addedProviders = new Dictionary<Type, TypeDescriptionProvider>();

        public static void AddAttributes(IEnumerable<IAttributeProvider> attributeProviders)
        {
            foreach (var type in addedProviders.Keys)
            {
                TypeDescriptor.RemoveProvider(addedProviders[type], type);
            }

            addedProviders.Clear();

            foreach (var group in attributeProviders.GroupBy(p => p.TargetType))
            {
                AddAttributes(group.Key, group);
            }
        }

        private static void AddAttributes(Type type, IGrouping<Type, IAttributeProvider> group)
        {
            var provider = new ExtendedTypeDescriptionProvider(type, group);
            TypeDescriptor.AddProvider(provider, type);
            addedProviders[type] = provider;
        }
    }

    class ExtendedTypeDescriptionProvider : TypeDescriptionProvider
    {
        IEnumerable<IAttributeProvider> attributeProviders;
        Type targetType;

        public ExtendedTypeDescriptionProvider(Type type, IEnumerable<IAttributeProvider> attributeProviders)
            : base(TypeDescriptor.GetProvider(type))
        {
            this.targetType = type;
            this.attributeProviders = attributeProviders;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (targetType.IsAssignableFrom(objectType))
                return new ExtendedCustomTypeDescriptor(base.GetTypeDescriptor(objectType, instance), attributeProviders);

            return base.GetTypeDescriptor(objectType, instance);
        }
    }

    class ExtendedCustomTypeDescriptor : CustomTypeDescriptor
    {
        IEnumerable<IAttributeProvider> attributeProviders;

        public ExtendedCustomTypeDescriptor(ICustomTypeDescriptor baseDescriptor, IEnumerable<IAttributeProvider> attributeProviders)
            : base(baseDescriptor)
        {
            this.attributeProviders = attributeProviders;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var properties = base.GetProperties().OfType<PropertyDescriptor>()
                                 .Select(p => TypeDescriptor.CreateProperty(p.ComponentType, p,
                                        attributeProviders.SelectMany(ap => ap.GetCustomAttributes(p.Name)).ToArray()));

            return new PropertyDescriptorCollection(properties.ToArray());
        }
    }
     */
}
