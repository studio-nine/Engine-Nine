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
#endregion


namespace Isles.Components
{
    public class World : IDisplayObject
    {
        [Loader(IsService=true)]
        public ModelBatch ModelBatch { get; private set; }
        public ICollection<object> WorldObjects { get { return worldObjects; } }
        public Matrix Transform { get; set; }


        #region WorldObjects
        [Loader("WorldObjects", Serializer=typeof(WorldObjectsLoader))]
        internal EnumerationCollection<object, LinkedList<object>> worldObjects = 
             new EnumerationCollection<object, LinkedList<object>>();
        
        internal class WorldObjectsLoader : IXmlLoader
        {
            public object Load(XmlElement input, IServiceProvider services)
            {
                EnumerationCollection<object, LinkedList<object>> worldObjects =
                    new EnumerationCollection<object, LinkedList<object>>();

                XmlLoader loader = new XmlLoader();

                foreach (XmlNode childNode in input.ChildNodes)
                {
                    if (childNode is XmlElement)
                    {
                        object child = loader.Load(childNode as XmlElement, services);

                        if (child != null)
                            worldObjects.Add(child);
                    }
                }

                return worldObjects;
            }
        }
        #endregion


        public World()
        {
            ModelBatch = new ModelBatch();
        }

        public T GetObject<T>()
        {
            foreach (object obj in worldObjects)
                if (obj is T)
                    return (T)obj;

            return default(T);
        }

        public IEnumerable<T> GetObjects<T>()
        {
            foreach (object obj in worldObjects)
                if (obj is T)
                    yield return (T)obj;
        }

        public virtual void Update(GameTime gameTime)
        {
            foreach (object o in worldObjects)
            {
                IUpdateObject tick = o as IUpdateObject;

                if (tick != null)
                    tick.Update(gameTime);
            }
        }

        public virtual void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            ModelBatch.Begin();

            foreach (object o in worldObjects)
            {
                IDisplayObject disp = o as IDisplayObject;

                if (disp != null)
                    disp.Draw(gameTime, view, projection);
            }

            ModelBatch.End();
        }
    }
}