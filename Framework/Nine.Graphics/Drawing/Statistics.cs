namespace Nine.Graphics.Drawing
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines commonly used statistics of the renderer.
    /// </summary>
    public class Statistics
    {
        public int VisibleLightCount { get; internal set; }
        public int VisibleObjectCount { get; internal set; }
        public int VisibleDrawableCount { get; internal set; }

        public int VertexCount { get; internal set; }
        public int PrimitiveCount { get; internal set; }

        internal Statistics()
        {
            Reset();
        }

        internal void Reset()
        {
            VisibleLightCount = 0;
            VisibleDrawableCount = 0;
            VisibleObjectCount = 0;
            VertexCount = 0;
            PrimitiveCount = 0;
        }
    }
}