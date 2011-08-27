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
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a template factory that can create a template of the specified
    /// type based on the name of the template.
    /// </summary>
    [Serializable]
    public class Template
    {
        /// <summary>
        /// Gets the factory name of this template.
        /// </summary>
        public string FactoryName { get; private set; }

        /// <summary>
        /// Gets the name of this template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the value of this template.
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// Create a new instance of Template object from string.
        /// </summary>
        public static implicit operator Template(string value)
        {
            var index = value.IndexOf(':');
            if (index <= 0)
                return new Template() { FactoryName = "Content", Name = value };
            return new Template() { FactoryName = value.Substring(0, index), Name = value.Substring(index + 1) };
        }

        public override string ToString()
        {
            return FactoryName + ":" + Name;
        }
    }
}