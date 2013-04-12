namespace Nine.Serialization
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel;

    class SerializationOverride : ISerializationOverride
    {
        private Dictionary<WeakReference, object> overrides;

        public SerializationOverride()
        {
            overrides = new Dictionary<WeakReference, object>(new WeakReferenceEqualtyComparer());
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
    }
}