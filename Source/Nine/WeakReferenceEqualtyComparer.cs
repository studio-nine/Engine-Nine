namespace Nine
{
    using System;
    using System.Collections.Generic;

    class WeakReferenceEqualtyComparer : IEqualityComparer<WeakReference>
    {
        public bool Equals(WeakReference x, WeakReference y)
        {
            var xx = x.Target;
            var yy = y.Target;
            return xx != null && yy != null && xx.Equals(yy);
        }

        public int GetHashCode(WeakReference obj)
        {
            var oo = obj.Target;
            return oo != null ? oo.GetHashCode() : int.MinValue;
        }
    }
}