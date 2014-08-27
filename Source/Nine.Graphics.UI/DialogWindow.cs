namespace Nine.Graphics.UI
{
    using System;
    using System.Collections;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Media;
    using Nine.Graphics.UI.Renderer;

    public enum ResizeMode
    {
        CanResize,
        //CanResizeWithGrip,
        NoResize,
    }

    internal enum DialogWindowState
    {
        Default,
        Drag,
        Resize,
        ResizeX1, ResizeY1, // Bottom and Right
        ResizeX2, ResizeY2, // Top and Left
    }

    /// <summary>
    /// A draggable window with a frame
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class DialogWindow : BaseWindow, IContainer
    {
        #region Properties

        public UIElement Content
        {
            get { return windowBorder.Content; }
            set
            {
                if (windowBorder.Content != value)
                {
                    windowBorder.Content = value;
                    windowBorder.Content.Window = this;
                }
            }
        }

        IList IContainer.Children { get { return ((IContainer)windowBorder).Children; } }

        public Thickness WindowBorder 
        {
            get { return windowBorder.BorderThickness; }
            set { windowBorder.BorderThickness = value; }
        }

        public Brush WindowBorderBrush
        {
            get { return windowBorder.BorderBrush; }
            set { windowBorder.BorderBrush = value; }
        }

        public Brush Background
        {
            get { return windowBorder.Background; }
            set { windowBorder.Background = value; }
        }

        public Thickness Padding
        {
            get { return windowBorder.Padding; }
            set { windowBorder.Padding = value; }
        }

        public Vector2 MinSize
        {
            get { return minSize; }
            set { minSize = value; }
        }
        private Vector2 minSize = new Vector2(100, 100);

        public Vector2 MaxSize
        {
            get { return maxSize; }
            set { maxSize = value; }
        }
        private Vector2 maxSize = new Vector2(float.MaxValue, float.MaxValue);

        public ResizeMode ResizeMode { get; set; }
        public bool LockPosition { get; set; }

        public TextBlock TitleElement
        {
            get { return title; }
            set { title = value; }
        }

        public string Title
        {
            get { return title.Text; }
            set { title.Text = value; }
        }

        public SpriteFont TitleFont
        {
            get { return title.Font; }
            set { title.Font = value; }
        }

        #endregion

        #region Fields
        
        private Border windowBorder;
        private TextBlock title;
        private Vector2 mouseOffset = Vector2.Zero;
        internal DialogWindowState state = DialogWindowState.Default;

        #endregion

        public DialogWindow()
        {
            windowBorder = new Border(new Media.SolidColorBrush(Color.White), new Thickness(8, 32, 8, 8));
            windowBorder.Window = this;

            title = new TextBlock();
            title.Window = this;
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.VerticalAlignment = VerticalAlignment.Center;


            // TODO: 
            windowBorder.MouseMove += WindowMouseMove;
            windowBorder.MouseDown += WindowMouseDown;
            windowBorder.MouseUp += WindowMouseUp;
        }

        #region Input

        void WindowMouseMove(object sender, MouseEventArgs e)
        {
            switch (state)
            {
                case DialogWindowState.Default:
                    break;
                case DialogWindowState.Drag:
                    {
                        var viewport = Viewport;
                        viewport.X = e.X + mouseOffset.X;
                        viewport.Y = e.Y + mouseOffset.Y;
                        Viewport = viewport;
                    }
                    break;
                case DialogWindowState.Resize:
                    {
                        float x = e.X - (windowBorder.AbsoluteRenderTransform.X + windowBorder.RenderSize.X);
                        float y = e.Y - (windowBorder.AbsoluteRenderTransform.Y + windowBorder.RenderSize.Y);

                        var viewport = Viewport;
                        viewport.Width = MathHelper.Clamp(viewport.Width + x - mouseOffset.X, MinSize.X, MaxSize.X);
                        viewport.Height = MathHelper.Clamp(viewport.Height + y - mouseOffset.Y, MinSize.Y, MaxSize.Y);
                        Viewport = viewport;

                        Resized(viewport.Width, viewport.Height);
                    }
                    break;
                case DialogWindowState.ResizeX1:
                    {
                        float x = e.X - (windowBorder.AbsoluteRenderTransform.X + windowBorder.RenderSize.X);

                        var viewport = Viewport;
                        viewport.Width = MathHelper.Clamp(viewport.Width + x - mouseOffset.X, MinSize.X, MaxSize.X);
                        Viewport = viewport;

                        Resized(viewport.Width, viewport.Height);
                    }
                    break;
                case DialogWindowState.ResizeY1:
                    {
                        float y = e.Y - (windowBorder.AbsoluteRenderTransform.Y + windowBorder.RenderSize.Y);

                        var viewport = Viewport;
                        viewport.Height = MathHelper.Clamp(viewport.Height + y - mouseOffset.Y, MinSize.Y, MaxSize.Y);
                        Viewport = viewport;

                        Resized(viewport.Width, viewport.Height);
                    }
                    break;
            }
        }

        void WindowMouseDown(object sender, MouseEventArgs e)
        {
            if (windowBorder.AbsoluteRenderTransform.Contains(e.X, e.Y) == ContainmentType.Contains)
            {
                if (!LockPosition)
                {
                    var rect = windowBorder.GetBorder(Direction.Top);
                    rect.X += windowBorder.AbsoluteRenderTransform.X;
                    rect.Y += windowBorder.AbsoluteRenderTransform.Y;
                    if (rect.Contains(e.X, e.Y) == ContainmentType.Contains)
                    {
                        mouseOffset = new Vector2(rect.X - e.X, rect.Y - e.Y);
                        state = DialogWindowState.Drag;
                    }
                }
                switch (ResizeMode)
                {
                    case UI.ResizeMode.CanResize:
                        resizeCanResize(e.X, e.Y);
                        break;
                    case UI.ResizeMode.NoResize: 
                        break;
                }
            }
        }

        void WindowMouseUp(object sender, MouseEventArgs e)
        {
            state = DialogWindowState.Default;
        }

        #endregion

        protected virtual void Resized(float width, float height) 
        { 
            
        }

        public override void Draw(DrawingContext context, System.Collections.Generic.IList<IDrawableObject> drawables)
        {
            base.Draw(context, drawables);

            windowBorder.Measure(new Vector2(Viewport.Width, Viewport.Height));
            windowBorder.Arrange(Viewport);

            BoundingRectangle titleBounds = new BoundingRectangle
            {
                X = Viewport.X,
                Y = Viewport.Y,
                Width = Viewport.Width,
                Height = windowBorder.BorderThickness.Top,
            };

            title.Measure(new Vector2(titleBounds.Width, windowBorder.BorderThickness.Top));
            title.Arrange(titleBounds);

            if (Renderer == null)
                Renderer = new SpriteBatchRenderer(context.GraphicsDevice);

            Renderer.elapsedTime = context.ElapsedTime;
            Renderer.Begin(context);
            windowBorder.Render(Renderer);
            title.Render(Renderer);
            Renderer.End(context);
        }

        private void resizeCanResize(float x, float y)
        {
            // this is just an idea for now!

            // Drag
            var rect = windowBorder.GetBorder(Direction.Top);
            rect.X += windowBorder.AbsoluteRenderTransform.X;
            rect.Y += windowBorder.AbsoluteRenderTransform.Y + 8;
            rect.Width -= windowBorder.BorderThickness.Right;
            rect.Height -= 8;
            if (rect.Contains(x, y) == ContainmentType.Contains)
            {
                mouseOffset = new Vector2(rect.X, rect.Y) - new Vector2(x, y);
                state = DialogWindowState.Drag;
                return;
            }
            
            // Resize
            var resizeRectBottom = windowBorder.GetBorder(Direction.Bottom);
            resizeRectBottom.X += windowBorder.AbsoluteRenderTransform.X;
            resizeRectBottom.Y += windowBorder.AbsoluteRenderTransform.Y;
            if (resizeRectBottom.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNS;
#endif
                mouseOffset = new Vector2(resizeRectBottom.X, resizeRectBottom.Y) - new Vector2(x, y);
                state = DialogWindowState.ResizeY1;
                return;
            }

            var recttop = windowBorder.GetBorder(Direction.Top);
            recttop.X += windowBorder.AbsoluteRenderTransform.X;
            recttop.Y += windowBorder.AbsoluteRenderTransform.Y;
            recttop.Width -= windowBorder.BorderThickness.Right;
            recttop.Height -= 24;
            if (recttop.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNS;
#endif
                mouseOffset = new Vector2(recttop.X, recttop.Y) - new Vector2(x, y);
                state = DialogWindowState.ResizeY2;
                return;
            }

            var resizeRectRight = windowBorder.GetBorder(Direction.Right);
            resizeRectRight.X += windowBorder.AbsoluteRenderTransform.X;
            resizeRectRight.Y += windowBorder.AbsoluteRenderTransform.Y;
            resizeRectRight.Height -= windowBorder.BorderThickness.Bottom;
            if (resizeRectRight.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
#endif
                mouseOffset = new Vector2(resizeRectRight.X, resizeRectRight.Y) - new Vector2(x, y);
                state = DialogWindowState.ResizeX1;
                return;
            }

            var resizeRectLeft = windowBorder.GetBorder(Direction.Left);
            resizeRectLeft.X += windowBorder.AbsoluteRenderTransform.X;
            resizeRectLeft.Y += windowBorder.AbsoluteRenderTransform.Y + windowBorder.BorderThickness.Top;
            resizeRectLeft.Height -= windowBorder.BorderThickness.Bottom + windowBorder.BorderThickness.Top;
            if (resizeRectLeft.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
#endif
                mouseOffset = Vector2.Zero;
                state = DialogWindowState.ResizeX2;
                return;
            }

            // resize corners
            var rectx1 = new BoundingRectangle()
            {
                X = windowBorder.AbsoluteRenderTransform.X + windowBorder.RenderSize.X - windowBorder.BorderThickness.Right,
                Y = windowBorder.AbsoluteRenderTransform.Y + windowBorder.RenderSize.Y - windowBorder.BorderThickness.Bottom,
                Width = windowBorder.BorderThickness.Right,
                Height = windowBorder.BorderThickness.Bottom,
            };
            if (rectx1.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNWSE;
#endif
                mouseOffset = Vector2.Zero;
                state = DialogWindowState.Resize;
                return;
            }

            var rectx2 = new BoundingRectangle()
            {
                X = windowBorder.AbsoluteRenderTransform.X,
                Y = windowBorder.AbsoluteRenderTransform.Y + windowBorder.RenderSize.Y - windowBorder.BorderThickness.Bottom,
                Width = windowBorder.BorderThickness.Left,
                Height = windowBorder.BorderThickness.Bottom,
            };
            if (rectx2.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNESW;
#endif
                mouseOffset = Vector2.Zero;
                state = DialogWindowState.Resize;
                return;
            }

            var rectx3 = new BoundingRectangle()
            {
                X = windowBorder.AbsoluteRenderTransform.X,
                Y = windowBorder.AbsoluteRenderTransform.Y,
                Width = windowBorder.BorderThickness.Left,
                Height = windowBorder.BorderThickness.Top,
            };
            if (rectx3.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNWSE;
#endif
                mouseOffset = Vector2.Zero;
                state = DialogWindowState.Resize;
                return;
            }

            var rectx4 = new BoundingRectangle()
            {
                X = windowBorder.AbsoluteRenderTransform.X + windowBorder.RenderSize.X - windowBorder.BorderThickness.Right,
                Y = windowBorder.AbsoluteRenderTransform.Y,
                Width = windowBorder.BorderThickness.Left,
                Height = windowBorder.BorderThickness.Top,
            };
            if (rectx4.Contains(x, y) == ContainmentType.Contains)
            {
#if WINDOWS
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNESW;
#endif
                mouseOffset = Vector2.Zero;
                state = DialogWindowState.Resize;
                return;
            }
        }
    }
}
