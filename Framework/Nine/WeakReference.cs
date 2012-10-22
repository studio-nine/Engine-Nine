namespace Nine
{
    using System;

    /// <summary>
    /// Represents a weak reference, which references an object while still allowing
    //  that object to be reclaimed by garbage collection.
    /// </summary>
    /// <remarks>
    /// Vote for a generic version of WeakReference at
    /// http://connect.microsoft.com/VisualStudio/feedback/details/98270/make-a-generic-form-of-weakreference-weakreference-t-where-t-class
    /// </remarks>
    public class WeakReference<T> : System.WeakReference where T : class
    {
#if WINDOWS_PHONE
        /// <summary>
        /// Initializes a new instance of the <see cref="WeakReference&lt;T&gt;"/> class.
        /// </summary>
        [System.Security.SecuritySafeCritical]
        protected WeakReference() { }
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="WeakReference&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public WeakReference(object target) : base(target) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakReference&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="trackResurrection">if set to <c>true</c> [track resurrection].</param>
        public WeakReference(object target, bool trackResurrection) : base(target, trackResurrection) { }

        public void SetTarget(T target)
        {
            base.Target = target;
        }

        public bool TryGetTarget(out T target)
        {
            target = base.Target as T;
            return target != null;
        }
    }
}