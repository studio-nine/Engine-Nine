#region License
/* The MIT License
 *
 * Copyright (c) 2013 Engine Nine
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
    public enum Direction
    {
        Left,
        Top,
        Right,
        Bottom,
    }

    public enum GridUnitType
    {
        Auto,
        Pixel,
        Star
    }

    /// <summary>
    /// Specifies if the text should wrap.
    /// </summary>
    public enum TextWrapping
    {
        NoWrap,
        Wrap,
        WrapWithOverflow
    }

    /// <summary>
    /// Specifies the display state of an element.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Display the element.
        /// </summary>
        Visible = 0,
        /// <summary>
        /// Do not display the element, but reserve space for the element in layout.
        /// </summary>
        Hidden = 1,
        /// <summary>
        /// Do not display the element, and do not reserve space for it in layout.
        /// </summary>
        Collapsed = 2,
    }

    /// <summary>
    /// Describes how the element is aligned horizontally within it's parent element.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// Stretches to fill the parent's layout slot.
        /// </summary>
        Stretch,
        /// <summary>
        /// Element is aligned to the left of the parent's layout slot. 
        /// </summary>
        Left,
        /// <summary>
        /// Element is aligned to the center of the parent's layout slot. 
        /// </summary>
        Center,
        /// <summary>
        /// Element is aligned to the right of the parent's layout slot. 
        /// </summary>
        Right
    }

    /// <summary>
    /// Describes how the element is aligned horizontally within it's parent element.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// Stretches to fill the parent's layout slot.
        /// </summary>
        Stretch,
        /// <summary>
        /// Element is aligned to the top of the parent's layout slot. 
        /// </summary> 
        Top,
        /// <summary>
        /// Element is aligned to the center of the parent's layout slot. 
        /// </summary>
        Center,
        /// <summary>
        /// Element is aligned to the bottom of the parent's layout slot. 
        /// </summary> 
        Bottom
    }
}
