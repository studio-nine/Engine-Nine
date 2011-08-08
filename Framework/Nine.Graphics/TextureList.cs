#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    #region TextureListItem
    /// <summary>
    /// A single instance of image.
    /// </summary>
    public class TextureListItem
    {
        public Texture2D Texture { get; internal set; }
        public Rectangle SourceRectangle { get; internal set; }

        internal TextureListItem() { }
        
        public TextureListItem(Texture2D texture, Rectangle sourceRectangle)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
        }
    }
    #endregion

    #region TextureList
    /// <summary>
    /// Defines a list of textures and source rectangles.
    /// </summary>
    public class TextureList : Collection<TextureListItem>
    {
        // Store the original sprite filenames, so we can look up sprites by name.
        internal Dictionary<string, int> spriteNames;


        public void Add(Texture2D texture, Rectangle sourceRectangle)
        {
            Add(new TextureListItem(texture, sourceRectangle));
        }
        
        /// <summary>
        /// Looks up the numeric index of the specified sprite. 
        /// </summary>
        public int GetIndex(string spriteName)
        {
            int index;

            if (!spriteNames.TryGetValue(spriteName, out index))
            {
                string error = "SpriteSheet does not contain a sprite named '{0}'.";

                throw new KeyNotFoundException(string.Format(error, spriteName));
            }

            return index;
        }


        public TextureListItem this[string name] 
        {
            get { return this[GetIndex(name)]; }
        }
    }
    #endregion

    #region TextureListReader
    /// <summary>
    /// Reader for TextureList.
    /// </summary>
    internal class TextureListReader : ContentTypeReader<TextureList>
    {
        protected override TextureList Read(ContentReader input, TextureList existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new TextureList();

            Texture2D[] textures = input.ReadObject<Texture2D[]>();
            Rectangle[] rectangles = input.ReadObject<Rectangle[]>();
            int[] indices = input.ReadObject<int[]>();
            existingInstance.spriteNames = input.ReadObject<Dictionary<string, int>>();

            for (int i = 0; i < indices.Length; i++)
            {
                existingInstance.Add(textures[indices[i]], rectangles[i]);
            }

            return existingInstance;
        }
    }
    #endregion
}
