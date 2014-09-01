namespace Nine.Graphics.UI
{
    using System;
    using System.ComponentModel;
    using System.Xaml;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SceneExtensions
    {
        /// <summary>
        /// Gets the Window Manager.
        /// </summary>
        public static WindowManager GetWindowManager(this Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            var value = GetWindowManagerInternal(scene);
            if (value == null)
                SetWindowManager(scene, value = new WindowManager());

            return value;
        }

        /// <summary>
        /// Sets the Window Manager.
        /// </summary>
        public static void SetWindowManager(Scene scene, WindowManager value)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            var currentValue = GetWindowManagerInternal(scene);
            if (currentValue != null && currentValue != value)
                throw new InvalidOperationException();

            if (currentValue == null)
            {
                if (value != null)
                    BindWindowManager(scene, value);
                AttachablePropertyServices.SetProperty(scene, WindowManagerProperty, value);
            }
        }
        private static AttachableMemberIdentifier WindowManagerProperty = new AttachableMemberIdentifier(typeof(SceneExtensions), "WindowManager");


        /// <summary>
        /// Gets the Window Manager of the specified scene.
        /// </summary>
        private static WindowManager GetWindowManagerInternal(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            WindowManager value = null;
            AttachablePropertyServices.TryGetProperty(scene, WindowManagerProperty, out value);
            return value;
        }

        /// <summary>
        /// Binds scene added/removed events to the Window Manager.
        /// </summary>
        private static void BindWindowManager(Scene scene, WindowManager windowManager)
        {
            scene.AddedToScene += (value) =>
            {
                var window = value as BaseWindow;
                if (window != null)
                    windowManager.windows.Add(window);
            };
            scene.RemovedFromScene += (value) =>
            {
                var window = value as BaseWindow;
                if (window != null)
                    windowManager.windows.Remove(window);
            };
            scene.Traverse<BaseWindow>(value =>
            {
                windowManager.windows.Add(value);
                return TraverseOptions.Continue;
            });
        }
    }
}
