namespace Nine.Graphics
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;

    /// <summary>
    /// Defines a skybox.
    /// </summary>
    [ContentSerializable]
    public class SkyBox : Nine.Object, IDrawableObject, IDisposable
    {
        /// <summary>
        /// Gets or sets whether the drawable is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the material used by this drawable.
        /// </summary>
        Material IDrawableObject.Material
        {
            get { return material; }
        }
        private SkyBoxMaterial material;

        /// <summary>
        /// Gets or sets the color of the skybox.
        /// </summary>
        public Vector3 Color
        {
            get { return material.Color; }
            set { material.Color = value; }
        }

        /// <summary>
        /// Gets or sets the skybox texture.
        /// </summary>
        public TextureCube Texture
        {
            get { return material.Texture; }
            set { material.Texture = value; }
        }
        private Box cube;

        public SkyBox(GraphicsDevice graphics)
        {
            Visible = true;
            material = new SkyBoxMaterial(graphics);
            cube = new Box(graphics) { InvertWindingOrder = true, Material = material, };
        }

        public void Draw(DrawingContext context, Material material)
        {
            // Skybox cannot be rendered using other materials.
            if (material == null || material == this.material)
                cube.Draw(context, this.material);
        }

        void IDrawableObject.OnAddedToView(DrawingContext context) 
        {

        }

        float IDrawableObject.GetDistanceToCamera(Vector3 cameraPosition) 
        {
            return 0; 
        }

        #region IDisposable
        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) 
        {
            if (disposing)
                cube.Dispose();
        }

        ~SkyBox()
        {
            Dispose(false);
            IsDisposed = true;
        }
        #endregion
    }
}