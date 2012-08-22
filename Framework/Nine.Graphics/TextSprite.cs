namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Defines a 2D text sprite.
    /// </summary>
    public class TextSprite : Transformable, IDrawableObject, ISceneObject, ISprite
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
        /// Gets or sets whether this sprite is additive.
        /// </summary>
        public bool IsAdditive { get; set; }

        /// <summary>
        /// Gets or sets the position of this sprite.
        /// </summary>
        public Vector2 Position 
        {
            get { return position; }
            set { position = value; }
        }
        private Vector2 position;
        
        /// <summary>
        /// Gets or sets the scale of this sprite.
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Gets or sets the rotation of this sprite.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the percentage based anchor point of this sprite.
        /// </summary>
        public Vector2 Anchor 
        {
            get { return anchor; }
            set { anchor = value; }
        }
        private Vector2 anchor = new Vector2(0.5f, 0.5f);

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
            if (spriteBatch != null && font != null)
            {
                Vector2 screenPosition;
                Vector2 anchorPoint;
                GetScreenPositionAndAnchorPoint(context, out screenPosition, out anchorPoint);

                spriteBatch.DrawString(font, Text, screenPosition, color * alpha,
                                       Rotation, anchorPoint, Scale, SpriteEffects.None, zOrder);
            }
        }

        private void GetScreenPositionAndAnchorPoint(
            DrawingContext context, out Vector2 screenPosition, out Vector2 anchorPoint)
        {
            var projectedPosition = new Vector3();
            projectedPosition.X = position.X;
            projectedPosition.Y = position.Y;

            projectedPosition = context.graphics.Viewport.Project(
                projectedPosition, context.matrices.projection, context.matrices.view, AbsoluteTransform);
            
            screenPosition.X = projectedPosition.X;
            screenPosition.Y = projectedPosition.Y;

            var size = font.MeasureString(Text);
            anchorPoint = new Vector2();
            anchorPoint.X = size.X * anchor.X;
            anchorPoint.Y = size.Y * anchor.Y;
        }

        void ISceneObject.OnAdded(DrawingContext context)
        {
            var passes = context.mainPass.Passes;
            for (var i = passes.Count - 1; i >= 0; --i)
            {
                if (passes[i] is SpritePass)
                    return;
            }
            passes.Add(new SpritePass());
        }

        Material IDrawableObject.Material { get { return null; } }
        bool ISprite.IsTransparent { get { return true; } }

        void IDrawableObject.OnAddedToView(DrawingContext context) { }
        void ISprite.Draw(DrawingContext context, Material material) { }
        void IDrawableObject.Draw(DrawingContext context, Material material) { }
        void ISceneObject.OnRemoved(DrawingContext context) { }
        float IDrawableObject.GetDistanceToCamera(Vector3 cameraPosition) { return 0; }
    }
}
