namespace Samples
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI;
    using Nine.Serialization;

    public class UITest : Sample
    {
        public override Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();
            //var window = new Window(graphics.Viewport.Bounds, );
            return scene;
        }
    }
}
