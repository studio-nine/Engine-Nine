// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Define the collection of parameters for SilverlightEffect class.
    /// </summary>
    public sealed class EffectParametersCollection : IEnumerable<EffectParameter>
    {
        private readonly List<EffectParameter> parameters;

        /// <summary>Gets a specific SilverlightEffectParameter object by using an index value.</summary>
        /// <param name="index">Index of the SilverlightEffectParameter to get.</param>
        public EffectParameter this[int index]
        {
            get
            {
                if (index >= 0 && index < parameters.Count)
                {
                    return parameters[index];
                }
                return null;
            }
        }

        /// <summary>Gets a specific SilverlightEffectParameter by name.</summary>
        /// <param name="name">The name of the SilverlightEffectParameter to retrieve.</param>
        public EffectParameter this[string name]
        {
            get
            {
                return parameters.FirstOrDefault(current => current.Name == name);
            }
        }

        /// <summary>Gets the number of EffectParameter objects in this EffectParameterCollection.</summary>
        public int Count
        {
            get
            {
                return parameters.Count;
            }
        }

        internal EffectParametersCollection(IEnumerable<EffectParameter> sourceParameters)
        {
            parameters = new List<EffectParameter>(sourceParameters);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        IEnumerator<EffectParameter> IEnumerable<EffectParameter>.GetEnumerator()
        {
            return parameters.GetEnumerator();
        }
    }
}
