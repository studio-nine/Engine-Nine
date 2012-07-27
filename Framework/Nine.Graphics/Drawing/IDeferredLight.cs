namespace Nine.Graphics.Drawing
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// Defines a light used by deferred rendering.
    /// </summary>
    public interface IDeferredLight
    {
        /// <summary>
        /// Gets the drawable object that is used to generate the light buffer.
        /// </summary>
        IDrawableObject Drawable { get; }
    }
}
