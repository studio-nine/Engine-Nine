namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Defines a 2D textured sprite.
    /// </summary>
    public class Sprite : Transformable, IDrawableObject, ISceneObject, ISprite
    {
        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets whether this sprite is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the material of this sprite.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set { material = value; }
        }
        private Material material;

        /// <summary>
        /// Gets or sets the alpha of this sprite.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }
        private float alpha = 1;

        /// <summary>
        /// Gets or sets the color of this sprite.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        private Color color = Color.White;

        /// <summary>
        /// Gets or sets whether this sprite is additive.
        /// </summary>
        public bool IsAdditive { get; set; }

        /// <summary>
        /// Gets or sets whether this sprite is transparent.
        /// </summary>
        public bool IsTransparent { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this sprite
        /// will be flipped on the x axis.
        /// </summary>
        public bool FlipX
        {
            get { return flipX; }
            set { flipX = value; }
        }
        private bool flipX;

        /// <summary>
        /// Gets or sets a value indicating whether this sprite
        /// will be flipped on the x axis.
        /// </summary>
        public bool FlipY
        {
            get { return flipY; }
            set { flipY = value; }
        }
        private bool flipY;

        /// <summary>
        /// Gets or sets the position of this sprite.
        /// </summary>
        public Vector2 Position 
        {
            get { return position; }
            set { position = value; }
        }
        private Vector2 position;
        
        /// <summary>
        /// Gets or sets the scale of this sprite.
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Gets or sets the rotation of this sprite.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the percentage based anchor point of this sprite.
        /// </summary>
        public Vector2 Anchor 
        {
            get { return anchor; }
            set { anchor = value; }
        }
        private Vector2 anchor = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// Gets or sets the z order of this sprite.
        /// </summary>
        public int ZOrder
        {
            get { return zOrder; }
            set { zOrder = value; }
        }
        private int zOrder;

        /// <summary>
        /// Gets or sets the texture of this sprite.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        private Texture2D texture;
        
        /// <summary>
        /// Gets or sets the rectangular region in the source texture.
        /// </summary>
        public Rectangle? SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }
        private Rectangle? sourceRectangle;

        /// <summary>
        /// The vertex buffer and index buffers are shared between sprites.
        /// </summary>
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private SpriteMaterial spriteMaterial;

        /// <summary>
        /// Initializes a new instance of Sprite.
        /// </summary>
        public Sprite(GraphicsDevice graphics)
        {   
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Visible = true;
            IsTransparent = true;
            Scale = Vector2.One;
            GraphicsDevice = graphics;
            spriteMaterial = new SpriteMaterial(graphics);
        }

        /// <summary>
        /// Gets the vertex and index buffer for drawing sprites.
        /// </summary>
        internal static void GetSpriteBuffers(GraphicsDevice graphics, out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer)
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
        
        /// <summary>
        /// Draws this sprite with the specified material.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
            if (vertexBuffer == null)
                GetSpriteBuffers(GraphicsDevice, out vertexBuffer, out indexBuffer);

            Vector2 screenPosition;
            Vector2 anchorPoint;
            GetScreenPositionAndAnchorPoint(context, out screenPosition, out anchorPoint);

            material.texture = texture;
            material.BeginApply(context);
            
            spriteMaterial.effect.ScreenPositionAndAnchorPoint.SetValue(
                new Vector4(screenPosition.X, screenPosition.Y, anchor.X, anchor.Y));

            if (Rotation == 0)
                spriteMaterial.effect.ScaleAndRotation.SetValue(new Vector4(Scale.X, Scale.Y, 1, 0));
            else
                spriteMaterial.effect.ScaleAndRotation.SetValue(
                    new Vector4(Scale.X, Scale.Y, (float)Math.Cos(Rotation), (float)Math.Sin(Rotation)));

            spriteMaterial.effect.TextureTransform.SetValue(
                TextureTransform.ToArray(
                TextureTransform.CreateFromSourceRectange(texture, sourceRectangle)));

            spriteMaterial.BeginApply(context);

            context.SetVertexBuffer(vertexBuffer, 0);
            GraphicsDevice.Indices = indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

            material.EndApply(context);
        }

        /// <summary>
        /// Draws this sprite using the specified sprite batch.
        /// </summary>
        public void Draw(DrawingContext context, SpriteBatch spriteBatch)
        {
            if (spriteBatch != null && texture != null)
            {
                var spriteEffects = SpriteEffects.None;
                if (flipX)
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                if (flipX)
                    spriteEffects |= SpriteEffects.FlipVertically;

                Vector2 screenPosition;
                Vector2 anchorPoint;
                GetScreenPositionAndAnchorPoint(context, out screenPosition, out anchorPoint);

                spriteBatch.Draw(texture, screenPosition, sourceRectangle, color * alpha,
                                 Rotation, anchorPoint, Scale, spriteEffects, zOrder);
            }
        }

        private void GetScreenPositionAndAnchorPoint(
            DrawingContext context, out Vector2 screenPosition, out Vector2 anchorPoint)
        {
            var projectedPosition = new Vector3();
            projectedPosition.X = position.X;
            projectedPosition.Y = position.Y;

            projectedPosition = context.graphics.Viewport.Project(
                projectedPosition, context.matrices.projection, context.matrices.view, AbsoluteTransform);
            
            screenPosition.X = projectedPosition.X;
            screenPosition.Y = projectedPosition.Y;

            anchorPoint = new Vector2();
            if (sourceRectangle != null)
            {
                var r = sourceRectangle.Value;
                anchorPoint.X = r.X + r.Width * anchor.X;
                anchorPoint.Y = r.Y + r.Height * anchor.Y;
            }
            else
            {
                anchorPoint.X = texture.Width * anchor.X;
                anchorPoint.Y = texture.Height * anchor.Y;
            }
        }

        void ISceneObject.OnAdded(DrawingContext context) 
        {
            var passes = context.mainPass.Passes;
            for (var i = passes.Count - 1; i >= 0; --i)
            {
                if (passes[i] is SpritePass)
                    return;
            }
            passes.Add(new SpritePass());
        }

        void ISprite.Draw(DrawingContext context, Material material) 
        {
            Draw(context, material);
        }

        void IDrawableObject.OnAddedToView(DrawingContext context) { }
        void IDrawableObject.Draw(DrawingContext context, Material material) { }
        void ISceneObject.OnRemoved(DrawingContext context) { }
        float IDrawableObject.GetDistanceToCamera(Vector3 cameraPosition) { return 0; }
    }
}
