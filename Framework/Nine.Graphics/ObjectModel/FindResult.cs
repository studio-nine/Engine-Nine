#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Effects;
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
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(FindResult other)
        {
            return Target == other.Target && OriginalTarget == other.OriginalTarget && Distance == other.Distance;
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

        public static bool operator ==(FindResult value1, FindResult value2)
        {
            return ((value1.Target == value2.Target) && (value1.OriginalTarget == value2.OriginalTarget) && (value1.Distance == value2.Distance));
        }

        public static bool operator !=(FindResult value1, FindResult value2)
        {
            return !(value1 == value2);
        }

        public override int GetHashCode()
        {
            return Target.GetHashCode() + OriginalTarget.GetHashCode();
        }
    }
}