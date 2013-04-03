namespace Nine.Graphics.UI
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Media;

    internal static class UIExtensions
    {
        public static Vector2[] TextureCoords(Flip flip)
        {
            switch (flip)
            {
                case Flip.None:
                    return new Vector2[] { 
                        new Vector2(1, 0),
                        new Vector2(1, 1),
                        new Vector2(0, 1),
                        new Vector2(0, 0)
                    };
                case Flip.Horizontally:
                    return new Vector2[] { 
                        new Vector2(0, 0),
                        new Vector2(0, 1),
                        new Vector2(1, 1),
                        new Vector2(1, 0)
                    };
                case Flip.Vertically:
                    return new Vector2[] { 
                        new Vector2(1, 1),
                        new Vector2(1, 0),
                        new Vector2(0, 0),
                        new Vector2(0, 1)
                    };
                case Flip.Both:
                    return new Vector2[] { 
                        new Vector2(0, 1),
                        new Vector2(0, 0),
                        new Vector2(1, 0),
                        new Vector2(1, 1)
                    };
            }
            throw new System.ArgumentNullException("flip");
        }
    }
}
