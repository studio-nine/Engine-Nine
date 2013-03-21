namespace Nine.Studio.Extensibility
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Resources;

    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string resourceName)
            : base(Strings.ResourceManager.GetString(resourceName)) { }

        public LocalizedDisplayNameAttribute(string resourceName, Type resourceManagerType)
            : base(GetResourceManager(resourceManagerType).GetString(resourceName)) { }

        private static ResourceManager GetResourceManager(Type resourceManagerType)
        {
            return resourceManagerType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                      .GetValue(null, null) as ResourceManager;
        }
    }

    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string resourceName)
            : base(Strings.ResourceManager.GetString(resourceName)) { }

        public LocalizedDescriptionAttribute(string resourceName, Type resourceManagerType)
            : base(GetResourceManager(resourceManagerType).GetString(resourceName)) { }

        private static ResourceManager GetResourceManager(Type resourceManagerType)
        {
            return resourceManagerType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                      .GetValue(null, null) as ResourceManager;
        }
    }

    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        public LocalizedCategoryAttribute(string resourceName)
            : base(Strings.ResourceManager.GetString(resourceName)) { }

        public LocalizedCategoryAttribute(string resourceName, Type resourceManagerType)
            : base(GetResourceManager(resourceManagerType).GetString(resourceName)) { }

        private static ResourceManager GetResourceManager(Type resourceManagerType)
        {
            return resourceManagerType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                      .GetValue(null, null) as ResourceManager;
        }
    }

    public class LocalizedDefaultValueAttribute : DefaultValueAttribute
    {
        public LocalizedDefaultValueAttribute(string resourceName)
            : base(Strings.ResourceManager.GetObject(resourceName)) { }

        public LocalizedDefaultValueAttribute(string resourceName, Type resourceManagerType)
            : base(GetResourceManager(resourceManagerType).GetObject(resourceName)) { }

        private static ResourceManager GetResourceManager(Type resourceManagerType)
        {
            return resourceManagerType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                      .GetValue(null, null) as ResourceManager;
        }
    }
}
