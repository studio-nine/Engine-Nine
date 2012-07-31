namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// Defines a pass that draws the scene depth buffer prior to the actual rendering.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DepthPrePass : Pass, IDisposable
    {
        private DepthMaterial depthMaterial;
        private RenderTarget2D depthBuffer;

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            var graphics = context.GraphicsDevice;
            if (depthMaterial == null)
                depthMaterial = new DepthMaterial(graphics);

            if (depthBuffer == null || depthBuffer.IsDisposed || depthBuffer.IsContentLost)
            {
                if (depthBuffer != null)
                    depthBuffer.Dispose();
                depthBuffer = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, 
                    false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            }

            try
            {
                depthBuffer.Begin();

                graphics.Clear(Color.White);

                graphics.SamplerStates[0] = SamplerState.PointClamp;
                graphics.DepthStencilState = DepthStencilState.Default;
                graphics.BlendState = BlendState.Opaque;

                var count = drawables.Count;
                for (int i = 0; i < count; i++)
                {
                    var drawable = drawables[i];
                    if (drawable == null || !drawable.Visible)
                        continue;

                    // TODO: Two sided...
                    var material = drawable.Material;
                    if (material == null)
                    {
                        drawable.Draw(context, depthMaterial);
                        continue;
                    }

                    // Ignore transparent objects
                    if ((material = material.GetMaterialByUsage(MaterialUsage.Depth)) != null)
                        drawable.Draw(context, material);
                }
            }
            finally
            {
                depthBuffer.End();
                context.textures[TextureUsage.DepthBuffer] = depthBuffer;
            }
        }        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (depthBuffer != null)
                {
                    depthBuffer.Dispose();
                    depthBuffer = null;
                }
            }
        }

        ~DepthPrePass()
        {
            Dispose(false);
        }
    }
}