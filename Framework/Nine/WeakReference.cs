#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.Security;

namespace Nine
{
    /// <summary>
    /// Represents a weak reference, which references an object while still allowing
    //  that object to be reclaimed by garbage collection.
    /// </summary>
    /// <remarks>
    /// Vote for a generic version of WeakReference at
    /// http://connect.microsoft.com/VisualStudio/feedback/details/98270/make-a-generic-form-of-weakreference-weakreference-t-where-t-class
    /// </remarks>
#if WINDOWS
    [Serializable]
#endif
    public class WeakReference<T> : System.WeakReference where T : class
    {
#if WINDOWS_PHONE
        /// <summary>
        /// Initializes a new instance of the <see cref="WeakReference&lt;T&gt;"/> class.
        /// </summary>
        [SecuritySafeCritical]
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

        /// <summary>
        /// Gets or sets the object (the target) referenced by the current <see cref="T:System.WeakReference"/> object.
        /// </summary>
        /// <returns>null if the object referenced by the current <see cref="T:System.WeakReference"/> object has been garbage collected; otherwise, a reference to the object referenced by the current <see cref="T:System.WeakReference"/> object.</returns>
        ///   
        /// <exception cref="T:System.InvalidOperationException">The reference to the target object is invalid. This exception can be thrown while setting this property if the value is a null reference or if the object has been finalized during the set operation.</exception>
        public new T Target
        {
            get { return (T)base.Target; }
            set { base.Target = value; }
        }
    }
}