#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
#endregion

namespace Nine.Navigation
{
    /// <summary>
    /// Defines a navigation component that can be added to a game object container.
    /// </summary>
    [Serializable]
    public class NavigatorComponent : Component, IUpdateable
    {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the template of the navigator.
        /// </summary>
        [XmlAttribute]
        public string Template { get; set; }

        /// <summary>
        /// Gets the underlying navigator.
        /// </summary>
        [XmlIgnore]
        public Navigator Navigator { get; private set; }

        protected override void OnAdded(WorldObject parent)
        {
            CreateNavigator();
        }

        /// <summary>
        /// Creates the navigator.
        /// </summary>
        internal void CreateNavigator()
        {
            Navigator = new Navigator();
            Navigator.Ground = Parent.Find<ISurface>();
            Navigator.PathGraph = Parent.Find<IPathGraph>();
            Navigator.Friends = Parent.Find<ISpatialQuery<Navigator>>();
            Navigator.Opponents = Parent.Find<ISpatialQuery<Navigator>>();            
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime)
        {
            if (Navigator != null)
            {
                Navigator.Update(elapsedTime);

                if (Navigator.State == NavigatorState.Moving)
                {
                    var transformable = Parent as ITransformable;
                    if (transformable != null)
                        transformable.Transform = Navigator.Transform;
                }
            }
        }
    }
}
