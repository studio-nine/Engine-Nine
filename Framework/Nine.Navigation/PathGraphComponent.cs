#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Navigation
{
    /// <summary>
    /// Defines a path graph component that contains a path graph.
    /// </summary>
    [Serializable]
    public class PathGraphComponent : Component
    {
        /// <summary>
        /// Gets or sets the referece of this navigation graph.
        /// </summary>
        [XmlAttribute]
        public string Source { get; set; }

        /// <summary>
        /// Gets the underlying path graph.
        /// </summary>
        public IPathGraph PathGraph
        {
            get 
            {
                if (pathGraph == null && !string.IsNullOrEmpty(Source))
                {
                    var contentManager = Parent.Find<ContentManager>();
                    if (contentManager == null)
                        throw new InvalidOperationException(string.Format(Strings.ServiceNotFound, typeof(ContentManager)));
                    pathGraph = contentManager.Load<IPathGraph>(Source);                    
                }
                return pathGraph;
            }
        }
        private IPathGraph pathGraph;
    }
}
