namespace Nine.Serialization.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    
    [DebuggerDisplay("{Type}")]
    class PropertyTypeData
    {
        public readonly Type Type;
        public readonly string EnumType;
        public readonly long EnumOffset;
        public readonly ReadMethodData Read;
        public readonly WriteMethodData Write;
        public PropertyTypeData[] GenericTypes;

        public PropertyTypeData(AssemblyData g, Type type)
        {
            this.Type = type;
            if (type.IsEnum)
                g.GetEnumTypeAndOffset(type, out this.EnumType, out this.EnumOffset);
            if ((this.Read = FindExactMatch(g, g.BinaryReaderMethods, type)) == null)
                this.Read = FindBestMatch(g, g.BinaryReaderMethods, type);
            if ((this.Write = FindExactMatch(g, g.BinaryWriterMethods, type)) == null)
                this.Write = FindBestMatch(g, g.BinaryWriterMethods, type);
        }

        private T FindBestMatch<T>(AssemblyData g, IDictionary<Type, T> dictionary, Type type)
        {
            if (type == null)
                return default(T);

            if (type == typeof(object))
                return FindExactMatch(g, dictionary, typeof(object));

            // BFS Interface Traversal
            Queue<Type> typeQueue = new Queue<Type>();
            foreach (var topLevelInterface in GetTopLevelInterfaces(type))
                typeQueue.Enqueue(topLevelInterface);

            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                var result = FindExactMatch(g, dictionary, baseType);
                if (result != null)
                    return result;
                foreach (var topLevelInterface in GetTopLevelInterfaces(baseType))
                    typeQueue.Enqueue(topLevelInterface);
                baseType = baseType.BaseType;
            }

            while (typeQueue.Count > 0)
            {
                var currentType = typeQueue.Dequeue();
                var result = FindExactMatch(g, dictionary, currentType);
                if (result != null)
                    return result;
                foreach (var topLevelInterface in GetTopLevelInterfaces(currentType))
                    typeQueue.Enqueue(topLevelInterface);
            }

            return FindExactMatch(g, dictionary, typeof(object));
        }

        private T FindExactMatch<T>(AssemblyData g, IDictionary<Type, T> dictionary, Type type)
        {
            T result;

            if (type == null)
                return default(T);

            if (dictionary.TryGetValue(type, out result))
                return result;

            if (type.IsGenericType && dictionary.TryGetValue(type.GetGenericTypeDefinition(), out result))
            {
                GenericTypes = type.GetGenericArguments().Select(t => g.GetPropertyType(t)).ToArray();
                return result;
            }

            if (type.IsArray && dictionary.TryGetValue(typeof(Array), out result))
            {
                GenericTypes = new [] { g.GetPropertyType(type.GetElementType()) };
                return result;
            }

            return default(T);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/2518762/how-do-i-enumerate-a-list-of-interfaces-that-are-directly-defined-on-an-inheriti
        /// </summary>
        public static Type[] GetTopLevelInterfaces(Type t)
        {
            var allInterfaces = t.GetInterfaces();
            var selection = allInterfaces
                .Where(x => !allInterfaces.Any(y => y.GetInterfaces().Contains(x)));
            if (t.BaseType != null)
                selection = selection.Except(t.BaseType.GetInterfaces());
            return selection.ToArray();
        }

        public string ToWriteString(string output, string member, int level)
        {
            if (Write == null)
                return "";
            
            var buffer = new StringBuilder();
            if (Type.IsEnum)
            {
                buffer.Append(output);
                buffer.Append(".Write((System.");
                buffer.Append(EnumType);
                buffer.Append(")(");
                buffer.Append(member);
                buffer.Append(" - ");
                buffer.Append(EnumOffset);
                buffer.Append("));");
            }
            else if (Write.IsExtensionMethod)
            {
                buffer.Append(Write.MethodName);
                buffer.Append("(");
                buffer.Append(output);
                buffer.Append(", ");

                buffer.Append("(");
                buffer.Append(GetClassFullName(Type));
                buffer.Append(")");

                buffer.Append(member);

                if (GenericTypes != null)
                {
                    foreach (var genericType in GenericTypes)
                    {
                        level++;
                        member = "v" + level;
                        buffer.AppendLine();
                        buffer.Append(", (");
                        buffer.Append(member);
                        buffer.Append(") => { ");
                        buffer.Append(genericType.ToWriteString(output, member, level));
                        buffer.Append(" }");
                    }
                }

                if (Write.HasServiceProvider)
                    buffer.Append(", serviceProvider);");
                else
                    buffer.Append(");");
            }
            else
            {
                buffer.Append(output);
                buffer.Append(".");
                buffer.Append(Write.MethodName);
                buffer.Append("(");
                buffer.Append(member);

                if (Write.HasServiceProvider)
                    buffer.Append(", serviceProvider);");
                else
                    buffer.Append(");");
            }

            return buffer.ToString();
        }

        public string ToReadString(string input, string member, int level)
        {
            if (Read == null)
                return "";

            var buffer = new StringBuilder();

            if (Type.IsEnum)
            {
                if (member != null)
                {
                    buffer.Append(member);
                    buffer.Append(" = ");
                }

                buffer.Append("(");
                buffer.Append(GetClassFullName(Type));
                buffer.Append(")(");

                buffer.Append(input);
                buffer.Append(".Read");
                buffer.Append(EnumType);
                buffer.Append("(");
                buffer.Append(") + ");
                buffer.Append(EnumOffset);
                buffer.Append(");");
            }
            else if (Read.IsExtensionMethod)
            {
                if (member != null && Read.HasReturnValue && level <= 0)
                {
                    buffer.Append(member);
                    buffer.Append(" = ");
                }

                if (Read.HasReturnValue)
                {
                    buffer.Append("(");
                    buffer.Append(GetClassFullName(Type));
                    buffer.Append(")");
                }

                buffer.Append(Read.MethodName);

                if (GenericTypes != null && GenericTypes.Length > 0)
                {
                    buffer.Append("<");
                    buffer.Append(string.Join(", ", GenericTypes.Select(x => GetClassFullName(x.Type))));
                    buffer.Append(">");
                }

                buffer.Append("(");
                buffer.Append(input);

                if (Read.HasExistingInstance)
                {
                    buffer.Append(", ");
                    buffer.Append(member);
                }

                if (GenericTypes != null)
                {
                    foreach (var genericType in GenericTypes)
                    {
                        level++;
                        member = "v" + level;
                        buffer.AppendLine();
                        buffer.Append(", (");
                        buffer.Append(member);
                        buffer.Append(") => { return ");
                        buffer.Append(genericType.ToReadString(input, member, level));
                        buffer.Append(" }");
                    }
                }

                if (Read.HasServiceProvider)
                    buffer.Append(", serviceProvider);");
                else
                    buffer.Append(");");
            }
            else
            {
                if (member != null)
                {
                    buffer.Append(member);
                    buffer.Append(" = ");
                }

                buffer.Append("(");
                buffer.Append(GetClassFullName(Type));
                buffer.Append(")");

                buffer.Append(input);
                buffer.Append(".");
                buffer.Append(Read.MethodName);
                buffer.Append("(");

                if (Read.HasServiceProvider)
                    buffer.Append("serviceProvider);");
                else
                    buffer.Append(");");
            }

            return buffer.ToString();
        }

        private static Dictionary<Type, string> ClassFullNames = new Dictionary<Type, string>();

        public static string GetClassFullName(Type type)
        {
            string result;
            if (!ClassFullNames.TryGetValue(type, out result))
                ClassFullNames[type] = result = GetClassFullNameInternal(type);
            return result;
        }

        private static string GetClassFullNameInternal(Type type)
        {
            var count = 0;
            var name = type.Name;
            var index = name.IndexOf('`');
            if (index >= 0)
            {
                name = name.Remove(index, 1);
                count = Convert.ToInt32(name[index].ToString());
                name = name.Remove(index, 1);
            }
            if (type.IsGenericTypeDefinition)
            {
                if (count == 1)
                {
                    name = name + "<T>";
                }
                else if (count > 1)
                {
                    name += "<";
                    for (var i = 0; i < count; i++)
                    {
                        name += "T" + (i + 1);
                        if (i != count - 1)
                            name += ", ";
                    }
                    name += ">";
                }
            }
            else if (type.IsGenericType)
            {
                name += "<";
                for (var i = 0; i < count; i++)
                {
                    name += GetClassFullName(type.GetGenericArguments()[i]);
                    if (i != count - 1)
                        name += ", ";
                }
                name += ">";
            }
            if (type.IsNested)
                name = GetClassFullName(type.DeclaringType) + "." + name;
            return (type.IsGenericParameter || type.IsNested) ? name : "global::" + type.Namespace + "." + name;
        }
    }
}