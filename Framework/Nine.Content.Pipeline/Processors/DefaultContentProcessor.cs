#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes each object of the input xml content using DefaultContentProcessorAttribute.
    /// </summary>
    [ContentProcessor(DisplayName = "Content - Engine Nine")]
    public class DefaultContentProcessor : ContentProcessor<object, object>
    {
        ContentProcessorContext context;

        public override object Process(object input, ContentProcessorContext context)
        {
            if (input == null)
                return null;

            this.context = context;
            ForEachProperty(input, Process);
            return input;
        }
        
        private object Process(Type type, object input)
        {
            ForEachProperty(input, Process);

            var defaultProcessorAttribute = input.GetType().GetCustomAttributes(typeof(DefaultContentProcessorAttribute), false).OfType<DefaultContentProcessorAttribute>().FirstOrDefault();
            if (defaultProcessorAttribute != null)
            {
                var processor = (IContentProcessor)Activator.CreateInstance(Type.GetType(defaultProcessorAttribute.DefaultProcessor));
                if (processor.InputType.IsAssignableFrom(input.GetType()) && type.IsAssignableFrom(processor.OutputType))
                {
                    context.Logger.LogImportantMessage("Processing {0} using {1}", input.GetType().Name, processor.GetType().Name);
                    return processor.Process(input, context);
                }
                else
                {
                    context.Logger.LogWarning(null, null, "Processor type mismatch {0} -> {1} -> {2} -> {4}", input.GetType().Name, processor.InputType.Name, processor.OutputType.Name, type.Name);
                }
            }
            return input;
        }

        private void ForEachProperty(object target, Func<Type, object, object> action)
        {
            if (target != null && action != null)
            {
                foreach (var property in target.GetType().GetProperties())
                {
                    if (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
                    {
                        context.Logger.LogMessage("Scanning {0}:{1}", property.Name, property.PropertyType.Name);

                        object value = property.GetValue(target, new object[0]);
                        if (value != null)
                        {
                            var result = action(property.PropertyType, value);
                            property.SetValue(target, result, new object[0]);
                        }
                    }
                }
            }

            ForEachCollectionProperty(target, action);
        }

        private void ForEachCollectionProperty(object target, Func<Type, object, object> action)
        {
            if (target != null && action != null)
            {
                foreach (var property in target.GetType().GetProperties())
                {
                    if (property.CanRead && property.GetIndexParameters().Length == 0)
                    {
                        object value = property.GetValue(target, new object[0]);
                        if (value != null)
                        {
                            var type = value.GetType();
                            if (IsList(type))
                            {
                                var itemAccessor = type.GetProperty("Item");
                                if (itemAccessor != null && itemAccessor.CanRead && itemAccessor.CanWrite &&
                                    itemAccessor.GetIndexParameters().Length == 1 &&
                                    itemAccessor.GetIndexParameters()[0].ParameterType == typeof(int))
                                {
                                    context.Logger.LogMessage("Scanning List {0}:{1}", property.Name, property.PropertyType.Name);

                                    for (int i = 0; i < ((ICollection)value).Count; i++)
                                    {
                                        itemAccessor.SetValue(value, action(property.PropertyType,
                                            itemAccessor.GetValue(value, new object[] { i })), new object[] { i });
                                    }
                                }
                            }
                            // TODO: Support dictionary.
                        }
                    }
                }
            }
        }

        private bool IsList(Type type)
        {
            return !(typeof(Array).IsAssignableFrom(type)) && !type.Name.Contains("ReadOnly") &&
                     type.FindInterfaces((t, o) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>), null).Length > 0;
        }

        private bool IsDictionary(Type type)
        {
            return type.FindInterfaces((t, o) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>), null).Length > 0;
        }
    }
}
