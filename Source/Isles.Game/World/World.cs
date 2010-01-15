#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Isles.Graphics;
using Isles.Graphics.Models;
using Isles.Transitions;
#endregion


namespace Isles.Game.World
{
    public class World
    {
        public ICollection<object> WorldObjects { get { return worldObjects; } }

        internal EnumerationCollection<object, LinkedList<object>> worldObjects = new EnumerationCollection<object, LinkedList<object>>();
    }


    [XmlLoader(typeof(World))]
    public class WorldLoader : IXmlLoader
    {
        public object Load(XmlElement input, IServiceProviderEx services)
        {
            World world = new World();


            foreach (XmlNode childNode in input.SelectSingleNode("WorldObjects").ChildNodes)
            {
                if (childNode is XmlElement)
                {
                    object child = services.GetService<IXmlLoader>(null).Load(childNode as XmlElement, services);

                    if (child != null)
                        world.worldObjects.Add(child);
                }
            }

            return world;
        }
    }
}