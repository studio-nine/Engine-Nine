namespace Nine.Animations
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    
    /// <summary>
    /// An animation player that plays TextureList based sprite animations.
    /// </summary>
    [ContentSerializable]
    public class SpriteAnimation : KeyframeAnimation, ISupportTarget
    {
        private object target;
        private string targetProperty;
        private bool expressionChanged;
        private PropertyExpression<TextureListItem> expression;

        /// <summary>
        /// Gets or sets the texture list used by this <see cref="SpriteAnimation"/>.
        /// </summary>
        public TextureList Source
        {
            get { return source; }
            set { source = value; if (source != null) TotalFrames = source.Count; }
        }
        private TextureList source;

        /// <summary>
        /// Gets the texture for this <see cref="SpriteAnimation"/>.
        /// </summary>
        public Texture2D Texture
        {
            get { return source != null ? source[CurrentFrame].Texture : null; } 
        }

        /// <summary>
        /// Gets the current source rectangle.
        /// </summary>
        public Rectangle SourceRectangle
        {
            get { return source != null ? source[CurrentFrame].SourceRectangle : new Rectangle(); } 
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
            Source = new TextureList();
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

            Source = list;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation(TextureList textureList)
        {
            if (textureList == null)
                throw new ArgumentNullException();

            Source = textureList;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation(TextureList textureList, int beginFrame, int frameCount)
        {
            if (textureList == null)
                throw new ArgumentNullException();

            Source = textureList;

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
