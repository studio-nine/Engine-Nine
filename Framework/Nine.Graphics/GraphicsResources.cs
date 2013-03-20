namespace Nine.Graphics
{
    using Microsoft.Xna.Framework.Graphics;

    class GraphicsResources<T> : Singleton<GraphicsDevice, T> where T : class { }
}
