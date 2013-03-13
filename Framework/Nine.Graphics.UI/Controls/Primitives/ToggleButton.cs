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

namespace Nine.Graphics.UI.Controls.Primitives
{
    using System;

    /// <summary>
    ///     Base class for all controls with toggling functionality.
    /// </summary>
    public class ToggleButton : ButtonBase
    {
        #region Properties / Fields

        /// <summary>
        ///     Gets or sets a value indicating whether a <see cref = "ToggleButton">ToggleButton</see> is in a checked, unchecked or indeterminate state.
        /// </summary>
        public bool? IsChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a <see cref = "ToggleButton">ToggleButton</see> supports two or three states.
        /// </summary>
        /// <remarks>
        ///     If false, <see cref = "IsChecked">IsChecked</see> can only be true or false.  If true, <see cref = "IsChecked">IsChecked</see> can enter a third null state.
        /// </remarks>
        public bool IsThreeState { get; set; }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a <see cref = "ToggleButton">ToggleButton</see> is checked.
        /// </summary>
        public event EventHandler<EventArgs> Checked;

        /// <summary>
        ///     Occurs when the state of a <see cref = "ToggleButton">ToggleButton</see> is neither checked nor unchecked.
        /// </summary>
        /// <remarks>
        ///     This can only occur when <see cref = "IsThreeState">IsThreeState</see> is true.
        /// </remarks>
        public event EventHandler<EventArgs> Indeterminate;

        /// <summary>
        ///     Occurs when a <see cref = "ToggleButton">ToggleButton</see> is unchecked.
        /// </summary>
        public event EventHandler<EventArgs> Unchecked;

        #endregion

        #region Methods

        protected internal virtual void OnToggle()
        {
            bool? isChecked = this.IsChecked;

            if (isChecked == true)
            {
                this.IsChecked = this.IsThreeState ? (bool?)null : false;
            }
            else
            {
                this.IsChecked = new bool?(isChecked.HasValue);
            }
        }

        protected virtual void OnChecked()
        {
            EventHandler<EventArgs> eventHandler = this.Checked;
            if (eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        protected override void OnClick()
        {
            this.OnToggle();
            base.OnClick();
        }

        protected virtual void OnIndeterminate()
        {
            EventHandler<EventArgs> eventHandler = this.Indeterminate;
            if (eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUnchecked()
        {
            EventHandler<EventArgs> eventHandler = this.Unchecked;
            if (eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
