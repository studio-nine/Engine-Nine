namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents a full screen quad.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("Material")]
    public class FullScreenQuad : Nine.Object, IDrawableObject
    {
        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets whether this object is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets the material of the object.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// The vertex buffer and index buffers are shared between FullScreenQuads.
        /// </summary>
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        /// <summary>
        /// Always use this pass through material as the vertex shader when drawing full screen quads.
        /// </summary>
        private VertexPassThrough2Material vertexPassThrough2;
        private VertexPassThrough3Material vertexPassThrough3;

        /// <summary>
        /// Initializes a new instance of the <see cref="FullScreenQuad"/> class.
        /// </summary>
        public FullScreenQuad(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Visible = true;
            GraphicsDevice = graphics;
            vertexPassThrough2 = new VertexPassThrough2Material(graphics);

            Sprite.GetSpriteBuffers(graphics, out vertexBuffer, out indexBuffer);
        }

        public void Draw(DrawingContext context, Material material)
        {
            material.BeginApply(context);

            context.SetVertexBuffer(vertexBuffer, 0);
            GraphicsDevice.Indices = indexBuffer;

#if SILVERLIGHT
            vertexPassThrough2.BeginApply(context);
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
#else
            // Apply a vertex pass through material in case the specified material does
            // not have a vertex shader.
            //
            // NOTE: There is no legal way to determine the current shader profile. If you are mix using a 
            //       2_0 vs and 3_0 ps, DrawIndexedPrimitives is going to throw an InvalidOperationException,
            //       in that case, catch that exception and try to draw with 3_0 vs.
            try
            {
                // Use vs 2_0
                vertexPassThrough2.BeginApply(context);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
            catch (InvalidOperationException)
            {
                if (context.graphics.GraphicsProfile != GraphicsProfile.HiDef)
                    throw;

                // Use vs 3_0
                if (vertexPassThrough3 == null)
                    vertexPassThrough3 = new VertexPassThrough3Material(GraphicsDevice);
                vertexPassThrough3.BeginApply(context);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
#endif

            material.EndApply(context);
        }

        void IDrawableObject.OnAddedToView(DrawingContext context) { }
    }
}
