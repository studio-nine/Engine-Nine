namespace Nine.Graphics
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

    /// <summary>
    /// Build-time type used to hold the output data from the SpriteSheetProcessor.
    /// This is saved into XNB format by the SpriteSheetWriter helper class, then
    /// at runtime, the SpriteSheetReader loads the data into a SpriteSheet object.
    /// </summary>
    public class TextureAtlasContent
    {
        /// <summary> Single texture contains many separate sprite images. </summary>
        public List<Texture2DContent> Textures { get; set; }

        /// <summary> Remember where in the texture each sprite has been placed. </summary>
        public List<Rectangle> SpriteRectangles { get; set; }

        /// <summary> Index to each of the texture. </summary>
        public List<int> SpriteTextures { get; set; }

        /// <summary> Store the original sprite filenames, so we can look up sprites by name. </summary>
        public Dictionary<string, int> SpriteNames { get; set; }

        public TextureAtlasContent()
        {
            Textures = new List<Texture2DContent>();
            SpriteRectangles = new List<Rectangle>();
            SpriteTextures = new List<int>();
            SpriteNames = new Dictionary<string, int>();
        }
    }
}
