namespace Nine.Graphics.UI.Controls
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

    // TODO: Move this to a window

    /// <summary>
    /// A Control that lets you navigate <see cref="Page">Pages</see>.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Pages")]
    public class PageNavigator : UIElement, IContainer, INotifyCollectionChanged<UIElement>
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
                    Pages[selectedIndex].Visible = false;
                selectedIndex = value;
                if (Pages.Count > value)
                    Pages[selectedIndex].Visible = true;

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

        // This have to be updated to work better
        protected override Microsoft.Xna.Framework.Vector2 MeasureOverride(Microsoft.Xna.Framework.Vector2 availableSize)
        {
            pages.ForEach<Page>(o => o.Measure(availableSize));
            return base.MeasureOverride(availableSize);
        }

        protected override Microsoft.Xna.Framework.Vector2 ArrangeOverride(Microsoft.Xna.Framework.Vector2 finalSize)
        {
            pages.ForEach<Page>(o => o.Arrange(new BoundingRectangle(0, 0, finalSize.X, finalSize.Y)));
            return base.ArrangeOverride(finalSize);
        }

        protected internal override void OnRender(Renderer.Renderer renderer)
        {
            base.OnRender(renderer);
            pages[SelectedIndex].OnRender(renderer);
        }

        private void Child_Added(object value)
        {
            var element = value as UIElement;
            if (element != null)
            {
                if (Pages.Count == 0)
                    element.Visible = true;

                element.Parent = this;
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
    }
}
