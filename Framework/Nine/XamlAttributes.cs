#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
#endregion

namespace System.Windows.Markup
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ContentPropertyAttribute : Attribute
    {
        public ContentPropertyAttribute() { }
        public ContentPropertyAttribute(string name) { Name = name; }
        public string Name { get; private set; }
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