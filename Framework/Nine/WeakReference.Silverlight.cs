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
#if SILVERLIGHT
    /// <summary>
    /// Represents a weak reference, which references an object while still allowing
    //  that object to be reclaimed by garbage collection.
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/questions/3231945/inherited-weakreference-throwing-reflectiontypeloadexception-in-silverlight
    /// </remarks>   
    public class WeakReference<T> where T : class 
    { 
        private WeakReference inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakReference&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public WeakReference(T target) 
            : this(target, false) 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakReference&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="trackResurrection">if set to <c>true</c> [track resurrection].</param>
        public WeakReference(T target, bool trackResurrection) 
        { 
            if(target == null) throw new ArgumentNullException("target"); 
            this.inner = new WeakReference((object)target, trackResurrection); 
        }

        public T Target 
        {
            get { return (T)this.inner.Target; }
            set { this.inner.Target = value; }
        } 
 
        public bool IsAlive 
        {
            get { return this.inner.IsAlive; }
        } 
    } 
#endif
}