namespace Nine.Graphics
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.AttachedProperty;

    [ContentProperty("Layers")]
    public class Splatter
    {
        public IList<ExternalReference<Texture2DContent>> Layers
        {
            get { return layers; }
        }
        private List<ExternalReference<Texture2DContent>> layers = new List<ExternalReference<Texture2DContent>>();
    }
}
