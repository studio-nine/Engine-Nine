#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel.Composition;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Visualizers
{
    [Default]
    [Export(typeof(IVisualizer))]
    public class Texture2DVisualizer : GraphicsVisualizer<Texture2DContent, Texture2D>
    {
        SpriteBatch spriteBatch;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Draw(TimeSpan elapsedTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.Draw(Drawable, GraphicsDevice.Viewport.Bounds.Center.ToVector2() -
                                       Drawable.Bounds.Center.ToVector2(), Color.White);
            spriteBatch.End();
        }
    }
}
