namespace Nine.Graphics.Materials.Deferred
{
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines a light used by deferred rendering.
    /// </summary>
    public interface IDeferredLight
    {
        /// <summary>
        /// Gets the effect used to draw the light geometry.
        /// </summary>
        Effect Effect { get; }

        /// <summary>
        /// Gets the vertex buffer of the light geometry.
        /// </summary>
        VertexBuffer VertexBuffer { get; }

        /// <summary>
        /// Gets the index buffer of the light geometry.
        /// </summary>
        IndexBuffer IndexBuffer { get; }
    }
}
