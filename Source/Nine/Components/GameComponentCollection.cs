#region File Description
//-----------------------------------------------------------------------------
// GameComponentCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Holds a collection of game objects and facilitates proper ordering as well 
    /// as helpers for updating and drawing components in tthe collection.
    /// </summary>
    public class GameComponentCollection : ICollection<IGameComponent>
    {
        // Three lists hold all components, all updateable components, and all drawable components, respectively
        private readonly List<IGameComponent> components = new List<IGameComponent>();
        private readonly List<IUpdateable> updateableComponents = new List<IUpdateable>();
        private readonly List<IDrawable> drawableComponents = new List<IDrawable>();

        // These lists are used as temporary contains when enumerating components in case those actions cause
        // components to be added or removed from the collection.
        private readonly List<IGameComponent> tempComponents = new List<IGameComponent>();
        private readonly List<IUpdateable> tempUpdateableComponents = new List<IUpdateable>();
        private readonly List<IDrawable> tempDrawableComponents = new List<IDrawable>();

        // Whether or not the collection has been initialized. This prevents double initialization and makes sure
        // that components added after the collection was initialized have their Initialize methods called
        private bool isInitialized = false;

        /// <summary>
        /// Gets the number of components in the collection.
        /// </summary>
        public int Count { get { return components.Count; } }

        /// <summary>
        /// Explicitly implemented because this isn't really useful for most cases, but is part of the interface.
        /// </summary>
        bool ICollection<IGameComponent>.IsReadOnly { get { return false; } }

        /// <summary>
        /// Invoked when a component is added to the collection.
        /// </summary>
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;

        /// <summary>
        /// Invoked when a component is removed from the collection.
        /// </summary>
        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

        /// <summary>
        /// Initializes all components.
        /// </summary>
        internal void Initialize()
        {
            // Prevent multiple initializations
            if (isInitialized)
                return;

            // Copy the components into our temporary list in case one of the Initialize methods
            // modifies the component collection.
            tempComponents.Clear();
            tempComponents.AddRange(components);
            
            // Enumerate the temporary list and initialize the components
            foreach (IGameComponent component in tempComponents)
            {
                component.Initialize();
            }

            // We are now initialized
            isInitialized = true;
        }

        /// <summary>
        /// Updates all enabled components.
        /// </summary>
        internal void Update(GameTime gameTime)
        {
            // Copy the components into our temporary list in case one of the Update methods
            // modifies the component collection.
            tempUpdateableComponents.Clear();
            tempUpdateableComponents.AddRange(updateableComponents);

            // Enumerate the temporary list and update any enabled components
            foreach (IUpdateable updateable in tempUpdateableComponents)
            {
                if (updateable.Enabled)
                    updateable.Update(gameTime);
            }
        }

        /// <summary>
        /// Draws all visible components.
        /// </summary>
        internal void Draw(GameTime gameTime)
        {
            // Copy the components into our temporary list in case one of the Draw methods
            // modifies the component collection.
            tempDrawableComponents.Clear();
            tempDrawableComponents.AddRange(drawableComponents);

            // Enumerate the temporary list and draw any visible components
            foreach (IDrawable drawable in tempDrawableComponents)
            {
                if (drawable.Visible)
                    drawable.Draw(gameTime);
            }
        }

        #region ICollection<T> Methods

        /// <summary>
        /// Adds a component to the collection.
        /// </summary>
        /// <param name="item">The component to add.</param>
        public void Add(IGameComponent item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            // Do nothing if the component is already in the collection
            if (components.Contains(item))
                return;

            // Add the item
            components.Add(item);

            // See if the item is updateable and/or drawable
            IUpdateable updateable = item as IUpdateable;
            IDrawable drawable = item as IDrawable;

            // If the component is updateable, add it to our updateable list, subscribe to its UpdateOrderChanged
            // event so we can keep the list in order, and manually invoke the event handler to sort the list.
            if (updateable != null)
            {
                updateableComponents.Add(updateable);
                updateable.UpdateOrderChanged += ComponentUpdateOrderChanged;
                ComponentUpdateOrderChanged(this, EventArgs.Empty);
            }

            // If the component is drawable, add it to our drawable list, subscribe to its DrawOrderChanged
            // event so we can keep the list in order, and manually invoke the event handler to sort the list.
            if (drawable != null)
            {
                drawableComponents.Add(drawable);
                drawable.DrawOrderChanged += ComponentDrawOrderChanged;
                ComponentDrawOrderChanged(this, EventArgs.Empty);
            }

            // If the collection has been initialized, call Initialize on the new item
            if (isInitialized)
                item.Initialize();

            // Invoke the ComponentAdded event
            if (ComponentAdded != null)
                ComponentAdded(this, new GameComponentCollectionEventArgs(item));
        }

        /// <summary>
        /// Removes a component from the collection.
        /// </summary>
        /// <param name="item">The component to be removed.</param>
        /// <returns>True if the component was removed, false if the component wasn't in the collection.</returns>
        public bool Remove(IGameComponent item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (components.Remove(item))
            {
                // See if the item is updateable and/or drawable
                IUpdateable updateable = item as IUpdateable;
                IDrawable drawable = item as IDrawable;

                // If the component is updateable, remove it from the updateable list and 
                // unsubscribe to its UpdateOrderChanged event
                if (updateable != null)
                {
                    updateableComponents.Remove(updateable);
                    updateable.UpdateOrderChanged -= ComponentUpdateOrderChanged;
                }

                // If the component is drawabe, remove it from the drawable list and 
                // unsubscribe to its DrawOrderChanged event
                if (drawable != null)
                {
                    drawableComponents.Remove(drawable);
                    drawable.DrawOrderChanged -= ComponentDrawOrderChanged;
                }

                // Invoke the ComponentRemoved event
                if (ComponentRemoved != null)
                    ComponentRemoved(this, new GameComponentCollectionEventArgs(item));
            }

            return false;
        }

        /// <summary>
        /// Clears the collection, removing all components.
        /// </summary>
        public void Clear()
        {
            // Copy the components to the temporary list so we can remove them
            tempComponents.Clear();
            tempComponents.AddRange(components);

            // Now remove each component
            foreach (IGameComponent component in tempComponents)
                Remove(component);
        }

        /// <summary>
        /// Determines if the collection contains a component.
        /// </summary>
        /// <param name="item">The component for which to check.</param>
        /// <returns>True if the component is in the collection, false otherwise.</returns>
        public bool Contains(IGameComponent item)
        {
            return components.Contains(item);
        }

        /// <summary>
        /// Copies the components into an array.
        /// </summary>
        /// <param name="array">The array into which the components will be copied.</param>
        /// <param name="arrayIndex">The starting index in the array at which to start copying the components.</param>
        public void CopyTo(IGameComponent[] array, int arrayIndex)
        {
            components.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator that can be used to iterate over the components.
        /// </summary>
        public IEnumerator<IGameComponent> GetEnumerator()
        {
            // Place all components into the temporary list in case the caller
            // adds/removes components during enumeration.
            tempComponents.Clear();
            tempComponents.AddRange(components);
            return tempComponents.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Sorting Helpers

        /// <summary>
        /// Invoked when a component's UpdateOrder changed so we can reorder our list.
        /// </summary>
        private void ComponentUpdateOrderChanged(object sender, EventArgs args)
        {
            updateableComponents.Sort(UpdateableSort);
        }

        /// <summary>
        /// Invoked when a component's DrawOrder changed so we can reorder our list.
        /// </summary>
        private void ComponentDrawOrderChanged(object sender, EventArgs args)
        {
            drawableComponents.Sort(DrawableSort);
        }

        /// <summary>
        /// Helper that used to sort a list of IUpdateable components.
        /// </summary>
        private static int UpdateableSort(IUpdateable a, IUpdateable b)
        {
            return a.UpdateOrder.CompareTo(b.UpdateOrder);
        }

        /// <summary>
        /// Helper that used to sort a list of IDrawable components.
        /// </summary>
        private static int DrawableSort(IDrawable a, IDrawable b)
        {
            return a.DrawOrder.CompareTo(b.DrawOrder);
        }

        #endregion
    }
}
