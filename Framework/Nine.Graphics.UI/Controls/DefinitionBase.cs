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

namespace Nine.Graphics.UI.Controls
{
    using System;

    public abstract class DefinitionBase
    {
        protected enum DefinitionType { Column, Row }

        #region Properties / Fields

        internal GridLength UserLength
        {
            get
            {
                return this.definitionType == DefinitionType.Column
                           ? GetValue<GridLength>("Width") : GetValue<GridLength>("Height");
            }
        }
        internal float UserMaxLength
        {
            get
            {
                return this.definitionType == DefinitionType.Column
                           ? GetValue<float>("MaxWidth") : GetValue<float>("MaxHeight");
            }
        }
        internal float UserMinLength
        {
            get
            {
                return this.definitionType == DefinitionType.Column
                           ? GetValue<float>("MinWidth") : GetValue<float>("MinHeight");
            }
        }

        private readonly DefinitionType definitionType;

        internal float AvailableLength { get; set; }
        internal float Denominator { get; set; }
        internal float FinalLength { get; set; }
        internal float FinalOffset { get; set; }
        internal GridUnitType LengthType { get; set; }
        internal float MinLength { get; set; }
        internal float Numerator { get; set; }
        internal float StarAllocationOrder { get; set; }

        #endregion

        protected DefinitionBase(DefinitionType definitionType)
        {
            this.definitionType = definitionType;
        }

        #region Methods

        internal void UpdateMinLength(float minLength)
        {
            this.MinLength = Math.Max(this.MinLength, minLength);
        }

        private T GetValue<T>(string Name) where T : struct
        {
            var Property = this.GetType().GetProperty(Name);
            if (Property == null)
                throw new ArgumentNullException("name");
            return (T)Property.GetValue(this, null);
        }

        #endregion
    }
}
