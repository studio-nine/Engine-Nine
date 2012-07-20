namespace Nine.Graphics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    #region TextureListItem
    /// <summary>
    /// Defines a texture and a source rectangle.
    /// </summary>
    public struct TextureListItem
    {
        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Gets or sets the source rectangle.
        /// </summary>
        public Rectangle SourceRectangle;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureListItem"/> struct.
        /// </summary>
        public TextureListItem(Texture2D texture, Rectangle? sourceRectangle)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : texture.Bounds;
        }
    }
    #endregion

    #region TextureList
    /// <summary>
    /// Defines a list of textures and source rectangles.
    /// </summary>
    public class TextureList : Collection<TextureListItem>
    {
        // Store the original sprite filenames, so we can look up sprites by name.
        internal Dictionary<string, int> spriteNames;


        public void Add(Texture2D texture, Rectangle sourceRectangle)
        {
            Add(new TextureListItem(texture, sourceRectangle));
        }
        
        /// <summary>
        /// Looks up the numeric index of the specified sprite. 
        /// </summary>
        public int GetIndex(string spriteName)
        {
            int index;

            if (!spriteNames.TryGetValue(spriteName, out index))
            {
                string error = "SpriteSheet does not contain a sprite named '{0}'.";

                throw new KeyNotFoundException(string.Format(error, spriteName));
            }

            return index;
        }


        public TextureListItem this[string name] 
        {
            get { return this[GetIndex(name)]; }
        }
    }
    #endregion

    #region TextureListReader
    /// <summary>
    /// Reader for TextureList.
    /// </summary>
    internal class TextureListReader : ContentTypeReader<TextureList>
    {
        protected override TextureList Read(ContentReader input, TextureList existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new TextureList();

            Texture2D[] textures = input.ReadObject<Texture2D[]>();
            Rectangle[] rectangles = input.ReadObject<Rectangle[]>();
            int[] indices = input.ReadObject<int[]>();
            existingInstance.spriteNames = input.ReadObject<Dictionary<string, int>>();

            for (int i = 0; i < indices.Length; i++)
            {
                existingInstance.Add(textures[indices[i]], rectangles[i]);
            }

            return existingInstance;
        }
    }
    #endregion
}
