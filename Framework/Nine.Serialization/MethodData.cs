namespace Nine.Serialization.CodeGeneration
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    
    [DebuggerDisplay("{MethodInfo}")]
    abstract class MethodData
    {
        public readonly MethodInfo MethodInfo;
        public readonly bool IsExtensionMethod;
        public readonly int AssemblyOrder;

        public Type TargetType;
        public string MethodName;
        public bool IsValid = true;
        public bool HasServiceProvider = false;
        public bool IsGeneric = false;

        public MethodData(AssemblyData g, MethodInfo method, bool isExtensionMethod)
        {
            this.MethodInfo = method;
            this.IsExtensionMethod = isExtensionMethod;

            if (!g.AssemblyOrders.TryGetValue(method.DeclaringType.Assembly, out AssemblyOrder))
                AssemblyOrder = int.MinValue;
        }
    }

    class ReadMethodData : MethodData
    {
        public readonly bool HasReturnValue;
        public readonly bool HasExistingInstance;
        public readonly bool ReadIntoExistingInstance;

        public ReadMethodData(AssemblyData g, MethodInfo method, bool isExtensionMethod)
            : base(g, method, isExtensionMethod)
        {
            if (isExtensionMethod)
            {
                this.HasReturnValue = method.ReturnType != typeof(void);

                var parameters = method.GetParameters();
                if (!method.IsGenericMethodDefinition)
                {
                    this.IsValid =
                        (parameters.Length == 1 ||
                        (parameters.Length == 2 && parameters[1].ParameterType == typeof(IServiceProvider) && this.HasReturnValue) ||
                        (parameters.Length == 3 && parameters[2].ParameterType == typeof(IServiceProvider)));
                    this.HasExistingInstance = parameters.Length == 3;
                    this.HasServiceProvider = IsValid && parameters.Length >= 2;
                    this.TargetType = this.HasReturnValue ? method.ReturnType : parameters[1].ParameterType;
                }
                else
                {
                    var genericArguments = method.GetGenericArguments();
                    if (this.IsValid = (
                        parameters.Length == 2 + genericArguments.Length &&
                        parameters.Skip(2).All(p => p.ParameterType.IsGenericType &&
                            p.ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))))
                    {
                        this.HasServiceProvider = false;
                        this.IsGeneric = true;
                        this.HasExistingInstance = true;
                        this.TargetType = this.HasReturnValue ?
                              (method.ReturnType.IsArray ? typeof(Array) : method.ReturnType.GetGenericTypeDefinition()) 
                            : parameters[1].ParameterType.GetGenericTypeDefinition();
                    }
                    else if (this.IsValid = (
                        parameters.Length == 1 + genericArguments.Length &&
                        parameters.Skip(1).All(p => p.ParameterType.IsGenericType &&
                            p.ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))))
                    {
                        this.HasServiceProvider = false;
                        this.IsGeneric = true;
                        this.TargetType = method.ReturnType.IsArray ? typeof(Array) : method.ReturnType.GetGenericTypeDefinition();
                    }
                }
            }
            else
            {
                this.HasReturnValue = true;
                this.TargetType = method.ReturnType;
            }

            if (!this.HasReturnValue && !this.HasExistingInstance)
                this.IsValid = false;

            this.ReadIntoExistingInstance = !this.HasReturnValue && this.HasExistingInstance;
            this.MethodName = isExtensionMethod ? method.DeclaringType.FullName + "." + method.Name : method.Name;
        }
    }

    class WriteMethodData : MethodData
    {
        public WriteMethodData(AssemblyData g, MethodInfo method, bool isExtensionMethod)
            : base(g, method, isExtensionMethod)
        {
            if (isExtensionMethod)
            {
                var parameters = method.GetParameters();
                if (!method.IsGenericMethodDefinition)
                {
                    if (this.IsValid = method.ReturnType == typeof(void) &&
                        (parameters.Length == 2 ||
                        (parameters.Length == 3 && parameters[2].ParameterType == typeof(IServiceProvider))))
                    {
                        this.HasServiceProvider = parameters.Length == 3;
                        this.TargetType = parameters[1].ParameterType;
                    }
                }
                else
                {
                    var genericArguments = method.GetGenericArguments();
                    if (this.IsValid = (method.ReturnType == typeof(void) &&
                        parameters.Length == 2 + genericArguments.Length &&
                        parameters.Skip(2).All(p => p.ParameterType.IsGenericType && 
                            p.ParameterType.GetGenericTypeDefinition() == typeof(Action<>))))
                    {
                        this.HasServiceProvider = false;
                        this.IsGeneric = true;
                        this.TargetType = parameters[1].ParameterType.IsArray ? typeof(Array) : parameters[1].ParameterType.GetGenericTypeDefinition();
                    }
                }
            }
            else
            {
                this.TargetType = method.GetParameters()[0].ParameterType;
            }
            this.MethodName = isExtensionMethod ? method.DeclaringType.FullName + "." + method.Name : method.Name;
        }
    }
}