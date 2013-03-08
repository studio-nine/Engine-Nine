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
    [Nine.Serialization.BinarySerializable]
    public class SpriteAnimation : KeyframeAnimation, ISupportTarget
    {
        private object target;
        private string targetProperty;
        private bool expressionChanged;
        private PropertyExpression<TextureAtlasFrame> expression;

        /// <summary>
        /// Gets or sets the texture list used by this <see cref="SpriteAnimation"/>.
        /// </summary>
        public TextureAtlas Source
        {
            get { return source; }
            set { source = value; TotalFrames = source != null ? source.Count : 0; }
        }
        private TextureAtlas source;

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
        [Nine.Serialization.NotBinarySerializable]
        public object Target
        {
            get { return target; }
            set { if (target != value) { target = value; expressionChanged = true; } }
        }

        /// <summary>
        /// Gets or sets the target property that this sprite animation should affect.
        /// The property must be of type <see cref="TextureAtlasFrame"/>.
        /// </summary>
        public string TargetProperty
        {
            get { return targetProperty; }
            set { if (targetProperty != value) { targetProperty = value; expressionChanged = true; } }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation(TextureAtlas textureAtlas)
        {
            if (textureAtlas == null)
                throw new ArgumentNullException();

            Source = textureAtlas;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAnimation"/> class.
        /// </summary>
        public SpriteAnimation(TextureAtlas textureAtlas, int beginFrame, int frameCount)
        {
            if (textureAtlas == null)
                throw new ArgumentNullException();

            Source = textureAtlas;

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

            var sprite = target as Sprite;
            if (sprite != null)
            {
                sprite.Texture = Texture;
                sprite.SourceRectangle = SourceRectangle;
            }
            else if (target != null && !string.IsNullOrEmpty(targetProperty))
            {
                if (expression == null || expressionChanged)
                {
                    expression = new PropertyExpression<TextureAtlasFrame>(target, targetProperty);
                    expressionChanged = false;
                }
                expression.Value = new TextureAtlasFrame(Texture, SourceRectangle);
            }
        }
    }
}
