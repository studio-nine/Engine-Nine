namespace Nine.Graphics.UI
{
    using System;
    using System.ComponentModel;
    using System.Xaml;

    /// <summary>
    /// Contains extension methods related to UI.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SceneExtensions
    {
        private static AttachableMemberIdentifier WindowManagerProperty = new AttachableMemberIdentifier(typeof(SceneExtensions), "WindowManager");

        public static WindowManager GetWindowManager(this Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            WindowManager windowManager = null;
            AttachablePropertyServices.TryGetProperty(scene, WindowManagerProperty, out windowManager);
            if (windowManager == null)
                SetWindowManager(scene, windowManager = CreateWindowManager());
            return windowManager;
        }

        public static void SetWindowManager(Scene scene, WindowManager value)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            WindowManager currentWindowManager = null;
            AttachablePropertyServices.TryGetProperty(scene, WindowManagerProperty, out currentWindowManager);
            if (currentWindowManager != null && currentWindowManager != value)
                throw new InvalidOperationException();

            if (currentWindowManager == null)
            {
                if (value != null)
                    BindWindowManager(scene, value);
                AttachablePropertyServices.SetProperty(scene, WindowManagerProperty, value);
            }
        }

        private static WindowManager CreateWindowManager()
        {
            return new WindowManager();
        }

        private static void BindWindowManager(Scene scene, WindowManager manager)
        {
            // Should it be like this or just use a Window Query
            scene.AddedToScene += (value) =>
            {
                var window = value as BaseWindow;
                if (window != null)
                    manager.windows.Add(window);
            };
            scene.RemovedFromScene += (value) =>
            {
                var window = value as BaseWindow;
                if (window != null)
                    manager.windows.Remove(window);
            };
            scene.Traverse<BaseWindow>(window =>
            {
                manager.windows.Add(window);
                return TraverseOptions.Continue;
            });
        }
    }
}
