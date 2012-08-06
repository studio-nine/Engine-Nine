namespace Nine.Physics.Colliders
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;

    public abstract class Collider : Transformable
    {
        public Range<float> Friction { get; set; }
        public float Restitution { get; set; }

        protected Collider()
        {
            Friction = new Range<float>(0.6f, 0.8f);
            Restitution = 0;
        }
    }
        
    [ContentProperty("Colliders")]
    public class CompoundCollider
    {
        public List<Collider> Colliders { get; private set; }

        public CompoundCollider()
        {
            Colliders = new List<Collider>();
        }

        public CompoundCollider(IEnumerable<Collider> colliders)
        {
            Colliders = new List<Collider>(colliders);
        }
    }

    public class BoxCollider : Collider
    {
        public Vector3 Size { get; set; }
    }

    public class SphereCollider : Collider
    {
        public float Radius { get; set; }
    }

    public class CylinderCollider : Collider
    {
        public float Height { get; set; }
        public float Radius { get; set; }
    }

    public class CapsuleCollider : Collider
    {
        public float Height { get; set; }
        public float Radius { get; set; }
    }

    public class ConeCollider : Collider
    {
        public float Height { get; set; }
        public float Radius { get; set; }
    }
}
