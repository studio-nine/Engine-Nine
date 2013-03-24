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

namespace Nine.Graphics.UI.Internal.Controls
{
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Media;

    internal class Viewbox
    {
        internal static Vector2 ComputeScaleFactor(
            Vector2 availableSize, Vector2 contentSize, Stretch stretch, StretchDirection stretchDirection)
        {
            float scaleX = 1.0f;
            float scaleY = 1.0f;
            bool isWidthContrained = !float.IsPositiveInfinity(availableSize.X);
            bool isHeightConstrained = !float.IsPositiveInfinity(availableSize.Y);
            if (stretch == Stretch.None || (!isWidthContrained && !isHeightConstrained))
            {
                return new Vector2(scaleX, scaleY);
            }

            scaleX = contentSize.X.IsCloseTo(0) ? 0 : (availableSize.X / contentSize.X);
            scaleY = contentSize.Y.IsCloseTo(0) ? 0 : (availableSize.Y / contentSize.Y);
            if (!isWidthContrained)
            {
                scaleX = scaleY;
            }
            else if (!isHeightConstrained)
            {
                scaleY = scaleX;
            }
            else
            {
                switch (stretch)
                {
                    case Stretch.Fill:
                        return new Vector2(availableSize.X / contentSize.X, availableSize.Y / contentSize.Y);

                    case Stretch.Uniform:
                        scaleX = scaleY = (scaleX < scaleY) ? scaleX : scaleY;
                        break;

                    case Stretch.UniformToFill:
                        scaleX = scaleY = (scaleX > scaleY) ? scaleX : scaleY;
                        break;
                }
            }

            switch (stretchDirection)
            {
                case StretchDirection.UpOnly:
                    if (scaleX < 1.0f)
                    {
                        scaleX = 1.0f;
                    }

                    if (scaleY < 1.0f)
                    {
                        scaleY = 1.0f;
                    }

                    break;

                case StretchDirection.DownOnly:
                    if (scaleX > 1.0f)
                    {
                        scaleX = 1.0f;
                    }

                    if (scaleY > 1.0f)
                    {
                        scaleY = 1.0f;
                    }

                    break;
            }

            return new Vector2(scaleX, scaleY);
        }
    }
}
