namespace Nine.Samples
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Serialization;

    public class Tutorial : Sample
    {
        private readonly string assetName;
     
        public Tutorial(string assetName) 
        {
            this.assetName = assetName;
        }

        public override string Title
        {
            get { return assetName.Replace(".xaml", ""); }
        }

        public override Scene CreateScene(GraphicsDevice graphics, Microsoft.Xna.Framework.Content.ContentManager content)
        {
            return content.Load<Scene>(assetName);
        }
    }
}
