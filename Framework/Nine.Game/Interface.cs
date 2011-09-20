#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace Nine
{
    /// <summary>
    /// Defines the protocal for game objects to interact with each other.
    /// </summary>
    public interface IGameObject
    {
        /// <summary>
        /// Gets or sets the parent of this game object. 
        /// </summary>
        /// <remarks>
        /// After this game object is added to the parent container object, 
        /// the parent object is responsable for setting the Parent property
        /// of the child game object. 
        /// This property should never be modified elsewhere.
        /// You can always trust a valid parent is set when implementing a 
        /// game object.
        /// </remarks>
        IGameObjectContainer Parent { get; set; }
    }

    /// <summary>
    /// Defines the protocal for game objects to interact with each other.
    /// </summary>
    public interface IGameObjectContainer : IGameObject
    {
        /// <summary>
        /// Find the first feature of type T owned by this game object container.
        /// </summary>
        T Find<T>();

        /// <summary>
        /// Find all the feature of type T owned by this game object container.
        /// </summary>
        IEnumerable<T> FindAll<T>();
    }

    /// <summary>
    /// Defines a factory that can create an instance of a object based on the
    /// specified type name.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates a new instance of the object with the specified type name.
        /// </summary>
        object Create(string typeName);
    }
}
