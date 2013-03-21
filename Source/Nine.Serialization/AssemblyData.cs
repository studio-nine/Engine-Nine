namespace Nine.Serialization.CodeGeneration
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Diagnostics;

    class AssemblyData
    {
        public readonly string GeneratorName;
        public readonly string GeneratorVersion;

        public readonly Assembly Assembly;
        public readonly ReadOnlyCollection<TypeData> Types;

        public Dictionary<Type, MethodInfo> ConstructorMethods;

        public Dictionary<Type, ReadMethodData> BinaryReaderMethods;
        public Dictionary<Type, WriteMethodData> BinaryWriterMethods;

        public Dictionary<Assembly, int> AssemblyOrders = new Dictionary<Assembly, int>();
        public Dictionary<Type, PropertyTypeData> PropertyTypes = new Dictionary<Type, PropertyTypeData>();
        public Dictionary<Type, KeyValuePair<string, long>> EnumTypeAndOffset = new Dictionary<Type, KeyValuePair<string, long>>();

        public AssemblyData(Assembly assembly)
        {
            var name = GetType().Assembly.GetName();

            this.GeneratorName = name.Name;
            this.GeneratorVersion = name.Version.ToString();

            BuildAssemblyOrder(assembly);

            this.ConstructorMethods = (
                from method in FindExtensionMethods(typeof(IServiceProvider))
                where method.ReturnType != typeof(void) && !method.IsGenericMethod &&
                     !method.IsGenericMethodDefinition && !method.IsPrivate &&
                     !method.ContainsGenericParameters && method.GetParameters().Length == 1
                select method).ToDictionary(x => x.ReturnType);

            this.BinaryReaderMethods = (
                from method in FindExtensionMethods(typeof(BinaryReader))
                select new ReadMethodData(this, method, true))
                .Where(x => x.IsValid).Concat(
                from method in typeof(BinaryReader).GetMethods()
                where method.Name.StartsWith("Read") && method.Name != "Read" &&
                      method.GetParameters().Length == 0 && !method.ReturnType.IsArray
                select new ReadMethodData(this, method, false))
                .ToLookup(x => x.TargetType).ToDictionary(x => x.Key, x => x.OrderBy(a => a.AssemblyOrder).First());

            this.BinaryWriterMethods = (
                from method in FindExtensionMethods(typeof(BinaryWriter))
                select new WriteMethodData(this, method, true))
                .Where(x => x.IsValid).Concat(
                from method in typeof(BinaryWriter).GetMethods() 
                where method.Name.StartsWith("Write") &&
                      method.GetParameters().Length == 1 && !method.GetParameters()[0].ParameterType.IsArray
                select new WriteMethodData(this, method, false))
                .ToLookup(x => x.TargetType).ToDictionary(x => x.Key, x => x.OrderBy(a => a.AssemblyOrder).First());

            if (this.BinaryReaderMethods.Count != this.BinaryWriterMethods.Count)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("The number of read methods and write methods does not match");
            }

            this.Assembly = assembly;
            this.Types = new ReadOnlyCollection<TypeData>((
                from type in assembly.GetTypes() 
                where IsSerializable(type) 
                select new TypeData(this, type)).ToArray());
        }

        private void BuildAssemblyOrder(Assembly assembly)
        {
            int order;
            if (AssemblyOrders.TryGetValue(assembly, out order))
            {
                // Why GetReferencedAssemblies returns a circular graph ??
                // System referenced System.Configuration, but
                // System.Configuration also references System...
                //
                // Force a break anyway.
                if (order < 10)
                    return;
                AssemblyOrders[assembly] = order - 1;
            }
            else
            {
                AssemblyOrders.Add(assembly, 0);
            }

            foreach (var dependency in assembly.GetReferencedAssemblies())
            {
                try
                {
                    BuildAssemblyOrder(Assembly.Load(dependency));
                }
                catch { }
            }
        }

        private bool IsSerializable(Type type)
        {
            return !type.IsAbstract && !type.IsInterface && !type.IsGenericType &&
                   !HasNotBinarySerializableAttribute(type) && HasBinarySerializableAttribute(type);
        }

        private bool HasNotBinarySerializableAttribute(Type type)
        {
            return type.GetCustomAttributes(true).Any(a => a.GetType().Name == "NotBinarySerializableAttribute");
        }

        private bool HasBinarySerializableAttribute(Type type)
        {
            return type.GetCustomAttributes(true).Any(a => a.GetType().Name == "BinarySerializableAttribute");
        }

        private IEnumerable<MethodInfo> FindExtensionMethods(Type targetType)
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   from type in assembly.GetTypes()
                   where type.IsClass && type.IsSealed && type.IsAbstract && type.IsPublic
                   from method in type.GetMethods()
                   where IsExtensionMethod(method, targetType)
                   select method;
        }

        private bool IsExtensionMethod(MethodInfo method, Type targetType)
        {
            if (!method.IsStatic)
                return false;
            var parameters = method.GetParameters();
            return parameters.Length > 0 && parameters[0].ParameterType == targetType &&
                   method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false).Length > 0;
        }

        public PropertyTypeData GetPropertyType(Type type)
        {
            PropertyTypeData result;
            if (!PropertyTypes.TryGetValue(type, out result))
                PropertyTypes[type] = result = new PropertyTypeData(this, type);
            return result;
        }

        public void GetEnumTypeAndOffset(Type type, out string enumType, out long enumOffset)
        {
            KeyValuePair<string, long> result;
            if (!EnumTypeAndOffset.TryGetValue(type, out result))
            {
                long min = long.MaxValue;
                long max = long.MinValue;
                
                foreach (var value in Enum.GetValues(type))
                {
                    long current = Convert.ToInt64(value);
                    min = Math.Min(current, min);
                    max = Math.Max(current, max);
                }

                long gap = max > min ? max - min : 0;
                enumType = (gap < 0 ? "UInt64" :
                           (gap <= byte.MaxValue ? "Byte" :
                           (gap <= ushort.MaxValue ? "UInt16" :
                           (gap <= uint.MaxValue ? "UInt32" : "UInt64"))));
                EnumTypeAndOffset[type] = result = new KeyValuePair<string, long>(enumType, min);
            }
            enumType = result.Key;
            enumOffset = result.Value;
        }
    }

    partial class BinaryObjectWriter
    {
        AssemblyData g;
        public BinaryObjectWriter(AssemblyData g) { this.g = g; }
    }

    partial class BinaryObjectReader
    {
        AssemblyData g;
        public BinaryObjectReader(AssemblyData g) { this.g = g; }
    }
}