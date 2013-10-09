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
    /// <summary>
    /// Defines Row properties that apply to <see cref="Nine.Graphics.UI.Controls.Grid"/>.
    /// </summary>
    public class RowDefinition : DefinitionBase
    {
        /// <summary>
        /// Gets or sets height of the Column
        /// </summary>
        public GridLength Height { get; set; }

        /// <summary>
        /// Gets or sets maximum height of the Column
        /// </summary>
        public float MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets minimum height of the Column
        /// </summary>
        public float MinHeight { get; set; }

        /// <summary>
        /// Constructs with 'Height 1' and 'Unit Type Star'.
        /// </summary>
        public RowDefinition() : this(1, GridUnitType.Star) { }

        /// <summary>
        /// Constructs with 'Unit Type Star'.
        /// </summary>
        /// <param name="value">Height</param>
        public RowDefinition(float value) : this(value, GridUnitType.Star) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Height</param>
        /// <param name="type">Unit Type</param>
        public RowDefinition(float value, GridUnitType type)
            : base(DefinitionType.Row)
        {
            Height = new GridLength(value, type);
            MaxHeight = float.PositiveInfinity;
            MinHeight = 0;
        }
    }
}
