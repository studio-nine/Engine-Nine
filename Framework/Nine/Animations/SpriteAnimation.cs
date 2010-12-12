#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Animations
{
    using Nine.Graphics;

    /// <summary>
    /// An animation player that plays ImageList based sprite animations.
    /// </summary>
    public class SpriteAnimation : KeyframeAnimation
    {
        int count;
        int startFrame = 0;

        protected override int GetTotalFrames()
        {
            return count;
        }

        public ImageList ImageList { get; private set; }
        public Texture2D Texture { get { return ImageList[CurrentFrame + startFrame].Texture; } }
        public Rectangle SourceRectangle { get { return ImageList[CurrentFrame + startFrame].SourceRectangle; } }

        public SpriteAnimation()
        {
            ImageList = new ImageList();
        }

        public SpriteAnimation(IEnumerable<Texture2D> textures)
        {
            ImageList = new ImageList();

            foreach (Texture2D texture in textures)
            {
                ImageList.Add(texture, texture.Bounds);
            }

            count = ImageList.Count;
        }

        public SpriteAnimation(ImageList imageList)
        {
            if (imageList == null)
                throw new ArgumentNullException();

            ImageList = imageList;

            count = imageList.Count;
        }

        public SpriteAnimation(ImageList imageList, int startFrame, int count)
        {
            if (imageList == null)
                throw new ArgumentNullException();

            ImageList = imageList;

            this.startFrame = startFrame;
            this.count = count;
        }
    }
}
