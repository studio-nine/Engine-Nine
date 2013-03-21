namespace Nine.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Defines a 2D text sprite.
    /// </summary>
    public class TextSprite : Transformable, ISprite
    {
        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets whether this sprite is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the text for this text sprite.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the alpha of this sprite.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }
        private float alpha = 1;

        /// <summary>
        /// Gets or sets the color of this sprite.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        private Color color = Color.White;

        /// <summary>
        /// Gets or sets the blend state of this sprite. The default value is BlendState.AlphaBlend.
        /// </summary>
#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets the z order of this sprite.
        /// </summary>
        public int ZOrder
        {
            get { return zOrder; }
            set { zOrder = value; }
        }
        private int zOrder;

        /// <summary>
        /// Gets or sets the texture of this sprite.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }
        private SpriteFont font;

        /// <summary>
        /// Gets or sets a value indicating whether this sprite
        /// will be flipped on the x axis.
        /// </summary>
        public bool FlipX
        {
            get { return flipX; }
            set { flipX = value; }
        }
        private bool flipX;

        /// <summary>
        /// Gets or sets a value indicating whether this sprite
        /// will be flipped on the x axis.
        /// </summary>
        public bool FlipY
        {
            get { return flipY; }
            set { flipY = value; }
        }
        private bool flipY;

        /// <summary>
        /// Gets or sets the position of this sprite in screen space.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                transform.M41 = value.X;
                transform.M42 = value.Y;
                transformChanging = true;
                NotifyTransformChanged();
                transformChanging = false;
            }
        }
        private Vector2 position;

        /// <summary>
        /// Gets or sets the scale of this sprite in screen space.
        /// </summary>
        public Vector2 Scale
        {
            get { return scale; }
            set { scale = value; UpdateScaleAndRotation(); }
        }
        private Vector2 scale = Vector2.One;

        /// <summary>
        /// Gets or sets the rotation of this sprite in screen space.
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; UpdateScaleAndRotation(); }
        }
        private float rotation;

        /// <summary>
        /// Gets or sets the optional target size of this sprite in screen space independent of the scale factor.
        /// </summary>
        public Vector2? Size
        {
            get { return size; }
            set { size = value; UpdateScaleAndRotation(); anchorPointNeedsUpdate = true; }
        }
        private Vector2? size;

        /// <summary>
        /// Gets or sets the percentage based anchor point of this sprite.
        /// </summary>
        public Vector2 Anchor
        {
            get { return anchor; }
            set { anchor = value; anchorPointNeedsUpdate = true; }
        }
        private Vector2 anchor = new Vector2(0.5f, 0.5f);
        private Vector2 anchorPoint;
        private bool anchorPointNeedsUpdate = true;
        private bool transformChanging;

        /// <summary>
        /// Initializes a new instance of TextSprite.
        /// </summary>
        public TextSprite(GraphicsDevice graphics)
        {   
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Visible = true;
            Scale = Vector2.One;
            GraphicsDevice = graphics;
        }

        /// <summary>
        /// Draws this sprite using the specified sprite batch.
        /// </summary>
        public void Draw(DrawingContext context, SpriteBatch spriteBatch)
        {
            if (font != null && !string.IsNullOrEmpty(Text))
            {
                var spriteEffects = SpriteEffects.None;
                if (flipX)
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                if (!flipY)
                    spriteEffects |= SpriteEffects.FlipVertically;

                float worldRotation;
                Vector2 worldScale;
                Vector2 worldPosition;
                Matrix worldTransform = AbsoluteTransform;

                ApplyScaleFactor(ref worldTransform);
                MatrixHelper.Decompose(ref worldTransform, out worldScale, out worldRotation, out worldPosition);

                UpdateAnchorPoint();

                spriteBatch.DrawString(font, Text, worldPosition, color * alpha, worldRotation, anchorPoint, worldScale, spriteEffects, 0);
            }
        }

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            if (!transformChanging)
                MatrixHelper.Decompose(ref transform, out scale, out rotation, out position);
        }

        private void UpdateScaleAndRotation()
        {
            if (rotation == 0)
            {
                transform.M11 = scale.X; transform.M12 = 0;
                transform.M21 = 0; transform.M22 = scale.Y;
            }
            else
            {
                var cos = (float)Math.Cos(rotation);
                var sin = (float)Math.Sin(rotation);

                transform.M11 = cos * scale.X; transform.M12 = sin * scale.X;
                transform.M21 = -sin * scale.Y; transform.M22 = cos * scale.Y;
            }

            transformChanging = true;
            NotifyTransformChanged();
            transformChanging = false;
        }

        private void ApplyScaleFactor(ref Matrix worldTransform)
        {
            if (size != null)
            {
                var scale = size.Value;

                worldTransform.M11 *= scale.X; worldTransform.M12 *= scale.X; worldTransform.M13 *= scale.X;
                worldTransform.M21 *= scale.Y; worldTransform.M22 *= scale.Y; worldTransform.M23 *= scale.Y;
            }
        }

        private void UpdateAnchorPoint()
        {
            if (anchorPointNeedsUpdate && font != null && !string.IsNullOrEmpty(Text))
            {
                var textSize = font.MeasureString(Text);
                anchorPoint.X = textSize.X * anchor.X;
                anchorPoint.Y = textSize.Y * anchor.Y;
                anchorPointNeedsUpdate = false;
            }
        }

        SamplerState ISprite.SamplerState { get { return null; } }
        Material ISprite.Material { get { return null; } }
        void ISprite.Draw(DrawingContext context, Material material) { }
    }
}
