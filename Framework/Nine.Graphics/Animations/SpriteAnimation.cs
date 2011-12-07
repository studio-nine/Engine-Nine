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
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// An animation player that plays TextureList based sprite animations.
    /// </summary>
    public class SpriteAnimation : KeyframeAnimation, ISupportTarget
    {
        private object target;
        private string targetProperty;
        private bool expressionChanged;
        private PropertyExpression<TextureListItem> expression;

        /// <summary>
        /// Gets or sets the texture list used by this <see cref="SpriteAnimation"/>.
        /// </summary>
        [ContentSerializer]
        public TextureList TextureList
        {
            get { return textureList; }
            set { textureList = value; if (textureList != null) TotalFrames = textureList.Count; }
        }
        private TextureList textureList;

        /// <summary>
        /// Gets the texture for this <see cref="SpriteAnimation"/>.
        /// </summary>
        public Texture2D Texture
        {
            get { return textureList != null ? textureList[CurrentFrame].Texture : null; } 
        }

        /// <summary>
        /// Gets the current source rectangle.
        /// </summary>
        public Rectangle SourceRectangle
        {
            get { return textureList != null ? textureList[CurrentFrame].SourceRectangle : new Rectangle(); } 
        }

        /// <summary>
        /// Gets or sets the target that this sprite animation should affect.
        /// </summary>
        [ContentSerializerIgnore]
        public object Target
        {
            get { return target; }
            set { if (target != value) { target = value; expressionChanged = true; } }
        }

        /// <summary>
        /// Gets or sets the target property that this sprite animation should affect.
        /// The property must be of type <see cref="TextureListItem"/>.
        /// </summary>
        public string TargetProperty
        {
            get { return targetProperty; }
            set { if (targetProperty != value) { targetProperty = value; expressionChanged = true; } }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        internal SpriteAnimation()
        {
            TextureList = new TextureList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation(IEnumerable<Texture2D> textures)
        {
            var list = new TextureList();

            foreach (Texture2D texture in textures)
            {
                list.Add(texture, texture.Bounds);
            }

            TextureList = list;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation(TextureList textureList)
        {
            if (textureList == null)
                throw new ArgumentNullException();

            TextureList = textureList;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation(TextureList textureList, int beginFrame, int frameCount)
        {
            if (textureList == null)
                throw new ArgumentNullException();

            TextureList = textureList;

            BeginFrame = beginFrame;
            EndFrame = BeginFrame + frameCount;
        }

        /// <summary>
        /// Moves the animation at the position between start frame and end frame
        /// specified by percentage.
        /// </summary>
        protected override void OnSeek(int startFrame, int endFrame, float percentage)
        {
            base.OnSeek(startFrame, endFrame, percentage);

            if (Target != null && !string.IsNullOrEmpty(TargetProperty))
            {
                if (expression == null || expressionChanged)
                {
                    expression = new PropertyExpression<TextureListItem>(Target, TargetProperty);
                    expressionChanged = false;
                }

                expression.Value = new TextureListItem(Texture, SourceRectangle);
            }
        }
    }
}
