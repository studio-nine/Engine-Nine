#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;

#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Represents the result of a scene query.
    /// </summary>
    public struct FindResult : IEquatable<FindResult>
    {
        /// <summary>
        /// Gets the target object found. This object equals to the object added to the scene.
        /// </summary>
        public object Target;

        /// <summary>
        /// Gets the original reporting source as determined by pure hit testing.
        /// </summary>
        public ISpatialQueryable OriginalTarget;

        /// <summary>
        /// Gets the distance from the ray original when doing ray casting.
        /// </summary>
        public float? Distance;

        /// <summary>
        /// Gets the containment type when finding objects from a bounding volumn.
        /// </summary>
        public ContainmentType ContainmentType;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(FindResult other)
        {
            return Target == other.Target && OriginalTarget == other.OriginalTarget &&
                   Distance == other.Distance && ContainmentType == other.ContainmentType;
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
            if (obj is FindResult)
                return Equals((FindResult)obj);

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
        public static bool operator ==(FindResult value1, FindResult value2)
        {
            return ((value1.Target == value2.Target) && (value1.OriginalTarget == value2.OriginalTarget) &&
                   (value1.Distance == value2.Distance) && (value1.ContainmentType == value2.ContainmentType));
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(FindResult value1, FindResult value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Target.GetHashCode() ^ OriginalTarget.GetHashCode();
        }
    }
}