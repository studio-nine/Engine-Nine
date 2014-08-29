namespace Nine.Graphics.UI.Navigation
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// A Control that lets you navigate <see cref="Page">Pages</see>.
    /// </summary>
    [Obsolete]
    [System.Windows.Markup.ContentProperty("Pages")]
    public class PageNavigator : BaseWindow, IContainer, INotifyCollectionChanged<UIElement>
    {
        #region Properties

        public IList<Page> Pages { get { return pages; } }
        private NotificationCollection<Page> pages;

        public int SelectedIndex 
        {
            get { return selectedIndex; }
            set
            {
                if (Pages.Count > selectedIndex)
                    Pages[selectedIndex].Visibility = Visibility.Collapsed;
                selectedIndex = value;
                if (Pages.Count > value)
                    Pages[selectedIndex].Visibility = Visibility.Visible;

                if (SelectedIndexChanged != null)
                    SelectedIndexChanged();
            }
        }
        private int selectedIndex = 0;

        IList IContainer.Children { get { return (IList)Pages; } }

        #endregion

        #region Events

        /// <summary>
        /// Occures when a Page is added.
        /// </summary>
        public event Action<UIElement> Added;

        /// <summary>
        /// Occures when a Page is Removed.
        /// </summary>
        public event Action<UIElement> Removed;

        /// <summary>
        /// Occures when Selected Index is Changed.
        /// </summary>
        public event Action SelectedIndexChanged;

        #endregion

        #region Constructer

        public PageNavigator()
        {
            pages = new NotificationCollection<Page>();
            pages.Sender = this;
            pages.Added += Child_Added;
            pages.Removed += Child_Removed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Navigate to the next page in the list.
        /// </summary>
        /// <returns>if it was successful></returns>
        public bool NextPageInList()
        {
            if (SelectedIndex < pages.Count)
            {
                SelectedIndex++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Navigate to the prev page in the list.
        /// </summary>
        /// <returns>if it was successful></returns>
        public bool PrevPageInList()
        {
            if (SelectedIndex < pages.Count && SelectedIndex > 0)
            {
                SelectedIndex--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Navigate to another page.
        /// </summary>
        /// <param name="name">Name of the Page</param>
        /// <returns>if it was successful</returns>
        public bool NavigateTo(string name)
        {
            var spage = pages.Where(o => o.Name == name).First();
            if (spage != null)
            {
                SelectedIndex = pages.IndexOf(spage);
                return true;
            }
            return false;
        }

        private void Child_Added(object value)
        {
            var element = value as UIElement;
            if (element != null)
            {
                if (Pages.Count == 0)
                    element.Visibility = Visibility.Visible;

                element.Window = this;
                if (Added != null)
                    Added.Invoke(element);
            }
        }

        private void Child_Removed(object value)
        {
            if (Removed != null)
                Removed.Invoke(value as UIElement);
        }

        #endregion

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (this.Viewport != (BoundingRectangle)context.GraphicsDevice.Viewport.TitleSafeArea)
                this.Viewport = (BoundingRectangle)context.GraphicsDevice.Viewport.TitleSafeArea;

            if (Viewport == null)
                throw new ArgumentNullException("Viewport");

            BoundingRectangle bounds = new BoundingRectangle
            {
                X = Viewport.X,
                Y = Viewport.Y,
                Width = Viewport.Width,
                Height = Viewport.Height,
            };

            var availableSize = new Vector2(bounds.Width, bounds.Height);
            pages.ForEach<Page>(o => o.Measure(availableSize));
            pages.ForEach<Page>(o => o.Arrange(bounds));

            pages[SelectedIndex].Draw(Renderer);
            base.Draw(context, drawables);
        }
    }
}
