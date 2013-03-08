#region License
/* The MIT License
 *
 * Copyright (c) 2011 Red Badger Consulting
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/
#endregion

namespace Nine.Graphics.UI
{
#if WINDOWS_PHONE

	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Nine.Graphics.UI.Data;
	using Nine.Graphics.UI.Internal;

#else
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nine.Graphics.UI.Internal;
#endif

    /// <summary>
    ///     Represents an object that participates in the Reactive Property system.
    /// </summary>
    public class ReactiveObject : Nine.Object
    {
        private readonly Dictionary<object, object> propertyValues =
            new Dictionary<object, object>();
        
        public void ClearValue<T>(ReactiveProperty<T> property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            this.propertyValues.Remove(property);
        }

        public T GetValue<T>(ReactiveProperty<T> property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            object result;
            if (this.propertyValues.TryGetValue(property, out result))
                return (T)result;
            return property.DefaultValue;
        }

        public void SetValue<T>(ReactiveProperty<T> property, T newValue)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            object result;
            if (!this.propertyValues.TryGetValue(property, out result))
            {
                this.propertyValues.Add(property, newValue);
                if (!newValue.Equals(property.DefaultValue))
                    property.ChangedCallback(this, new ReactivePropertyChangeEventArgs<T>(property, property.DefaultValue, newValue));
            }
            else
            {
                T oldValue = (T)result;
                this.propertyValues[property] = newValue;
                if (!newValue.Equals(oldValue))
                    property.ChangedCallback(this, new ReactivePropertyChangeEventArgs<T>(property, oldValue, newValue));
            }
        }
    }
}
