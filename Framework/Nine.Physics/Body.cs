namespace Nine.Physics
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Nine.Physics.Colliders;

    [ContentProperty("Collider")]
    public class Body : Transformable
    {
        public float Mass { get; set; }
        public Collider Collider { get; set; }

        public Body()
        {
            Mass = 1;
        }
    }
}
