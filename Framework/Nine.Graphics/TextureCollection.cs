namespace Nine.Graphics
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Contains commonly used textures in a drawing context.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TextureCollection
    {
        /// <summary>
        /// Gets a value indicating the maximum number of textures supported.
        /// </summary>
        public const int MaxTextureSlots = 64;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureCollection"/> class.
        /// </summary>
        internal TextureCollection() { }

        /// <summary>
        /// Gets or sets the global texture with the specified texture usage.
        /// </summary>
        public Texture this[TextureUsage textureUsage]
        {
            get { return textureSlots[(int)textureUsage]; }
            set { textureSlots[(int)textureUsage] = value; }
        }
        private Texture[] textureSlots = new Texture[MaxTextureSlots];
    }
}