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

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// Specifies the default process for a content type. The default processor will be used to 
    /// process the content when processing using DefaultContentProcessor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple=false)]
    public class DefaultContentProcessorAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the assembly quanlified name of the default processor.
        /// </summary>
        public string DefaultProcessor { get; set; }

        public DefaultContentProcessorAttribute() { }
        public DefaultContentProcessorAttribute(string defaultProcessor) { DefaultProcessor = defaultProcessor; }
        public DefaultContentProcessorAttribute(Type defaultProcessorType) { DefaultProcessor = defaultProcessorType.AssemblyQualifiedName; }
    }
}
