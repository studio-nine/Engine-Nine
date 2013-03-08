namespace Nine.Serialization.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    
    [DebuggerDisplay("{Member}")]
    class PropertyData
    {
        public readonly string Name;
        public readonly MemberInfo Member;
        public readonly PropertyTypeData Type;

        internal readonly bool IsSerializable = false;

        public PropertyData(AssemblyData g, MemberInfo member)
        {
            this.Member = member;
            this.Name = member.Name;

            if (HasNotBinarySerializableAttribute(member))
                return;

            var forceSerialize = HasBinarySerializableAttribute(member);

            var field = member as FieldInfo;
            if (field != null)
            {
                if (forceSerialize || (field.IsPublic && CanSerialize(field.FieldType)))
                {
                    this.Type = g.GetPropertyType(field.FieldType);
                    this.IsSerializable = this.Type.Read != null && this.Type.Write != null;
                }
            }

            var property = member as PropertyInfo;
            if (property != null)
            {
                if (property.GetIndexParameters().Length > 0 || !CanSerialize(property.PropertyType))
                    return;

                var get = property.GetGetMethod(true);
                if (get == null)
                    return;

                if (forceSerialize || get.IsPublic)
                {
                    this.Type = g.GetPropertyType(property.PropertyType);
                    this.IsSerializable = this.Type.Read != null && this.Type.Write != null;
                    if (this.IsSerializable)
                    {
                        var set = property.GetSetMethod(true);
                        if (set == null)
                            this.IsSerializable = this.Type.Read.ReadIntoExistingInstance;
                        else if (!set.IsPublic)
                            this.IsSerializable = this.Type.Read.ReadIntoExistingInstance || forceSerialize;
                    }
                }
            }
        }

        private bool ShouldSerialize(MemberInfo member)
        {
            var property = member as PropertyInfo;
            if (property != null)
            {
                if (property.GetIndexParameters().Length > 0 || !CanSerialize(property.PropertyType))
                    return false;

                var get = property.GetGetMethod();
                return get != null && get.IsPublic;
            }
            return false;
        }

        private bool CanSerialize(Type type)
        {
            return !typeof(Delegate).IsAssignableFrom(type);
        }

        private bool HasNotBinarySerializableAttribute(MemberInfo member)
        {
            return member.GetCustomAttributes(true).Any(a => a.GetType().Name == "NotBinarySerializableAttribute");
        }

        private bool HasBinarySerializableAttribute(MemberInfo member)
        {
            return member.GetCustomAttributes(true).Any(a => a.GetType().Name == "BinarySerializableAttribute");
        }
    }
}