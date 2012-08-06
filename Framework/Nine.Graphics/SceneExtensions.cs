namespace Nine.Graphics
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xaml;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    
    /// <summary>
    /// Contains extension methods related to graphics.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SceneExtensions
    {
        /// <summary>
        /// Gets the drawing context of the specified scene.
        /// </summary>
        public static DrawingContext GetDrawingContext(Scene scene)
        {
            DrawingContext value = null;
            AttachablePropertyServices.TryGetProperty(scene, DrawingContextProperty, out value);
            return value;
        }

        /// <summary>
        /// Sets the drawing context of the specified scene.
        /// </summary>
        public static void SetDrawingContext(Scene scene, DrawingContext value)
        {
            AttachablePropertyServices.SetProperty(scene, DrawingContextProperty, value);
        }
        private static AttachableMemberIdentifier DrawingContextProperty = new AttachableMemberIdentifier(typeof(SceneExtensions), "DrawingContext");

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public static void Draw(this Scene scene, GraphicsDevice graphics, TimeSpan elapsedTime)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            var context = GetDrawingContext(scene);
            if (context == null)
                SetDrawingContext(scene, context = new DrawingContext(graphics, scene));
            context.Draw(elapsedTime);
        }

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public static void Draw(this Scene scene, DrawingContext context, TimeSpan elapsedTime)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");
            if (context == null)
                throw new ArgumentNullException("context");

            var existingContext = GetDrawingContext(scene);
            if (existingContext != null)
            {
                if (existingContext != context)
                    throw new InvalidOperationException();
            }
            else
            {
                SetDrawingContext(scene, existingContext = context);
            }            
            context.Draw(elapsedTime);
        }
    }
}