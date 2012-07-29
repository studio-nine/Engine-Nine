namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// Defines a pass that draws the scene depth buffer prior to the actual rendering.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DepthPrePass : Pass
    {
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {

        }
    }
}