namespace Nine.Navigation
{
    using System;
    using System.Xml.Serialization;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// Defines a navigation component that can be added to a game object container.
    /// </summary>
    [Serializable]
    public class NavigatorComponent : Component, IUpdateable, IServiceProvider, ICloneable
    {
        /// <summary>
        /// Gets or sets the animation when the navigator is moving.
        /// </summary>
        public string MoveAnimation { get; set; }

        /// <summary>
        /// Gets or sets the animation when the navigator has stopped.
        /// </summary>
        public string StopAnimation { get; set; }

        /// <summary>
        /// Gets the underlying navigator.
        /// </summary>
        [XmlIgnore]
        public Navigator Navigator { get; private set; }

        protected override void OnAdded(WorldObject parent)
        {
            CreateNavigator();
        }

        /// <summary>
        /// Creates the navigator.
        /// </summary>
        internal void CreateNavigator()
        {
            Navigator = new Navigator();
            Navigator.Position = Parent.Position;
            Navigator.Ground = Parent.Find<ISurface>();
            Navigator.PathGraph = Parent.Find<IPathGraph>();
            Navigator.Friends = Parent.Find<ISpatialQuery<Navigator>>();
            Navigator.Opponents = Parent.Find<ISpatialQuery<Navigator>>();
            Navigator.Started += new EventHandler<EventArgs>(Navigator_Started);
            Navigator.Stopped += new EventHandler<EventArgs>(Navigator_Stopped);

            Parent.Transform = Navigator.Transform;
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime)
        {
            if (Navigator != null)
            {
                Navigator.Update(elapsedTime);

                if (Navigator.State == NavigatorState.Moving)
                {
                    var transformable = Parent as ITransformable;
                    if (transformable != null)
                        transformable.Transform = Navigator.Transform;
                }
            }
        }

        void Navigator_Stopped(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(StopAnimation))
            {
                var displayObject = Parent.Find<DrawingGroup>();
                if (displayObject != null)
                    displayObject.Animations.Play(StopAnimation);
            }
        }

        void Navigator_Started(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(StopAnimation))
            {
                var displayObject = Parent.Find<DrawingGroup>();
                if (displayObject != null)
                    displayObject.Animations.Play(MoveAnimation);
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (Navigator != null && serviceType.IsAssignableFrom(typeof(Navigator)))
                return Navigator;
            return null;
        }

        public NavigatorComponent Clone()
        {
            return new NavigatorComponent()
            {
                Name = Name,
                Tag = Tag,
                MoveAnimation = MoveAnimation,
                StopAnimation = StopAnimation,
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
