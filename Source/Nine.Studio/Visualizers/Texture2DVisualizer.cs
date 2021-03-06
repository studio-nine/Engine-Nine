﻿namespace Nine.Studio.Visualizers
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Studio.Extensibility;

    [Export(typeof(IVisualizer))]
    [ExportMetadata(IsDefault = true)]
    public class Texture2DVisualizer : GraphicsVisualizer<Texture2D>
    {
        SpriteBatch spriteBatch;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Draw(float elapsedTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.Draw(Drawable, GraphicsDevice.Viewport.Bounds.Center.ToVector2() -
                                       Drawable.Bounds.Center.ToVector2(), Color.White);
            spriteBatch.End();
        }
    }
}
