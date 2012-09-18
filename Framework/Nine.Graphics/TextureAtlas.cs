namespace Nine.Graphics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines a texture and a source rectangle.
    /// </summary>
    public class TextureAtlasFrame
    {
        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
        }
        private Texture2D texture;

        /// <summary>
        /// Gets or sets the source rectangle.
        /// </summary>
        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
        }
        private Rectangle sourceRectangle;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureAtlasFrame"/> class.
        /// </summary>
        public TextureAtlasFrame(Texture2D texture, Rectangle? sourceRectangle)
        {
            this.texture = texture;
            this.sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : texture.Bounds;
        }
    }
    
    /// <summary>
    /// Defines a list of textures and source rectangles.
    /// </summary>
    public class TextureAtlas
    {
        /// <summary>
        /// Gets a read-only collection of frames.
        /// </summary>
        public ReadOnlyCollection<TextureAtlasFrame> Frames { get; private set; }

        /// <summary>
        /// Gets the total number of frames.
        /// </summary>
        public int Count { get { return frames.Count; } }

        internal List<TextureAtlasFrame> frames;
        internal Dictionary<string, int> spriteNames;

        internal TextureAtlas(List<TextureAtlasFrame> frames)
        {
            this.frames = frames;
            this.Frames = new ReadOnlyCollection<TextureAtlasFrame>(frames);
        }

        /// <summary>
        /// Looks up the numeric index of the specified sprite. 
        /// </summary>
        public int GetIndex(string spriteName)
        {
            int index;
            if (!spriteNames.TryGetValue(spriteName, out index))
                throw new KeyNotFoundException(spriteName);
            return index;
        }

        public TextureAtlasFrame this[int index]
        {
            get { return frames[index]; }
        }

        public TextureAtlasFrame this[string name] 
        {
            get { return frames[GetIndex(name)]; }
        }
    }

    class TextureAtlasReader : ContentTypeReader<TextureAtlas>
    {
        protected override TextureAtlas Read(ContentReader input, TextureAtlas existingInstance)
        {
            var textures = input.ReadObject<Texture2D[]>();
            var rectangles = input.ReadObject<Rectangle[]>();
            var indices = input.ReadObject<int[]>();
            var frames = new List<TextureAtlasFrame>(indices.Length);
            var result = new TextureAtlas(frames);

            for (int i = 0; i < indices.Length; ++i)
            {
                frames.Add(new TextureAtlasFrame(textures[indices[i]], rectangles[i]));
            }

            result.spriteNames = input.ReadObject<Dictionary<string, int>>();
            return result;
        }
    }
}
