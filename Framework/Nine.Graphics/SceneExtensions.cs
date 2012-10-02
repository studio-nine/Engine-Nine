namespace Nine.Graphics
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xaml;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    
    /// <summary>
    /// Contains extension methods related to graphics.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SceneExtensions
    {
        /// <summary>
        /// Gets the graphics device associated with the target group.
        /// </summary>
        public static GraphicsDevice GetGraphicsDevice(this Group group)
        {
#if SILVERLIGHT
            return System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice;
#else
            return group.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice;
#endif
        }

        /// <summary>
        /// Gets the content manager associated with the target group.
        /// </summary>
        public static ContentManager GetContentManager(this Group group)
        {
            return group.ServiceProvider.GetService<ContentManager>();
        }

        /// <summary>
        /// Gets the drawing context of the specified scene.
        /// </summary>
        private static DrawingContext GetDrawingContextInternal(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            DrawingContext value = null;
            AttachablePropertyServices.TryGetProperty(scene, DrawingContextProperty, out value);
            return value;
        }

        /// <summary>
        /// Gets the drawing context of the specified scene.
        /// Creates a default drawing context if no drawing context is currently bound
        /// to the scene.
        /// </summary>
        public static DrawingContext GetDrawingContext(this Scene scene)
        {
            return GetDrawingContext(scene, GetGraphicsDevice(scene));
        }

        /// <summary>
        /// Gets the drawing context of the specified scene. 
        /// Creates a default drawing context if no drawing context is currently bound
        /// to the scene.
        /// </summary>
        public static DrawingContext GetDrawingContext(this Scene scene, GraphicsDevice graphics)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            var context = GetDrawingContextInternal(scene);
            if (context == null)
                SetDrawingContext(scene, context = new DrawingContext(graphics, (ISpatialQuery)scene));
            else if (context.graphics != graphics)
                throw new ArgumentException("graphics");

            if (context.camera == null)
                scene.Add(new FreeCamera(graphics, new Vector3(0, 10, 40)));

            return context;
        }

        /// <summary>
        /// Sets the drawing context of the specified scene.
        /// </summary>
        public static void SetDrawingContext(Scene scene, DrawingContext value)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            var currentContext = GetDrawingContextInternal(scene);
            if (currentContext != null && currentContext != value)
                throw new InvalidOperationException();

            if (currentContext == null)
            {
                if (value != null)
                    BindDrawingContext(scene, value);
                AttachablePropertyServices.SetProperty(scene, DrawingContextProperty, value);
            }
        }
        private static AttachableMemberIdentifier DrawingContextProperty = new AttachableMemberIdentifier(typeof(SceneExtensions), "DrawingContext");

        /// <summary>
        /// Draws the specified scene.
        /// </summary>
        public static void Draw(this Scene scene, GraphicsDevice graphics, TimeSpan elapsedTime)
        {
            GetDrawingContext(scene, graphics).Draw(elapsedTime);
        }

        /// <summary>
        /// Draws diagnostic information of the target scene.
        /// </summary>
        public static void DrawDiagnostics(this Scene scene, GraphicsDevice graphics, TimeSpan elapsedTime)
        {
            GetDrawingContext(scene, graphics).DrawDiagnostics();
        }

        /// <summary>
        /// Binds scene added/removed events to the drawing context.
        /// </summary>
        private static void BindDrawingContext(Scene scene, DrawingContext context)
        {
            scene.AddedToScene += (value) =>
            {
                var sceneObject = value as IGraphicsObject;
                if (sceneObject != null)
                    sceneObject.OnAdded(context);                
            };
            scene.RemovedFromScene += (value) =>
            {
                var sceneObject = value as IGraphicsObject;
                if (sceneObject != null)
                    sceneObject.OnRemoved(context);
            };
            scene.Traverse<IGraphicsObject>(sceneObject =>
            {
                sceneObject.OnAdded(context);
                return TraverseOptions.Continue;
            });
        }
    }
}