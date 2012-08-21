namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

    /// <summary>
    /// Defines a pass that draws all the 2D sprites.
    /// </summary>
    sealed class SpritePass : Pass, IDisposable
    {
        private bool drawing;
        private GraphicsDevice graphics;

        internal SpriteBatch SpriteBatch
        {
            get 
            {
                if (spriteBatch == null)
                {
                    spriteBatch = new SpriteBatch(graphics);
                    if (drawing)
                        spriteBatch.Begin(0, BlendState.Opaque);
                }
                return spriteBatch; 
            }
        }
        private SpriteBatch spriteBatch;

        public SpritePass(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            this.order = Constants.SpritePassOrder;
        }

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            drawing = true;

            if (spriteBatch != null)
                spriteBatch.Begin(0, BlendState.Opaque);
            if (transparentBatch != null)
                transparentBatch.Begin(0, BlendState.AlphaBlend);
            if (additiveBatch != null)
                additiveBatch.Begin(0, BlendState.Additive);

            var count = drawables.Count;
            for (var i = 0; i < count; ++i)
            {
                var drawable = drawables[i];
                var sprite = drawable as Sprite;
                if (sprite != null)
                {
                    if (sprite.Material == null)
                        sprite.Draw(context, this);
                }
                else
                {
                    var textSprite = drawable as TextSprite;
                    if (textSprite != null)
                        textSprite.Draw(context, this);
                }
            }

            if (spriteBatch != null)    
                spriteBatch.End();
            if (transparentBatch != null)
                transparentBatch.End();
            if (additiveBatch != null)
                additiveBatch.End();
            
            drawing = false;
        }

        public void Dispose()
        {
            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }

            if (transparentBatch != null)
            {
                transparentBatch.Dispose();
                transparentBatch = null;
            }

            if (additiveBatch != null)
            {
                additiveBatch.Dispose();
                additiveBatch = null;
            }
            GC.SuppressFinalize(this);
        }

        ~SpritePass()
        {
            Dispose();
        }
    }
}