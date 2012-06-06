#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Nine.Studio.Extensibility;
using Nine.Studio.Controls;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace Nine.Studio.Visualizers
{
    [Export(typeof(IDocumentVisualizer))]
    public class Texture2DVisualizer : GraphicsDocumentVisualizer<Texture2DContent, Texture2D>
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
