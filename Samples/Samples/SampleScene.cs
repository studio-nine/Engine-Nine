namespace Samples
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Serialization;

    public class SampleScene : Sample
    {
        private readonly string assetName;
     
        public SampleScene(string assetName) 
        {
            this.assetName = assetName; 
        }

        public override string Title
        {
            get { return assetName; }
        }

        public override Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            return content.Load<Scene>(assetName);
        }
    }
}
