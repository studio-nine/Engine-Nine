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
        private BasicEffect basicEffect;
        private SpriteBatch spriteBatch;
        private BlendState currentBlendState;
        private SamplerState currentSamplerState;
        private ISpatialQuery<ISprite> spriteQuery;
        private Action<ISprite> findSpriteAction;
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
            FindSprites(context);

            var totalSprites = positiveSprites.Count + negativeSprites.Count + zeroOrderSprites.Count;
            if (totalSprites <= 0)
                return;

            if (spriteBatch == null)
            {
                spriteBatch = new SpriteBatch(context.graphics);
                basicEffect = new BasicEffect(context.graphics) { VertexColorEnabled = true, TextureEnabled = true, LightingEnabled = false };
            }

            if (positiveSprites.Count > 1)
                Array.Sort(positiveSprites.Elements, 0, positiveSprites.Count, sortComparer);
            if (negativeSprites.Count > 1)
                Array.Sort(negativeSprites.Elements, 0, negativeSprites.Count, sortComparer);
            
            isDrawing = false;
            currentBlendState = null;
            context.PreviousMaterial = null;
            context.graphics.DepthStencilState = DepthStencilState.None;
            context.graphics.RasterizerState = RasterizerState.CullNone;

            // Compensate for half pixel offset
            var viewport = context.graphics.Viewport;
            var invX = 1f / viewport.Width;
            var invY = -1f / viewport.Height;

            context.matrices.projection.M41 -= invX;
            context.matrices.projection.M42 -= invY;

            basicEffect.View = context.matrices.view;
            basicEffect.Projection = context.matrices.projection;

            DrawSprites(context, negativeSprites);
            DrawSprites(context, zeroOrderSprites);
            DrawSprites(context, positiveSprites);

            if (isDrawing)
                spriteBatch.End();

            context.matrices.projection.M41 += invX;
            context.matrices.projection.M42 += invY;

            positiveSprites.Clear();
            negativeSprites.Clear();
            zeroOrderSprites.Clear();

            // Restore rasterizer state to default since we use CullNone for sprites.
            context.graphics.RasterizerState = RasterizerState.CullCounterClockwise;
            context.graphics.DepthStencilState = DepthStencilState.Default;
        }

        private void DrawSprites(DrawingContext context, FastList<Entry> sprites)
        {
            for (var i = 0; i < sprites.Count; ++i)
            {
                var sprite = sprites[i].Sprite;
                var material = sprite.Material;
                if (material != null)
                {
                    if (isDrawing)
                    {
                        isDrawing = false;
                        spriteBatch.End();
                    }
                    currentBlendState = null;
                    currentSamplerState = null;
                    context.graphics.BlendState = sprite.BlendState ?? BlendState.AlphaBlend;
                    context.graphics.SamplerStates[0] = sprite.SamplerState ?? SamplerState.LinearClamp;
                    sprite.Draw(context, material);
                }
                else
                {
                    var blendState = sprite.BlendState ?? BlendState.AlphaBlend;
                    var samplerState = sprite.SamplerState ?? SamplerState.LinearClamp;
                    if (blendState != currentBlendState || samplerState != currentSamplerState)
                    {
                        if (isDrawing)
                            spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, currentBlendState = blendState, currentSamplerState = samplerState, null, RasterizerState.CullNone, basicEffect);
                        isDrawing = true;
                    }
                    sprite.Draw(context, spriteBatch);
                }
            }
        }

        private void FindSprites(DrawingContext context)
        {
            if (spriteQuery == null)
            {
                spriteQuery = context.CreateSpatialQuery<ISprite>(sprite => sprite.Visible);
                findSpriteAction = new Action<ISprite>(AddSprite);
            }
            spriteQuery.FindAll(context.ViewFrustum, findSpriteAction);
        }

        private void AddSprite(ISprite sprite)
        {
            var zOrder = sprite.ZOrder;
            if (zOrder > 0)
                positiveSprites.Add(new Entry { Sprite = sprite, ZOrder = zOrder });
            else if (zOrder < 0)
                negativeSprites.Add(new Entry { Sprite = sprite, ZOrder = zOrder });
            else
                zeroOrderSprites.Add(new Entry { Sprite = sprite });
        }

        struct Entry
        {
            public ISprite Sprite;
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