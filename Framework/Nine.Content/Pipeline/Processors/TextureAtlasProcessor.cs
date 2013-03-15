namespace Nine.Content.Pipeline.Processors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Graphics;

    #region SequentialTextureListProcessor
    /// <summary>
    /// Processes image sequential
    /// </summary>
    [ContentProcessor(DisplayName = "Texture Atlas - Engine Nine")]
    public class TextureAtlasProcessor : ContentProcessor<Texture2DContent, TextureAtlasContent>
    {
        [DefaultValue(1)]
        public int RowCount { get; set; }

        [DefaultValue(1)]
        public int ColumnCount { get; set; }

        [DefaultValue(true)]
        [Description("Whether the images will be packed into a single large texture.")]
        public bool Pack { get; set; }

        [DefaultValue(typeof(Color), "255, 0, 255, 255")]
        public Color ColorKey { get; set; }
        
        [DefaultValue(true)]
        public bool ColorKeyEnabled { get; set; }

        [DefaultValue(false)]
        public bool GenerateMipmaps { get; set; }

        [DefaultValue(true)]
        public bool PremultiplyAlpha { get; set; }

        [DefaultValue(false)]
        public bool ResizeToPowerOfTwo { get; set; }

        [DefaultValue(TextureProcessorOutputFormat.Color)]
        public TextureProcessorOutputFormat TextureFormat { get; set; }

        public TextureAtlasProcessor()
        {
            Pack = true;
            RowCount = 1;
            ColumnCount = 1;

            ColorKey = new Color(255, 0, 255, 255);
            ColorKeyEnabled = true;
            GenerateMipmaps = false;
            PremultiplyAlpha = true;
            ResizeToPowerOfTwo = false;
            TextureFormat = TextureProcessorOutputFormat.Color;
        }

        public override TextureAtlasContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            TextureProcessor processor = new TextureProcessor();

            processor.ColorKeyColor = ColorKey;
            processor.ColorKeyEnabled = ColorKeyEnabled;
            processor.GenerateMipmaps = GenerateMipmaps;
            processor.PremultiplyAlpha = PremultiplyAlpha;
            processor.ResizeToPowerOfTwo = ResizeToPowerOfTwo;
            processor.TextureFormat = TextureFormat;


            TextureAtlasContent result = new TextureAtlasContent();        
            List<BitmapContent> sourceSprites = new List<BitmapContent>();

            string[] inputFiles = Import(input.Identity.SourceFilename);

            if (inputFiles.Length <= 0)
                throw new InvalidOperationException();

            if (inputFiles.Length == 1)
                return ProcessSingleTexture(input, context, processor);

            // Loop over each input sprite filename.
            foreach (string inputFilename in inputFiles)
            {
                // Store the name of this sprite.
                string spriteName = Path.GetFileNameWithoutExtension(inputFilename);

                result.SpriteNames.Add(spriteName, sourceSprites.Count);

                // Load the sprite texture into memory.
                ExternalReference<Texture2DContent> textureReference =
                                new ExternalReference<Texture2DContent>(inputFilename);

                Texture2DContent texture =
                    context.BuildAndLoadAsset<Texture2DContent,
                                              Texture2DContent>(textureReference, null);
                
                if (Pack)
                {
                    result.SpriteTextures.Add(0);
                    sourceSprites.Add(texture.Faces[0][0]);
                }
                else
                {
                    texture = (Texture2DContent)processor.Process(texture, context);

                    result.SpriteTextures.Add(result.Textures.Count);
                    result.Textures.Add(texture);
                    result.SpriteRectangles.Add(new Rectangle(0, 0, texture.Mipmaps[0].Width, 
                                                                    texture.Mipmaps[0].Height));
                }
            }

            // Pack all the sprites into a single large texture.
            if (Pack)
            {
                BitmapContent packedSprites = SpritePacker.PackSprites(sourceSprites,
                                                    result.SpriteRectangles, context);

                Texture2DContent content = new Texture2DContent();

                content.Mipmaps.Add(packedSprites);

                content = (Texture2DContent)processor.Process(content, context);

                result.Textures.Add(content);
            }

            return result;
        }

        private string[] Import(string filename)
        {
            int i = 0;
            int num = 0;
            int digits = 0;

            string name = Path.GetFileNameWithoutExtension(filename);

            for (i = name.Length - 1; i >= 0; i--)
            {
                if (!(name[i] <= '9' && name[i] >= '0'))
                    break;

                digits++;
            }

            if (digits <= 0)
                return new string[] { filename };

            string baseName = name.Substring(0, i + 1);
            num = int.Parse(name.Substring(i + 1));
            num++;

            string ext = Path.GetExtension(filename);
            string dir = filename.Substring(0, filename.LastIndexOf(Path.DirectorySeparatorChar));
            string file = Path.Combine(dir, baseName + string.Format("{0:d" + digits + "}", (num++)) + ext);

            List<string> result = new List<string>();

            result.Add(filename);

            while (File.Exists(file))
            {
                result.Add(file);

                file = Path.Combine(dir, baseName + string.Format("{0:d" + digits + "}", (num++)) + ext);
            }

            return result.ToArray();
        }

        private TextureAtlasContent ProcessSingleTexture(Texture2DContent input, ContentProcessorContext context, TextureProcessor processor)
        {
            Texture2DContent texture = (Texture2DContent)processor.Process(input, context);

            TextureAtlasContent result = new TextureAtlasContent();

            result.Textures.Add(texture);

            int width = input.Mipmaps[0].Width;
            int height = input.Mipmaps[0].Height;

            for (int y = 0; y < RowCount; ++y)
            {
                for (int x = 0; x < ColumnCount; ++x)
                {
                    result.SpriteTextures.Add(0);
                    result.SpriteRectangles.Add(new Rectangle(
                        x * width / ColumnCount, y * height / RowCount,
                        width / ColumnCount, height / RowCount));
                }
            }

            return result;
        }
    }
    #endregion

    #region SpritePacker
    /// <summary>
    /// Helper for arranging many small sprites into a single larger sheet.
    /// </summary>
    internal static class SpritePacker
    {
        /// <summary>
        /// Packs a list of sprites into a single big texture,
        /// recording where each one was stored.
        /// </summary>
        public static BitmapContent PackSprites(IList<BitmapContent> sourceSprites,
                                                ICollection<Rectangle> outputSprites,
                                                ContentProcessorContext context)
        {
            if (sourceSprites.Count == 0)
                throw new InvalidContentException("There are no sprites to arrange");

            // Build up a list of all the sprites needing to be arranged.
            List<ArrangedSprite> sprites = new List<ArrangedSprite>();

            for (int i = 0; i < sourceSprites.Count; ++i)
            {
                ArrangedSprite sprite = new ArrangedSprite();

                // Include a single pixel padding around each sprite, to avoid
                // filtering problems if the sprite is scaled or rotated.
                sprite.Width = sourceSprites[i].Width + 2;
                sprite.Height = sourceSprites[i].Height + 2;

                sprite.Index = i;

                sprites.Add(sprite);
            }

            // Sort so the largest sprites get arranged first.
            sprites.Sort(CompareSpriteSizes);

            // Work out how big the output bitmap should be.
            int outputWidth = GuessOutputWidth(sprites);
            int outputHeight = 0;
            int totalSpriteSize = 0;

            // Choose positions for each sprite, one at a time.
            for (int i = 0; i < sprites.Count; ++i)
            {
                PositionSprite(sprites, i, outputWidth);

                outputHeight = Math.Max(outputHeight, sprites[i].Y + sprites[i].Height);

                totalSpriteSize += sprites[i].Width * sprites[i].Height;
            }

            // Sort the sprites back into index order.
            sprites.Sort(CompareSpriteIndices);

            context.Logger.LogImportantMessage(
                "Packed {0} sprites into a {1}x{2} sheet, {3}% efficiency",
                sprites.Count, outputWidth, outputHeight,
                totalSpriteSize * 100 / outputWidth / outputHeight);

            return CopySpritesToOutput(sprites, sourceSprites, outputSprites,
                                       outputWidth, outputHeight);
        }


        /// <summary>
        /// Once the arranging is complete, copies the bitmap data for each
        /// sprite to its chosen position in the single larger output bitmap.
        /// </summary>
        static BitmapContent CopySpritesToOutput(List<ArrangedSprite> sprites,
                                                 IList<BitmapContent> sourceSprites,
                                                 ICollection<Rectangle> outputSprites,
                                                 int width, int height)
        {
            BitmapContent output = new PixelBitmapContent<Color>(width, height);

            foreach (ArrangedSprite sprite in sprites)
            {
                BitmapContent source = sourceSprites[sprite.Index];

                int x = sprite.X;
                int y = sprite.Y;

                int w = source.Width;
                int h = source.Height;

                // Copy the main sprite data to the output sheet.
                BitmapContent.Copy(source, new Rectangle(0, 0, w, h),
                                   output, new Rectangle(x + 1, y + 1, w, h));

                // Copy a border strip from each edge of the sprite, creating
                // a one pixel padding area to avoid filtering problems if the
                // sprite is scaled or rotated.
                BitmapContent.Copy(source, new Rectangle(0, 0, 1, h),
                                   output, new Rectangle(x, y + 1, 1, h));

                BitmapContent.Copy(source, new Rectangle(w - 1, 0, 1, h),
                                   output, new Rectangle(x + w + 1, y + 1, 1, h));

                BitmapContent.Copy(source, new Rectangle(0, 0, w, 1),
                                   output, new Rectangle(x + 1, y, w, 1));

                BitmapContent.Copy(source, new Rectangle(0, h - 1, w, 1),
                                   output, new Rectangle(x + 1, y + h + 1, w, 1));

                // Copy a single pixel from each corner of the sprite,
                // filling in the corners of the one pixel padding area.
                BitmapContent.Copy(source, new Rectangle(0, 0, 1, 1),
                                   output, new Rectangle(x, y, 1, 1));

                BitmapContent.Copy(source, new Rectangle(w - 1, 0, 1, 1),
                                   output, new Rectangle(x + w + 1, y, 1, 1));

                BitmapContent.Copy(source, new Rectangle(0, h - 1, 1, 1),
                                   output, new Rectangle(x, y + h + 1, 1, 1));

                BitmapContent.Copy(source, new Rectangle(w - 1, h - 1, 1, 1),
                                   output, new Rectangle(x + w + 1, y + h + 1, 1, 1));

                // Remember where we placed this sprite.
                outputSprites.Add(new Rectangle(x + 1, y + 1, w, h));
            }

            return output;
        }


        /// <summary>
        /// Internal helper class keeps track of a sprite while it is being arranged.
        /// </summary>
        class ArrangedSprite
        {
            public int Index;

            public int X;
            public int Y;

            public int Width;
            public int Height;
        }


        /// <summary>
        /// Works out where to position a single sprite.
        /// </summary>
        static void PositionSprite(List<ArrangedSprite> sprites,
                                   int index, int outputWidth)
        {
            int x = 0;
            int y = 0;

            while (true)
            {
                // Is this position free for us to use?
                int intersects = FindIntersectingSprite(sprites, index, x, y);

                if (intersects < 0)
                {
                    sprites[index].X = x;
                    sprites[index].Y = y;

                    return;
                }

                // Skip past the existing sprite that we collided with.
                x = sprites[intersects].X + sprites[intersects].Width;

                // If we ran out of room to move to the right,
                // try the next line down instead.
                if (x + sprites[index].Width > outputWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }


        /// <summary>
        /// Checks if a proposed sprite position collides with anything
        /// that we already arranged.
        /// </summary>
        static int FindIntersectingSprite(List<ArrangedSprite> sprites,
                                          int index, int x, int y)
        {
            int w = sprites[index].Width;
            int h = sprites[index].Height;

            for (int i = 0; i < index; ++i)
            {
                if (sprites[i].X >= x + w)
                    continue;

                if (sprites[i].X + sprites[i].Width <= x)
                    continue;

                if (sprites[i].Y >= y + h)
                    continue;

                if (sprites[i].Y + sprites[i].Height <= y)
                    continue;

                return i;
            }

            return -1;
        }


        /// <summary>
        /// Comparison function for sorting sprites by size.
        /// </summary>
        static int CompareSpriteSizes(ArrangedSprite a, ArrangedSprite b)
        {
            int aSize = a.Height * 1024 + a.Width;
            int bSize = b.Height * 1024 + b.Width;

            return bSize.CompareTo(aSize);
        }


        /// <summary>
        /// Comparison function for sorting sprites by their original indices.
        /// </summary>
        static int CompareSpriteIndices(ArrangedSprite a, ArrangedSprite b)
        {
            return a.Index.CompareTo(b.Index);
        }


        /// <summary>
        /// Heuristic guesses what might be a good output width for a list of sprites.
        /// </summary>
        static int GuessOutputWidth(List<ArrangedSprite> sprites)
        {
            // Gather the widths of all our sprites into a temporary list.
            List<int> widths = new List<int>();

            foreach (ArrangedSprite sprite in sprites)
            {
                widths.Add(sprite.Width);
            }

            // Sort the widths into ascending order.
            widths.Sort();

            // Extract the maximum and median widths.
            int maxWidth = widths[widths.Count - 1];
            int medianWidth = widths[widths.Count / 2];

            // Heuristic assumes an NxN grid of median sized sprites.
            int width = medianWidth * (int)Math.Round(Math.Sqrt(sprites.Count));

            // Make sure we never choose anything smaller than our largest sprite.
            return Math.Max(width, maxWidth);
        }
    }
    #endregion
}
