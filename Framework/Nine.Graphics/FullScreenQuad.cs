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
        /// Gets or sets the material of the object.
        /// </summary>
        public Material Material
        {
            // The default material will enable lighting, so we need to
            // explicitly create a material without lighting.
            get { return material ?? (material = new BasicMaterial(GraphicsDevice) { LightingEnabled = false }); }
            set { material = value; }
        }
        private Material material;

        /// <summary>
        /// Gets or sets the texture to display in this quad.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// The vertex buffer and index buffers are shared between FullScreenQuads.
        /// </summary>
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

#if !WINDOWS_PHONE
        /// <summary>
        /// Always use this pass through material as the vertex shader when drawing full screen quads.
        /// </summary>
        private VertexPassThrough2Material vertexPassThrough2;
        private VertexPassThrough3Material vertexPassThrough3;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="FullScreenQuad"/> class.
        /// </summary>
        public FullScreenQuad(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Visible = true;
            GraphicsDevice = graphics;

#if !WINDOWS_PHONE
            vertexPassThrough2 = new VertexPassThrough2Material(graphics);
#endif
            GetBuffers(graphics, out vertexBuffer, out indexBuffer);
        }

        /// <summary>
        /// Gets the vertex and index buffer for drawing sprites.
        /// </summary>
        private static void GetBuffers(GraphicsDevice graphics, out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer)
        {
            KeyValuePair<VertexBuffer, IndexBuffer> sharedBuffer;

            if (SharedBuffers == null)
                SharedBuffers = new Dictionary<GraphicsDevice, KeyValuePair<VertexBuffer, IndexBuffer>>();

            if (!SharedBuffers.TryGetValue(graphics, out sharedBuffer))
            {
                sharedBuffer = new KeyValuePair<VertexBuffer, IndexBuffer>(
                    new VertexBuffer(graphics, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly)
                  , new IndexBuffer(graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly));

                sharedBuffer.Key.SetData(new[] 
                {
                    new VertexPositionTexture() { Position = new Vector3(-1, 1, 0), TextureCoordinate = new Vector2(0, 0) },
                    new VertexPositionTexture() { Position = new Vector3(1, 1, 0), TextureCoordinate = new Vector2(1, 0) },
                    new VertexPositionTexture() { Position = new Vector3(1, -1, 0), TextureCoordinate = new Vector2(1, 1) },
                    new VertexPositionTexture() { Position = new Vector3(-1, -1, 0), TextureCoordinate = new Vector2(0, 1) },
                });

                sharedBuffer.Value.SetData<ushort>(new ushort[] { 0, 1, 2, 0, 2, 3 });
                SharedBuffers.Add(graphics, sharedBuffer);
            }

            vertexBuffer = sharedBuffer.Key;
            indexBuffer = sharedBuffer.Value;
        }
        private static Dictionary<GraphicsDevice, KeyValuePair<VertexBuffer, IndexBuffer>> SharedBuffers;

        public void Draw(DrawingContext context, Material material)
        {
#if WINDOWS_PHONE
            var view = context.matrices.view;
            var projection = context.matrices.projection;

            context.matrices.view = context.matrices.projection = material.world = Matrix.Identity;
#endif
            if (Texture != null)
                material.texture = Texture;
            material.BeginApply(context);

            context.SetVertexBuffer(vertexBuffer, 0);
            GraphicsDevice.Indices = indexBuffer;

#if SILVERLIGHT
            vertexPassThrough2.BeginApply(context);
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

#elif WINDOWS_PHONE
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

            context.matrices.view = view;
            context.matrices.projection = projection;
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
        float IDrawableObject.GetDistanceToCamera(Vector3 cameraPosition) { return 0; }
        bool IDrawableObject.CastShadow { get { return false; } }
    }
}
