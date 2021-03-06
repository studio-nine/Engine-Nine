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
    /// Defines Column properties that apply to <see cref="Nine.Graphics.UI.Controls.Grid"/>.
    /// </summary>
    public class ColumnDefinition : DefinitionBase
    {
        /// <summary>
        /// Gets or sets width of the Column
        /// </summary>
        public GridLength Width { get; set; }

        /// <summary>
        /// Gets or sets maximum width of the Column
        /// </summary>
        public float MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets minimum width of the Column
        /// </summary>
        public float MinWidth { get; set; }

        #region Ctor

        /// <summary>
        /// Constructs with 'Width 1' and 'Unit Type Star'.
        /// </summary>
        public ColumnDefinition() : this(1, GridUnitType.Star) { }

        /// <summary>
        /// Constructs with 'Unit Type Star'.
        /// </summary>
        /// <param name="value">Width</param>
        public ColumnDefinition(float value) : this(value, GridUnitType.Star) { }

        /// <summary>
        /// Constructs with type and value 0.
        /// </summary>
        /// <param name="type"></param>
        public ColumnDefinition(GridUnitType type) 
            : this(0, type)
        {
            // Designed to make it easier to create with type 'Auto' in code
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Width</param>
        /// <param name="type">Unit Type</param>
        public ColumnDefinition(float value, GridUnitType type)
            : base(DefinitionType.Column)
        {
            Width = new GridLength(value, type);
            MaxWidth = float.PositiveInfinity;
            MinWidth = 0;
        }

        #endregion
    }
}
