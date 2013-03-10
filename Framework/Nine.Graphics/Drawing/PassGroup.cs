namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;

    /// <summary>
    /// Enables a group of drawing passes to be rendered one by one.
    /// </summary>
    public class PassGroup : Pass
    {
        /// <summary>
        /// Gets the child passes of this drawing pass.
        /// </summary>
        public IList<Pass> Passes
        {
            get { return passes; }
        }
        private PassCollection passes = new PassCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="PassGroup"/> class.
        /// </summary>
        /// <param name="graphics"></param>
        public PassGroup()
        {

        }

        /// <summary>
        /// Gets all the passes that are going to be rendered.
        /// </summary>
        public override void GetActivePasses(IList<Pass> result)
        {
            var passOrder = passes.GetEnabledAndSortedOrder();
            var count = passOrder.Count;
            for (int i = 0; i < count; ++i)
                passes[passOrder[i]].GetActivePasses(result);
        }

        /// <summary>
        /// Prepares a render target to hold the result of this pass.
        /// </summary>
        public override RenderTarget2D PrepareRenderTarget(GraphicsDevice graphics, Texture2D input, SurfaceFormat? preferredFormat)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public sealed override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            throw new InvalidOperationException();
        }
    }
}