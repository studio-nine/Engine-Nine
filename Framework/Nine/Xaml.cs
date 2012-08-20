namespace System.Windows.Markup
{
#if !SILVERLIGHT
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ContentPropertyAttribute : Attribute
    {
        public ContentPropertyAttribute() { }
        public ContentPropertyAttribute(string name) { Name = name; }
        public string Name { get; private set; }
    }
#endif

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(string name) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RuntimeNamePropertyAttribute : Attribute
    {
        public RuntimeNamePropertyAttribute(string name) { Name = name; }
        public string Name { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DictionaryKeyPropertyAttribute : Attribute
    {
        public DictionaryKeyPropertyAttribute(string name) { Name = name; }
        public string Name { get; private set; }
    }
}