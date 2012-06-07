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
using System.Reflection;
using System.Resources;
using System.ComponentModel.Composition;
#endregion

namespace Nine.Studio.Extensibility
{
    [MetadataAttribute]
    public class DefaultAttribute : Attribute
    {
        public bool IsDefault { get { return true; } }
    }

    [MetadataAttribute]
    public class FolderNameAttribute : Attribute
    {
        public FolderNameAttribute() { }
        public FolderNameAttribute(string folderName) { this.FolderName = folderName; }
        
        public string FolderName
        {
            get { return folderName; }
            set { Verify.IsValidPath(value, "value"); folderName = value; }
        }
        private string folderName;        
    }

    [MetadataAttribute]
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

    [MetadataAttribute]
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

    [MetadataAttribute]
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

    [MetadataAttribute]
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
