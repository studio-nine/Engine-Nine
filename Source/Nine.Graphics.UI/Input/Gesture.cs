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

namespace Nine.Graphics.UI.Input
{
    using Microsoft.Xna.Framework;
    using System.Diagnostics;

    [DebuggerDisplay("{Type} @ P:{Vector2} D:{Delta}")]
    public struct Gesture
    {
        public Vector2 Delta;

        public Vector2 Vector2;

        public GestureType Type;

        public Gesture(GestureType type, Vector2 Vector2)
            : this(type, Vector2, Vector2.Zero)
        {
        }

        public Gesture(GestureType type, Vector2 Vector2, Vector2 delta)
        {
            this.Type = type;
            this.Vector2 = Vector2;
            this.Delta = delta;
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Vector2: {1}, Delta: {2}", this.Type, this.Vector2, this.Delta);
        }
    }
}
