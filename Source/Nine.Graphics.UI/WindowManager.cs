namespace Nine.Graphics.UI
{
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using System.Collections;

    /// <summary>
    /// Handles multiple windows and input
    /// </summary>
    [System.Windows.Markup.ContentProperty("Windows")]
    public class WindowManager : Nine.Object, IContainer
    {
        public bool InputEnabled
        {
            get { return input != null && input.Enabled; }
            set
            {
                if (value)
                    EnsureInput();
                if (input != null)
                    input.Enabled = value;
            }
        }

        IList IContainer.Children { get { return windows; } }
        public IList<BaseWindow> Windows
        {
            get { return windows; }
        }
        private NotificationCollection<BaseWindow> windows;

        private Input input;

        public WindowManager()
        {
            windows = new NotificationCollection<BaseWindow>();
            EnsureInput();
        }

        #region Mouse

        void MouseMove(object sender, MouseEventArgs e)
        {
            // var win = windows.Where(o => o.Viewport.Contains(new Point(e.X, e.Y))).OrderByDescending(o => o.ZDepth);
            // if (win.Count() > 0)
            // {
            //     var window = win.First();
            //     window.MouseMove(sender, e);
            // }
            foreach (var window in windows)
            {
                window.MouseMove(sender, e);
            }
        }
        
        void MouseUp(object sender, MouseEventArgs e)
        {
            // var win = windows.Where(o => o.Viewport.Intersects(new Rectangle(e.X, e.Y, 1, 1))).OrderBy(o => o.ZDepth);
            // if (win.Count() > 0)
            // {
            //     var window = win.First();
            //     window.MouseUp(sender, e);
            // }
            foreach (var window in windows)
            {
                window.MouseUp(sender, e);
            }
        }

        void MouseDown(object sender, MouseEventArgs e)
        {
            // var win = windows.Where(o => o.Viewport.Intersects(new Rectangle(e.X, e.Y, 1, 1))).OrderBy(o => o.ZDepth);
            // if (win.Count() > 0)
            // {
            //     var window = win.First();
            //     if (window.ZDepth != c)
            //     {
            //         c++;
            //         window.ZDepth = c;
            //         var toAdd = windows.OrderByDescending(o => o.ZDepth);
            //         windows.Clear();
            //         windows.AddRange(toAdd);
            //     }
            //     window.MouseDown(sender, e);
            // }
            foreach (var window in windows)
            {
                window.MouseDown(sender, e);
            }
        }

        void MouseWheel(object sender, MouseEventArgs e)
        {
            var win = windows.Where(o => o.Viewport.Contains(e.X, e.Y) == ContainmentType.Contains).OrderBy(o => o.ZDepth);
            if (win.Count() > 0)
            {
                var window = win.First();
                window.MouseWheel(sender, e);
            }
        }

        #endregion

        #region Keyboard

        void KeyDown(object sender, KeyboardEventArgs e)
        {

        }

        void KeyUp(object sender, KeyboardEventArgs e)
        {

        }

        #endregion

        #region GamePad

        void ButtonUp(object sender, GamePadEventArgs e)
        {

        }

        void ButtonDown(object sender, GamePadEventArgs e)
        {

        }

        #endregion

        #region Touch

        void GestureSampled(object sender, GestureEventArgs e)
        {

        }

        #endregion

        private void EnsureInput()
        {
            if (input == null)
            {
                input = new Input();
                input.MouseMove += MouseMove;
                input.MouseUp += MouseUp;
                input.MouseDown += MouseDown;
                input.MouseWheel += MouseWheel;
                input.KeyDown += KeyDown;
                input.KeyUp += KeyUp;
                input.ButtonUp += ButtonUp;
                input.ButtonDown += ButtonDown;
                input.GestureSampled += GestureSampled;
            }
        }
    }
}
