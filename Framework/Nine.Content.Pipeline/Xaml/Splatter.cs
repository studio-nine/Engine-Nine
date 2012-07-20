namespace Nine.Content.Pipeline.Xaml
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.Content.Pipeline.Processors;

    [ContentProperty("Layers")]
    [DefaultContentProcessor(typeof(SplatterTextureProcessor))]
    public class Splatter
    {
        public IList<ExternalReference<Texture2DContent>> Layers
        {
            get { return layers; }
        }
        private List<ExternalReference<Texture2DContent>> layers = new List<ExternalReference<Texture2DContent>>();
    }
}
