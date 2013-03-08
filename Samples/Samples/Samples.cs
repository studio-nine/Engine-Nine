namespace Samples
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Content;
    using Nine.Graphics.Materials;
    using System.Threading.Tasks;

    public class __1_HelloWorld : ISample
    {
        public Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            return content.LoadAsync<Scene>("Scenes/01. Hello World").Result;
        }
    }

    public class __2_Models : ISample
    {
        public Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            return content.Load<Scene>("Scenes/02. Models");
        }
    }

    public class _14_Sprites : ISample
    {
        public Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            return content.Load<Scene>("Scenes/14. Sprites");
        }
    }
}
