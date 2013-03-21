namespace Nine.Serialization.CodeGeneration
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [DebuggerDisplay("{Type}")]
    class TypeData
    {
        public readonly Type Type;
        public readonly string Name;
        public readonly string Namespace;
        public readonly bool SupportsInitialize;
        public readonly bool HasDefaultConstructor;
        public readonly bool HasServiceProviderConstructor;
        public readonly MethodInfo[] ConstructorParameters;
        public readonly ReadOnlyCollection<PropertyData> Properties;
        
        static BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public TypeData(AssemblyData g, Type type)
        {
            this.Type = type;
            this.Name = type.Name;
            this.Namespace = type.Namespace;
            this.SupportsInitialize = typeof(ISupportInitialize).IsAssignableFrom(type);
            this.Properties = new ReadOnlyCollection<PropertyData>((
                from member in type.GetMembers(BindingFlags)
                select new PropertyData(g, member)).Where(x => x.IsSerializable).ToArray());

            var constructors = type.GetConstructors(BindingFlags);
            this.HasDefaultConstructor = constructors.Any(x => x.GetParameters().Length == 0);
            this.HasServiceProviderConstructor = constructors.Any(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(IServiceProvider));

            if (!HasDefaultConstructor && !HasServiceProviderConstructor)
            {
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length < 1)
                        continue;
                    if (parameters.All(p => g.ConstructorMethods.ContainsKey(p.ParameterType)))
                    {
                        ConstructorParameters = parameters.Select(p => g.ConstructorMethods[p.ParameterType]).ToArray();
                        break;
                    }
                }
            }
        }
    }
}