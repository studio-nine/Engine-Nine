namespace Nine.Physics
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Nine.Physics.Colliders;
    using System.Collections;

    /// <summary>
    /// Defines a rigid body in the physics simulation.
    /// </summary>
    [ContentProperty("Collider")]
    public class Body : Component, IContainer
    {
        /// <summary>
        /// Gets the collider used by this body.
        /// </summary>
        public Collider Collider
        {
            get { return Collider; }
            set
            {
                if (collider != value)
                {
                    value.Collidable.BecomeDynamic(value.Mass);
                    collider = value;
                }
            }
        }
        private Collider collider;

        /// <summary>
        /// Initializes a new instance of Body.
        /// </summary>
        public Body()
        {

        }

        /// <summary>
        /// An collider will always have 1 direct child.
        /// </summary>
        IList IContainer.Children
        {
            get { ColliderList[0] = Collider; return ColliderList; }
        }
        private static Collider[] ColliderList = new Collider[1];
    }
}
