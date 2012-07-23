namespace Nine
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    class ContentSerializableAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    class NotContentSerializableAttribute : Attribute { }
}