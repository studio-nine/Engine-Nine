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
        private bool isDrawing;
        private SpriteBatch spriteBatch;
        private BlendState currentBlendState;
        private FastList<Entry> positiveSprites = new FastList<Entry>();
        private FastList<Entry> negativeSprites = new FastList<Entry>();
        private FastList<Entry> zeroOrderSprites = new FastList<Entry>();
        private SpriteSortComparer sortComparer = new SpriteSortComparer();

        public SpritePass()
        {
            this.order = Constants.SpritePassOrder;
        }

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            FindSprites(drawables);

            if (positiveSprites.Count > 1)
                Array.Sort(positiveSprites.Elements, 0, positiveSprites.Count, sortComparer);
            if (negativeSprites.Count > 1)
                Array.Sort(negativeSprites.Elements, 0, negativeSprites.Count, sortComparer);

            if (positiveSprites.Count + negativeSprites.Count + zeroOrderSprites.Count > 0)
            {
                if (spriteBatch == null)
                    spriteBatch = new SpriteBatch(context.graphics);
            }

            isDrawing = false;
            currentBlendState = null;

            DrawSprites(context, negativeSprites);
            DrawSprites(context, zeroOrderSprites);
            DrawSprites(context, positiveSprites);

            if (isDrawing)
                spriteBatch.End();

            positiveSprites.Clear();
            negativeSprites.Clear();
            zeroOrderSprites.Clear();
        }

        private void DrawSprites(DrawingContext context, FastList<Entry> sprites)
        {
            for (var i = 0; i < sprites.Count; ++i)
            {
                var sprite = sprites[i].Sprite;
                var material = sprites[i].Material;
                if (material != null)
                {
                    if (isDrawing)
                    {
                        isDrawing = false;
                        spriteBatch.End();
                    }
                    context.graphics.BlendState = GetBlendState(sprite);
                    currentBlendState = null;
                    sprite.Draw(context, material);
                }
                else
                {
                    var blendState = GetBlendState(sprite);
                    if (blendState != currentBlendState)
                    {
                        if (isDrawing)
                            spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, currentBlendState = blendState);
                        isDrawing = true;
                    }
                    sprite.Draw(context, spriteBatch);
                }
            }
        }

        private void FindSprites(IList<IDrawableObject> drawables)
        {
            var count = drawables.Count;
            for (var i = 0; i < count; ++i)
            {
                var drawable = drawables[i];
                var sprite = drawable as ISprite;
                if (sprite != null)
                {
                    var zOrder = sprite.ZOrder;
                    if (zOrder > 0)
                        positiveSprites.Add(new Entry { Sprite = sprite, Material = drawable.Material, ZOrder = zOrder });
                    else if (zOrder < 0)
                        negativeSprites.Add(new Entry { Sprite = sprite, Material = drawable.Material, ZOrder = zOrder });
                    else
                        zeroOrderSprites.Add(new Entry { Sprite = sprite, Material = drawable.Material });
                }
            }
        }

        private BlendState GetBlendState(ISprite sprite)
        {
            if (sprite.IsAdditive)
                return BlendState.Additive;
            if (sprite.IsTransparent)
                return BlendState.AlphaBlend;
            return BlendState.Opaque;
        }

        struct Entry
        {
            public ISprite Sprite;
            public Material Material;
            public int ZOrder;
        }

        class SpriteSortComparer : IComparer<Entry>
        {
            public int Compare(Entry x, Entry y)
            {
                return x.ZOrder - y.ZOrder;
            }
        }

        public void Dispose()
        {
            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }
            GC.SuppressFinalize(this);
        }

        ~SpritePass()
        {
            Dispose();
        }
    }
}