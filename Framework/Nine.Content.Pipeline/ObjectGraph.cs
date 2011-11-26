#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Nine.Content.Pipeline
{
    static class ObjectGraph
    {
        public static void ForEachProperty(object target, Func<Type, object, object> action)
        {
            if (target != null && action != null)
            {
                foreach (var property in target.GetType().GetProperties())
                {
                    if (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
                    {
                        Trace.WriteLine(string.Format("Scanning {0}:{1}", property.Name, property.PropertyType.Name));

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

        private static void ForEachCollectionProperty(object target, Func<Type, object, object> action)
        {
            if (target != null && action != null)
            {
                foreach (var property in target.GetType().GetProperties())
                {
                    if (property.CanRead && property.GetIndexParameters().Length == 0)
                    {
                        object value = null;
                        try
                        {
                            value = property.GetValue(target, new object[0]);
                        }
                        catch { }
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
                                    Trace.WriteLine(string.Format("Scanning List {0}:{1}", property.Name, property.PropertyType.Name));

                                    dynamic dynamicValue = value;
                                    var count = dynamicValue.Count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        itemAccessor.SetValue(value, action(property.PropertyType,
                                            itemAccessor.GetValue(value, new object[] { i })), new object[] { i });

                                        ObjectGraph.ForEachProperty(itemAccessor.GetValue(value, new object[] { i }), action);
                                    }
                                }
                            }
                            // TODO: Support dictionary.
                        }
                    }
                }
            }
        }

        private static bool IsList(Type type)
        {
            return !(typeof(Array).IsAssignableFrom(type)) && !type.Name.Contains("ReadOnly") &&
                     type.FindInterfaces((t, o) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>), null).Length > 0;
        }

        private static bool IsDictionary(Type type)
        {
            return type.FindInterfaces((t, o) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>), null).Length > 0;
        }
    }
}
