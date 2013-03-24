namespace Nine.Graphics.UI.Media
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public abstract class TileBrush : Brush
    {
        /* public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; } */

        public Stretch Stretch = Stretch.Fill;
        public SpriteEffects Effects = SpriteEffects.None;

        internal Rectangle Calculate(Texture2D Texture, Rectangle RenderTransform)
        {
            // TODO: Remove
            switch (Stretch)
            {
                case Media.Stretch.None:
                    {
                        var Result = RenderTransform;
                        if (Texture.Width > RenderTransform.Width)
                            ;
                        else if (Texture.Width < RenderTransform.Width)
                            Result.Width = Texture.Width;

                        if (Texture.Height > RenderTransform.Height)
                            ;
                        else if (Texture.Height < RenderTransform.Height)
                            Result.Height = Texture.Height;
                        return Result;
                    }

                case Media.Stretch.Uniform:
                    throw new NotImplementedException();
                case Media.Stretch.UniformToFill:
                    throw new NotImplementedException();

                case Media.Stretch.Fill:
                    return RenderTransform;
            }
            throw new ArgumentNullException("Stretch");
        }
    }
}
