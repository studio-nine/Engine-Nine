namespace Nine.Serialization
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel;

    class SerializationOverride : ISerializationOverride, IEqualityComparer<WeakReference>
    {
        private Dictionary<WeakReference, object> overrides;

        public SerializationOverride()
        {
            overrides = new Dictionary<WeakReference, object>(this);
        }

        public bool RemoveOverride(object target)
        {
            return overrides.Remove(new WeakReference(target));
        }

        public void SetOverride(object target, object targetOverride)
        {
            overrides[new WeakReference(target)] = targetOverride;
        }

        public bool TryGetOverride(object target, out object targetOverride)
        {
            return overrides.TryGetValue(new WeakReference(target), out targetOverride);
        }

        bool IEqualityComparer<WeakReference>.Equals(WeakReference x, WeakReference y)
        {
            var xx = x.Target;
            var yy = y.Target;
            return xx != null && yy != null && xx == yy;
        }

        int IEqualityComparer<WeakReference>.GetHashCode(WeakReference obj)
        {
            var target = obj.Target;
            return target != null ? target.GetHashCode() : 0;
        }
    }
}