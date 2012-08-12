namespace Nine.Content.Pipeline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Xaml;

    /// <summary>
    /// Traverses an object graph.
    /// </summary>
    static class ObjectGraph
    {
        /// <summary>
        /// Traverses the each property of the target object.
        /// </summary>
        public static void TraverseProperties(object target, Func<object, object> action)
        {
            if (target == null || action == null)
                return;

            try
            {
                InternalTraverseProperties(target, action);
            }
            finally
            {
                TraversedObjects.Clear();
            }
        }

        private static void InternalTraverseProperties(object target, Func<object, object> action)
        {
            try
            {
                Trace.Indent();
                InternalTraversePropertiesWorker(target, action);
            }   
            finally
            {
                if (target != null)
                {
                    // Type.GetProperties do not always return the properties in the same order.
                    // The following workaround clears the property cache of the input type to make them
                    // returns the same order for ContentCompiler and IntermediateSerializer.
                    // 
                    // See this post http://blogs.msdn.com/b/haibo_luo/archive/2006/07/09/661091.aspx for details.
                    ClearReflectionCache(target.GetType());
                }
             
                Trace.Unindent();
                Trace.Flush();
            }
        }

        private static void InternalTraversePropertiesWorker(object target, Func<object, object> action)
        {
            if (target == null || action == null)
                return;

            // Skip if the target has already been traversed.
            if (TraversedObjects.Contains(target))
                return;
            TraversedObjects.Add(target);

            // Skip if the target is a known basic type.
            Type targetType = target.GetType();
            if (IsBasicType(targetType))
                return;

            // Browse each element if target is IEnumerable.
            IEnumerable enumerable = target as IEnumerable;
            if (enumerable != null)
            {
                // Special routine for dictionaries.
                IDictionary dictionary = enumerable as IDictionary;
                if (dictionary != null)
                {
                    foreach (var key in dictionary.Keys.Cast<object>().ToArray())
                    {
                        object input = dictionary[key];
                        if (input == null || IsBasicType(input.GetType()))
                            continue;

                        if (TraversedObjects.Contains(input))
                            continue;

                        object output = action(input);
                        if (output != input)
                            dictionary[key] = output;
                        else
                            InternalTraverseProperties(output, action);
                    }
                    return;
                }

                // Special routine for list.
                IList list = enumerable as IList;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        object input = list[i];
                        if (input == null || IsBasicType(input.GetType()))
                            continue;

                        if (TraversedObjects.Contains(input))
                            continue;

                        object output = action(input);
                        if (output != input)
                            list[i] = output;
                        else
                            InternalTraverseProperties(output, action);
                    }
                    return;
                }

                foreach (var input in enumerable)
                {
                    if (input == null || IsBasicType(input.GetType()))
                        continue;

                    if (TraversedObjects.Contains(input))
                        continue;

                    var output = action(input);
                    if (output != input)
                        throw new InvalidOperationException("Output must be the same as input.");
                    InternalTraverseProperties(output, action);
                }
                return;
            }

            // Browse public properties
            foreach (var property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead)
                    continue;
                if (property.GetIndexParameters().Length != 0)
                    continue;
                if (IsBasicType(property.PropertyType))
                    continue;

                object input = null;
                try
                {
                    input = property.GetValue(target, new object[0]);
                }
                catch (Exception) 
                {
                    Trace.WriteLine(string.Format("Failed getting property value {0}", property.Name));
                    continue;
                }

                if (input != null)
                {
                    if (TraversedObjects.Contains(input))
                        continue;

                    Trace.WriteLine(string.Format("{0}.{1} : {2}", targetType.Name, property.Name, input.GetType().Name));

                    object output = action(input);
                    if (input != output)
                    {
                        try
                        {
                            property.SetValue(target, output, new object[0]);
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine(string.Format("Failed setting property value {0}", property.Name));
                        }
                    }
                    else
                    {
                        InternalTraverseProperties(output, action);
                    }
                 }
            }

            // Browse attached properties
            var attachedPropertyCount = AttachablePropertyServices.GetAttachedPropertyCount(target);
            if (attachedPropertyCount > 0)
            {
                var attachedProperties = new KeyValuePair<AttachableMemberIdentifier, object>[attachedPropertyCount];
                AttachablePropertyServices.CopyPropertiesTo(target, attachedProperties, 0);

                foreach (var attachedProperty in attachedProperties)
                {
                    var input = attachedProperty.Value;
                    if (input != null && !IsBasicType(input.GetType()))
                    {
                        if (TraversedObjects.Contains(input))
                            continue;

                        Trace.WriteLine(string.Format("{0}.{1} : {2}", attachedProperty.Key.DeclaringType, attachedProperty.Key.MemberName, input.GetType().Name));

                        object output = action(input);
                        if (input != output)
                        {
                            try
                            {
                                AttachablePropertyServices.SetProperty(target, attachedProperty.Key, output);
                            }
                            catch (Exception)
                            {
                                Trace.WriteLine(string.Format("Failed setting property value {0}.{1}", attachedProperty.Key.DeclaringType, attachedProperty.Key.MemberName));
                            }
                        }
                        else
                        {
                            InternalTraverseProperties(output, action);
                        }
                    }
                }
            }
        }

        private static void ClearReflectionCache(Type recordType)
        {
            object cache = recordType.GetType().GetProperty("Cache", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(recordType, null);
            cache.GetType().GetField("m_fieldInfoCache", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic).SetValue(cache, null);

            // Somehow I need to force GC collection. The above two lines alone won't work.
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static bool IsBasicType(Type type)
        {
            return type.IsPrimitive || type.IsValueType || type.IsEnum || type == typeof(string) ||
                   type.Assembly == typeof(Microsoft.Xna.Framework.Vector3).Assembly ||
                   type.Assembly == typeof(Microsoft.Xna.Framework.Graphics.GraphicsDevice).Assembly ||
                   type.Assembly == typeof(Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor).Assembly ||
                   type.Assembly == typeof(Nine.Physics.RigidBody).Assembly ||
                   ((type.Assembly == typeof(string).Assembly || type.Assembly == typeof(Stack).Assembly) &&
                    !type.FullName.StartsWith("System.Collections.") && !type.IsInterface && !type.IsAbstract && type != typeof(object));
        }

        static HashSet<object> TraversedObjects = new HashSet<object>();
    }
}
