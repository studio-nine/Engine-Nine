namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

    /// <summary>
    /// Defines a pass that draws the scene depth buffer prior to the actual rendering.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DepthPrePass : Pass, IDisposable
    {
        private DepthMaterial depthMaterial;
        private RenderTarget2D depthBuffer;
        private DrawingPass drawingPass;

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            var rootPasses = context.RootPass.Passes;
            for (int i = 0; i < rootPasses.Count; i++)
            {
                if (rootPasses[i].Enabled && rootPasses[i] is LightPrePass)
                    return;
            }

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

            if (drawingPass == null)
            {
                drawingPass = new DrawingPass();
                drawingPass.MaterialUsage = MaterialUsage.Depth;                
            }

            try
            {
                depthBuffer.Begin();
                graphics.Clear(Color.White);
                drawingPass.Draw(context, drawables);
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