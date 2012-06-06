#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Nine.Studio.Extensibility
{
    /// <summary>
    /// Represents an interface that is used to visualize a document of the given type.
    /// </summary>
    public interface IDocumentVisualizer
    {
        /// <summary>
        /// Gets the type of object that can be visualized.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Visualize the target object.
        /// </summary>
        /// <returns>
        /// A host container. It could be a WinForm or WPF control.
        /// </returns>
        object Visualize(object targetObject);
    }

    /// <summary>
    /// Generic base class implementing IDocumentVisualizer
    /// </summary>
    public abstract class DocumentVisualizer<T> : IDocumentVisualizer
    {
        public Type TargetType
        {
            get { return typeof(T); }
        }

        public object Visualize(object targetObject)
        {
            if (targetObject is T)
                return Visualize((T)targetObject);
            return null;
        }

        protected abstract object Visualize(T targetObject);
    }
}
