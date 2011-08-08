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
    /// An animation player that plays TextureList based sprite animations.
    /// </summary>
    public class SpriteAnimation : KeyframeAnimation
    {
        int count;
        int startFrame = 0;

        protected override int GetTotalFrames()
        {
            return count;
        }

        public TextureList TextureList { get; private set; }
        public Texture2D Texture { get { return TextureList[CurrentFrame + startFrame].Texture; } }
        public Rectangle SourceRectangle { get { return TextureList[CurrentFrame + startFrame].SourceRectangle; } }

        public SpriteAnimation()
        {
            TextureList = new TextureList();
        }

        public SpriteAnimation(IEnumerable<Texture2D> textures)
        {
            TextureList = new TextureList();

            foreach (Texture2D texture in textures)
            {
                TextureList.Add(texture, texture.Bounds);
            }

            count = TextureList.Count;
        }

        public SpriteAnimation(TextureList imageList)
        {
            if (imageList == null)
                throw new ArgumentNullException();

            TextureList = imageList;

            count = imageList.Count;
        }

        public SpriteAnimation(TextureList imageList, int startFrame, int count)
        {
            if (imageList == null)
                throw new ArgumentNullException();

            TextureList = imageList;

            this.startFrame = startFrame;
            this.count = count;
        }
    }
}
