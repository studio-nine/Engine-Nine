namespace Nine.Graphics
{
    using System;

    /// <summary>
    /// Represents an object whose value is version controlled. Each time the
    /// value changes, the corresponding version increments by one.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    public class Versioned<T> : IEquatable<Versioned<T>>
    {
        /// <summary>
        /// Gets or sets the value of this versioned object.
        /// </summary>
        public T Value
        {
            get { return value; }
            set { this.value = value; version++; }
        }
        internal T value;

        /// <summary>
        /// Gets the current version of this versioned object.
        /// </summary>
        public int Version
        {
            get { return version; }
        }
        internal int version;

        /// <summary>
        /// Initializes a new instance of the <see cref="Versioned&lt;T&gt;"/> class.
        /// </summary>
        public Versioned()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Versioned&lt;T&gt;"/> struct.
        /// </summary>
        public Versioned(T value)
        {
            this.value = value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Nine.Graphics.Versioned&lt;T&gt;"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator T(Versioned<T> value)
        {
            return value.value;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Versioned<T> other)
        {
            return value.Equals(other.value);
        }
        
        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Versioned<T>)
                return Equals((Versioned<T>)obj);
            if (obj is T)
                return Value.Equals((T)obj);
            return false;
        }
        
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Versioned<T> value1, Versioned<T> value2)
        {
            return value1.value.Equals(value2.value);
        }
        
        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Versioned<T> value1, Versioned<T> value2)
        {
            return !value1.value.Equals(value2.value);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} v{1}", value, version);
        }
    }
}
