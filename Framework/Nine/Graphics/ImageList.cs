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
    #region ImageListItem
    /// <summary>
    /// A single instance of image.
    /// </summary>
    public class ImageListItem
    {
        public Texture2D Texture { get; internal set; }
        public Rectangle SourceRectangle { get; internal set; }

        internal ImageListItem() { }
        
        public ImageListItem(Texture2D texture, Rectangle sourceRectangle)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
        }
    }
    #endregion

    #region ImageList
    /// <summary>
    /// Defines a list of textures and source rectangles.
    /// </summary>
    public class ImageList : Collection<ImageListItem>
    {
        // Store the original sprite filenames, so we can look up sprites by name.
        internal Dictionary<string, int> spriteNames;


        public void Add(Texture2D texture, Rectangle sourceRectangle)
        {
            Add(new ImageListItem(texture, sourceRectangle));
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


        public ImageListItem this[string name] 
        {
            get { return this[GetIndex(name)]; }
        }
    }
    #endregion

    #region ImageListReader
    /// <summary>
    /// Reader for ImageList.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ImageListReader : ContentTypeReader<ImageList>
    {
        protected override ImageList Read(ContentReader input, ImageList existingInstance)
        {
            ImageList list = new ImageList();

            Texture2D[] textures = input.ReadObject<Texture2D[]>();
            Rectangle[] rectangles = input.ReadObject<Rectangle[]>();
            int[] indices = input.ReadObject<int[]>();
            list.spriteNames = input.ReadObject<Dictionary<string, int>>();

            for (int i = 0; i < indices.Length; i++)
            {
                list.Add(textures[indices[i]], rectangles[i]);
            }

            return list;
        }
    }
    #endregion
}
